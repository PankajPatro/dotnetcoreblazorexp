# dotnetcoreblazorexp
Making Blazor Work with .Net Core

This is an experiment to test Blazor and Work in .Net core in Vs Code in Ubuntu

This code has been taken as is from official asp.net core blazor git https://github.com/aspnet/Blazor

I cloned the solution and changed the solution to build for dotnet core in ubuntu using VS Code.

Currently facing some issues. Need to see and fix the same.

Plan is to make the StandaloneApp in Samples to work in Ubuntu With Dotnet Core using VSCode.

Current Error - 

Exception has occurred: CLR/System.TypeLoadException
An unhandled exception of type 'System.TypeLoadException' occurred in Microsoft.AspNetCore.Blazor.Browser.dll: 'Generic method or method in generic class is internal call, PInvoke, or is defined in a COM Import class.'
   at Microsoft.AspNetCore.Blazor.Browser.Interop.RegisteredFunction.InvokeUnmarshalled[T0,T1,T2,TRes](String identifier, T0 arg0, T1 arg1, T2 arg2) in /home/mikun/LearningCode/CSharpBlazor/dotnetcoreblazorexp/Blazor-dev/src/Microsoft.AspNetCore.Blazor.Browser/Interop/RegisteredFunction.cs:line 103
   at Microsoft.AspNetCore.Blazor.Browser.Interop.RegisteredFunction.InvokeUnmarshalled[TRes](String identifier) in /home/mikun/LearningCode/CSharpBlazor/dotnetcoreblazorexp/Blazor-dev/src/Microsoft.AspNetCore.Blazor.Browser/Interop/RegisteredFunction.cs:line 59
   at Microsoft.AspNetCore.Blazor.Browser.Services.BrowserUriHelper.EnsureBaseUriPopulated() in /home/mikun/LearningCode/CSharpBlazor/dotnetcoreblazorexp/Blazor-dev/src/Microsoft.AspNetCore.Blazor.Browser/Services/BrowserUriHelper.cs:line 123
   at Microsoft.AspNetCore.Blazor.Browser.Services.BrowserUriHelper.GetBaseUriPrefix() in /home/mikun/LearningCode/CSharpBlazor/dotnetcoreblazorexp/Blazor-dev/src/Microsoft.AspNetCore.Blazor.Browser/Services/BrowserUriHelper.cs:line 49
   at Microsoft.AspNetCore.Blazor.Browser.Services.BrowserServiceProvider.AddDefaultServices(ServiceCollection serviceCollection) in /home/mikun/LearningCode/CSharpBlazor/dotnetcoreblazorexp/Blazor-dev/src/Microsoft.AspNetCore.Blazor.Browser/Services/BrowserServiceProvider.cs:line 47
   at Microsoft.AspNetCore.Blazor.Browser.Services.BrowserServiceProvider..ctor(Action`1 configure) in /home/mikun/LearningCode/CSharpBlazor/dotnetcoreblazorexp/Blazor-dev/src/Microsoft.AspNetCore.Blazor.Browser/Services/BrowserServiceProvider.cs:line 34
   at StandaloneApp.Program.Main(String[] args) in /home/mikun/LearningCode/CSharpBlazor/dotnetcoreblazorexp/Blazor-dev/samples/StandaloneApp/Program.cs:line 13