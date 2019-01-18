using System;
using Microsoft.Owin.Extensions;
using Owin;

namespace Umbraco.RestApi.Tests.TestHelpers
{
    public static class AuthenticateEverythingExtensions
    {
        public static IAppBuilder AuthenticateEverything(this IAppBuilder app, AuthenticateEverythingAuthenticationOptions options = null)
        {
            if (app == null)
                throw new ArgumentNullException("app");
            app.Use(typeof(AuthenticateEverythingMiddleware), 
                app,
                options ?? new AuthenticateEverythingAuthenticationOptions());
            app.UseStageMarker(PipelineStage.Authenticate);
            return app;
        }
    }
}