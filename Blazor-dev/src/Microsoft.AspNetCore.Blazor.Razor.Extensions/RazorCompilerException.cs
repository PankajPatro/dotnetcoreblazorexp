﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Razor.Language;
using System;

namespace Microsoft.AspNetCore.Blazor.Razor
{
    /// <summary>
    /// Represents a fatal error during the transformation of a Blazor component from
    /// Razor source code to C# source code.
    /// </summary>
    public class RazorCompilerException : Exception
    {
        public RazorCompilerException(RazorDiagnostic diagnostic)
        {
            Diagnostic = diagnostic;
        }

        public RazorDiagnostic Diagnostic { get; }
    }
}
