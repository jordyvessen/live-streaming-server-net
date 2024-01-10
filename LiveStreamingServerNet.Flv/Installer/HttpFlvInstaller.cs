﻿using LiveStreamingServerNet.Flv.Contracts;
using LiveStreamingServerNet.Flv.Internal;
using LiveStreamingServerNet.Flv.Internal.Contracts;
using LiveStreamingServerNet.Flv.Internal.Middlewares;
using LiveStreamingServerNet.Flv.Internal.Services;
using LiveStreamingServerNet.Flv.Internal.Services.Contracts;
using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Rtmp.Installer.Contracts;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace LiveStreamingServerNet.Flv.Installer
{
    public static class HttpFlvInstaller
    {
        public static IServiceCollection AddHttpFlv(this IServiceCollection services)
        {
            services.AddSingleton<IFlvHeaderWriter, FlvHeaderWriter>();

            return services;
        }

        public static IRtmpServerConfigurator AddFlv(this IRtmpServerConfigurator configurator)
        {
            var services = configurator.Services;

            configurator.AddStreamEventHandlerr<RtmpServerStreamEventListener>()
                        .AddMediaMessageInterceptor<RtmpMediaMessageScraper>();

            services.AddSingleton<IHttpFlvClientFactory, HttpFlvClientFactory>()
                    .AddTransient<IFlvClient, FlvClient>();

            services.AddSingleton<IFlvStreamManagerService, FlvStreamManagerService>();

            return configurator;
        }

        public static void UseHttpFlv(this WebApplication webApplication, IServer liveStreamingServer)
        {
            webApplication.UseMiddleware<HttpFlvMiddleware>(liveStreamingServer);
        }
    }
}
