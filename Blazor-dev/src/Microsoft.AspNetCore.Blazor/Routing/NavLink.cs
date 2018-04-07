﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Blazor.Components;
using Microsoft.AspNetCore.Blazor.RenderTree;
using Microsoft.AspNetCore.Blazor.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.AspNetCore.Blazor.Routing
{
    // NOTE: This could be implemented in a more performant way by iterating through
    // the ParameterCollection only once (instead of multiple TryGetValue calls), and
    // avoiding allocating a dictionary in the case where there are no additional params.
    // However the intention here is to get a sense of what more high-level coding patterns
    // will exist and what APIs are needed to support them. Later in the project when we
    // have more examples of components implemented in pure C# (not Razor) we could change
    // this one to the more low-level perf-sensitive implementation.

    /// <summary>
    /// A component that renders an anchor tag, automatically toggling its 'active'
    /// class based on whether its 'href' matches the current URI.
    /// </summary>
    public class NavLink : IComponent, IDisposable
    {
        private const string DefaultActiveClass = "active";

        private RenderHandle _renderHandle;
        private bool _isActive;

        private RenderFragment _childContent;
        private string _cssClass;
        private string _hrefAbsolute;
        private IReadOnlyDictionary<string, object> _allAttributes;

        /// <summary>
        /// Gets or sets the CSS class name applied to the NavLink when the 
        /// current route matches the NavLink href.
        /// </summary>
        public string ActiveClass { get; set; }

        /// <summary>
        /// Gets or sets a value representing the URL matching behavior.
        /// </summary>
        public NavLinkMatch Match { get; set; }

        [Inject] private IUriHelper UriHelper { get; set; }

        /// <inheritdoc />
        public void Init(RenderHandle renderHandle)
        {
            _renderHandle = renderHandle;

            // We'll consider re-rendering on each location change
            UriHelper.OnLocationChanged += OnLocationChanged;
        }

        /// <inheritdoc />
        public void SetParameters(ParameterCollection parameters)
        {
            // Capture the parameters we want to do special things with, plus all as a dictionary
            parameters.TryGetValue(RenderTreeBuilder.ChildContent, out _childContent);
            parameters.TryGetValue("class", out _cssClass);
            parameters.TryGetValue("href", out string href);
            ActiveClass = parameters.GetValueOrDefault(nameof(ActiveClass), DefaultActiveClass);
            Match = parameters.GetValueOrDefault(nameof(Match), NavLinkMatch.Prefix);
            _allAttributes = parameters.ToDictionary();

            // Update computed state and render
            _hrefAbsolute = href == null ? null : UriHelper.ToAbsoluteUri(href).AbsoluteUri;
            _isActive = ShouldMatch(UriHelper.GetAbsoluteUri());
            _renderHandle.Render(Render);
        }

        public void Dispose()
        {
            // To avoid leaking memory, it's important to detach any event handlers in Dispose()
            UriHelper.OnLocationChanged -= OnLocationChanged;
        }

        private void OnLocationChanged(object sender, string newUriAbsolute)
        {
            // We could just re-render always, but for this component we know the
            // only relevant state change is to the _isActive property.
            var shouldBeActiveNow = ShouldMatch(newUriAbsolute);
            if (shouldBeActiveNow != _isActive)
            {
                _isActive = shouldBeActiveNow;
                _renderHandle.Render(Render);
            }
        }

        private bool ShouldMatch(string currentUriAbsolute)
        {
            if (Match == NavLinkMatch.Prefix)
            {
                return StartsWithAndHasSeparator(currentUriAbsolute, _hrefAbsolute);
            }
            else if (Match == NavLinkMatch.All)
            {
                return string.Equals(currentUriAbsolute, _hrefAbsolute, StringComparison.Ordinal);
            }
            else
            {
                throw new InvalidOperationException($"Unsupported {nameof(NavLinkMatch)} value: {Match}");
            }
        }

        private void Render(RenderTreeBuilder builder)
        {
            builder.OpenElement(0, "a");

            // Set class attribute
            builder.AddAttribute(0, "class",
                CombineWithSpace(_cssClass, _isActive ? ActiveClass : null));

            // Pass through all other attributes unchanged
            foreach (var kvp in _allAttributes.Where(kvp => kvp.Key != "class" && kvp.Key != nameof(RenderTreeBuilder.ChildContent)))
            {
                builder.AddAttribute(0, kvp.Key, kvp.Value);
            }

            // Pass through any child content unchanged
            builder.AddContent(1, _childContent);

            builder.CloseElement();
        }

        private string CombineWithSpace(string str1, string str2)
            => str1 == null ? str2
            : (str2 == null ? str1 : $"{str1} {str2}");

        private static bool StartsWithAndHasSeparator(string value, string prefix)
        {
            var valueLength = value.Length;
            var prefixLength = prefix.Length;
            if (prefixLength == valueLength)
            {
                return string.Equals(value, prefix, StringComparison.Ordinal);
            }
            else if (valueLength > prefixLength)
            {
                return value.StartsWith(prefix, StringComparison.Ordinal)
                    && (
                        // Only match when there's a separator character either at the end of the
                        // prefix or right after it.
                        // Example: "/abc" is treated as a prefix of "/abc/def" but not "/abcdef"
                        // Example: "/abc/" is treated as a prefix of "/abc/def" but not "/abcdef"
                        prefixLength == 0
                        || !char.IsLetterOrDigit(prefix[prefixLength - 1])
                        || !char.IsLetterOrDigit(value[prefixLength])
                    );
            }
            else
            {
                return false;
            }
        }
    }
}
