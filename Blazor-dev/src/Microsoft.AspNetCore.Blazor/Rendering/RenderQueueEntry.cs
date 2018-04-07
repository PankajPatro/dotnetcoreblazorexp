﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Blazor.RenderTree;
using System;

namespace Microsoft.AspNetCore.Blazor.Rendering
{
    internal readonly struct RenderQueueEntry
    {
        public readonly ComponentState ComponentState;
        public readonly RenderFragment RenderFragment;

        public RenderQueueEntry(ComponentState componentState, RenderFragment renderFragment)
        {
            ComponentState = componentState;
            RenderFragment = renderFragment ?? throw new ArgumentNullException(nameof(renderFragment));
        }
    }
}
