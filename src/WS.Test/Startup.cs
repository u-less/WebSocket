using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Autofac;
using WS.Core;
using WS.Core.IO;
using System;

namespace WS.Coin
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public IConfigurationRoot Configuration { get; }
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }
        public void ConfigureServices(IServiceCollection services)
        {
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            //初始化数据库

            var maxBufferSize = Configuration.GetSection("WebSocket").GetSection("MaxBufferSize").Value;
            var bufferSize = Configuration.GetSection("WebSocket").GetSection("BufferSize").Value;
            var perBlockBufferCount = Configuration.GetSection("WebSocket").GetSection("PerBlockBufferCount").Value;
            WSConfig.Init(int.Parse(maxBufferSize), int.Parse(bufferSize), int.Parse(perBlockBufferCount));
            ContainerBuilder builder = new ContainerBuilder();
            Func<Type, ILogger> loggerGenerator = (Type type) =>
               {
                   return loggerFactory.CreateLogger(type);
               };
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseStaticFiles();

            app.UseWebSockets();
            app.UseWSSockets(GlobalConfig.DefaultIocManager, loggerGenerator);

            GlobalConfig.DefaultIocManager.Build();//初始化默认IOC管理器
        }
    }
}