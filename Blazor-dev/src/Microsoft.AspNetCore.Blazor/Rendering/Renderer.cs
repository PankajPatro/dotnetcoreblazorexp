﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Blazor.Components;
using Microsoft.AspNetCore.Blazor.RenderTree;
using System;
using System.Collections.Generic;

namespace Microsoft.AspNetCore.Blazor.Rendering
{
    /// <summary>
    /// Provides mechanisms for rendering hierarchies of <see cref="IComponent"/> instances,
    /// dispatching events to them, and notifying when the user interface is being updated.
    /// </summary>
    public abstract class Renderer
    {
        private readonly ComponentFactory _componentFactory;
        private int _nextComponentId = 0; // TODO: change to 'long' when Mono .NET->JS interop supports it
        private readonly Dictionary<int, ComponentState> _componentStateById
            = new Dictionary<int, ComponentState>();

        private readonly RenderBatchBuilder _batchBuilder = new RenderBatchBuilder();
        private bool _isBatchInProgress;

        private int _lastEventHandlerId = 0;
        private readonly Dictionary<int, UIEventHandler> _eventHandlersById
            = new Dictionary<int, UIEventHandler>();

        /// <summary>
        /// Constructs an instance of <see cref="Renderer"/>.
        /// </summary>
        /// <param name="serviceProvider">The <see cref="IServiceProvider"/> to be used when initialising components.</param>
        public Renderer(IServiceProvider serviceProvider)
        {
            _componentFactory = new ComponentFactory(serviceProvider);
        }

        /// <summary>
        /// Constructs a new component of the specified type.
        /// </summary>
        /// <param name="componentType">The type of the component to instantiate.</param>
        /// <returns>The component instance.</returns>
        protected IComponent InstantiateComponent(Type componentType)
            => _componentFactory.InstantiateComponent(componentType);

        /// <summary>
        /// Associates the <see cref="IComponent"/> with the <see cref="Renderer"/>, assigning
        /// an identifier that is unique within the scope of the <see cref="Renderer"/>.
        /// </summary>
        /// <param name="component">The component.</param>
        /// <returns>The component's assigned identifier.</returns>
        protected int AssignComponentId(IComponent component)
        {
            var componentId = _nextComponentId++;
            var componentState = new ComponentState(this, componentId, component);
            _componentStateById.Add(componentId, componentState);
            component.Init(new RenderHandle(this, componentId));
            return componentId;
        }

        /// <summary>
        /// Updates the visible UI.
        /// </summary>
        /// <param name="renderBatch">The changes to the UI since the previous call.</param>
        protected abstract void UpdateDisplay(RenderBatch renderBatch);

        /// <summary>
        /// Notifies the specified component that an event has occurred.
        /// </summary>
        /// <param name="componentId">The unique identifier for the component within the scope of this <see cref="Renderer"/>.</param>
        /// <param name="eventHandlerId">The <see cref="RenderTreeFrame.AttributeEventHandlerId"/> value from the original event attribute.</param>
        /// <param name="eventArgs">Arguments to be passed to the event handler.</param>
        protected void DispatchEvent(int componentId, int eventHandlerId, UIEventArgs eventArgs)
        {
            if (_eventHandlersById.TryGetValue(eventHandlerId, out var handler))
            {
                // The event handler might request multiple renders in sequence. Capture them
                // all in a single batch.
                try
                {
                    _isBatchInProgress = true;
                    GetRequiredComponentState(componentId).DispatchEvent(handler, eventArgs);
                }
                finally
                {
                    _isBatchInProgress = false;
                    ProcessRenderQueue();
                }
            }
            else
            {
                throw new ArgumentException($"There is no event handler with ID {eventHandlerId}");
            }
        }

        internal void InstantiateChildComponentOnFrame(ref RenderTreeFrame frame)
        {
            if (frame.FrameType != RenderTreeFrameType.Component)
            {
                throw new ArgumentException($"The frame's {nameof(RenderTreeFrame.FrameType)} property must equal {RenderTreeFrameType.Component}", nameof(frame));
            }

            if (frame.Component != null)
            {
                throw new ArgumentException($"The frame already has a non-null component instance", nameof(frame));
            }

            var newComponent = InstantiateComponent(frame.ComponentType);
            var newComponentId = AssignComponentId(newComponent);
            frame = frame.WithComponentInstance(newComponentId, newComponent);
        }

        internal void AssignEventHandlerId(ref RenderTreeFrame frame)
        {
            var id = ++_lastEventHandlerId;
            _eventHandlersById.Add(id, (UIEventHandler)frame.AttributeValue);
            frame = frame.WithAttributeEventHandlerId(id);
        }

        internal void AddToRenderQueue(int componentId, RenderFragment renderFragment)
        {
            var componentState = GetOptionalComponentState(componentId);
            if (componentState == null)
            {
                // If the component was already disposed, then its render handle trying to
                // queue a render is a no-op.
                return;
            }

            _batchBuilder.ComponentRenderQueue.Enqueue(
                new RenderQueueEntry(componentState, renderFragment));

            if (!_isBatchInProgress)
            {
                ProcessRenderQueue();
            }
        }

        private ComponentState GetRequiredComponentState(int componentId)
            => _componentStateById.TryGetValue(componentId, out var componentState)
                ? componentState
                : throw new ArgumentException($"The renderer does not have a component with ID {componentId}.");

        private ComponentState GetOptionalComponentState(int componentId)
            => _componentStateById.TryGetValue(componentId, out var componentState)
                ? componentState
                : null;

        private void ProcessRenderQueue()
        {
            _isBatchInProgress = true;

            try
            {
                // Process render queue until empty
                while (_batchBuilder.ComponentRenderQueue.Count > 0)
                {
                    var nextToRender = _batchBuilder.ComponentRenderQueue.Dequeue();
                    RenderInExistingBatch(nextToRender);
                }

                UpdateDisplay(_batchBuilder.ToBatch());
            }
            finally
            {
                RemoveEventHandlerIds(_batchBuilder.DisposedEventHandlerIds.ToRange());
                _batchBuilder.Clear();
                _isBatchInProgress = false;
            }
        }

        private void RenderInExistingBatch(RenderQueueEntry renderQueueEntry)
        {
            renderQueueEntry.ComponentState
                .RenderIntoBatch(_batchBuilder, renderQueueEntry.RenderFragment);

            // Process disposal queue now in case it causes further component renders to be enqueued
            while (_batchBuilder.ComponentDisposalQueue.Count > 0)
            {
                var disposeComponentId = _batchBuilder.ComponentDisposalQueue.Dequeue();
                GetRequiredComponentState(disposeComponentId).DisposeInBatch(_batchBuilder);
                _componentStateById.Remove(disposeComponentId);
                _batchBuilder.DisposedComponentIds.Append(disposeComponentId);
            }
        }

        private void RemoveEventHandlerIds(ArrayRange<int> eventHandlerIds)
        {
            var array = eventHandlerIds.Array;
            var count = eventHandlerIds.Count;
            for (var i = 0; i < count; i++)
            {
                _eventHandlersById.Remove(array[i]);
            }
        }
    }
}
