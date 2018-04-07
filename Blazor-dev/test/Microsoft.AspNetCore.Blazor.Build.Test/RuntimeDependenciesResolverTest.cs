﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Linq;
using Xunit;

namespace Microsoft.AspNetCore.Blazor.Build.Test
{
    public class RuntimeDependenciesResolverTest
    {
        [Fact]
        public void FindsReferenceAssemblyGraph_ForStandaloneApp()
        {
            // Arrange
            var standaloneAppAssembly = typeof(StandaloneApp.Program).Assembly;
            var mainAssemblyLocation = standaloneAppAssembly.Location;
            // This list of hints is populated by MSBuild so it will be on the output
            // folder.
            var hintPaths = File.ReadAllLines(Path.Combine(
                Path.GetDirectoryName(mainAssemblyLocation),
                "referenceHints.txt"));
            var references = new[]
            {
                "Microsoft.AspNetCore.Blazor.Browser.dll",
                "Microsoft.AspNetCore.Blazor.dll",
                "Microsoft.Extensions.DependencyInjection.Abstractions.dll",
                "Microsoft.Extensions.DependencyInjection.dll"
            }.Select(a => hintPaths.Single(p => Path.GetFileName(p) == a))
            .ToArray();

            var basePath = Path.GetDirectoryName(typeof(RuntimeDependenciesResolverTest).Assembly.Location);
            var bclLocations = new []
            {
                Path.Combine(basePath, "../../../../../src/mono/dist/optimized/bcl/"),
                Path.Combine(basePath, "../../../../../src/mono/dist/optimized/bcl/Facades/"),
            };

            var expectedContents = new[]
            {
                /*
                 The current Mono WASM BCL forwards from netstandard.dll to various facade assemblies
                 in which small bits of implementation live, such as System.Xml.XPath.XDocument. So
                 if you reference netstandard, then you also reference System.Xml.XPath.XDocument.dll,
                 even though you're very unlikely to be calling it at runtime. That's why the following
                 list (for a very basic Blazor app) is longer than you'd expect.

                 These redundant references could be stripped out during publishing, but it's still
                 unfortunate that in development mode you'd see all these unexpected assemblies get
                 fetched from the server. We should try to get the Mono WASM BCL reorganized so that
                 all the implementation goes into mscorlib.dll, with the facade assemblies existing only
                 in case someone (or some 3rd party assembly) references them directly, but with their
                 implementations 100% forwarding to mscorlib.dll. Then in development you'd fetch far
                 fewer assemblies from the server, and during publishing, illink would remove all the
                 uncalled implementation code from mscorlib.dll anyway.
                 */
                "Microsoft.AspNetCore.Blazor.Browser.dll",
                "Microsoft.AspNetCore.Blazor.dll",
                "Microsoft.Extensions.DependencyInjection.Abstractions.dll",
                "Microsoft.Extensions.DependencyInjection.dll",
                "Mono.Security.dll",
                "mscorlib.dll",
                "netstandard.dll",
                "StandaloneApp.dll",
                "System.ComponentModel.Composition.dll",
                "System.Core.dll",
                "System.Data.dll",
                "System.Diagnostics.StackTrace.dll",
                "System.dll",
                "System.Drawing.dll",
                "System.Globalization.Extensions.dll",
                "System.IO.Compression.dll",
                "System.IO.Compression.FileSystem.dll",
                "System.Net.Http.dll",
                "System.Numerics.dll",
                "System.Runtime.Serialization.dll",
                "System.Runtime.Serialization.Primitives.dll",
                "System.Runtime.Serialization.Xml.dll",
                "System.Security.Cryptography.Algorithms.dll",
                "System.Security.SecureString.dll",
                "System.ServiceModel.Internals.dll",
                "System.Transactions.dll",
                "System.Web.Services.dll",
                "System.Xml.dll",
                "System.Xml.Linq.dll",
                "System.Xml.XPath.XDocument.dll",
            }.OrderBy(i => i, StringComparer.Ordinal)
            .ToArray();

            // Act

            var paths = RuntimeDependenciesResolver
                .ResolveRuntimeDependenciesCore(
                    mainAssemblyLocation,
                    references,
                    bclLocations);

            var contents = paths
                .Select(p => Path.GetFileName(p))
                .OrderBy(i => i, StringComparer.Ordinal)
                .ToArray();

            // Assert
            Assert.Equal(expectedContents, contents);
        }
    }
}
