using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrbintSoft.Yauaa.WebSample.AspNetCore.Services
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddUserAgentService(this IServiceCollection serviceCollection)
        {

            return serviceCollection
                .AddSingleton<IUserAgentMapper, UserAgentMapper>()
                .AddSingleton<IUserAgentService, UserAgentService>();
        }
    }
}
