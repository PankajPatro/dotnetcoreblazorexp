﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Blazor.E2ETest.Infrastructure.ServerFixtures;
using System;
using Xunit;

namespace Microsoft.AspNetCore.Blazor.E2ETest.Infrastructure
{
    public abstract class ServerTestBase<TServerFixture>
        : BrowserTestBase, IClassFixture<TServerFixture>
        where TServerFixture: ServerFixture
    {
        private readonly TServerFixture _serverFixture;

        public ServerTestBase(BrowserFixture browserFixture, TServerFixture serverFixture)
            : base(browserFixture)
        {
            _serverFixture = serverFixture;
        }

        public void Navigate(string relativeUrl, bool noReload = false)
        {
            var absoluteUrl = new Uri(_serverFixture.RootUri, relativeUrl);

            if (noReload)
            {
                var existingUrl = Browser.Url;
                if (string.Equals(existingUrl, absoluteUrl.AbsoluteUri, StringComparison.Ordinal))
                {
                    return;
                }
            }

            Browser.Navigate().GoToUrl(absoluteUrl);
        }
    }
}
