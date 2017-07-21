using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WS.Core.Base;
using WS.Core.IOC;
using System.Linq;
using Microsoft.Extensions.Options;
using System.Net.WebSockets;

namespace WS.Core
{
    public static class BuildExtensions
    {
        public static WSSessionManager SessionManager { get; set; }
        public static IApplicationBuilder UseWSSockets(this IApplicationBuilder app, IocManager ioc, Func<Type, ILogger> loggerGenerator, WSSessionManager sessionManager)
        {
            SessionManager = sessionManager;
            SessionManager.Init(ioc, loggerGenerator);
            return app.UseMiddleware<WSMiddleware>();
        }
    }
    public class WSMiddleware
    {
        private readonly RequestDelegate _next;

        public WSMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                await BuildExtensions.SessionManager.Accept(context, webSocket);
            }
            else
                await _next.Invoke(context);
        }
    }

}
