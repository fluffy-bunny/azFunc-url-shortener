using System;
using System.Collections.Generic;
using System.Text;
using dotnetcore.urlshortener.contracts;
using Microsoft.Extensions.DependencyInjection;

namespace dotnetcore.urlshortener.InMemoryStore.Extensions
{
    public static class AspNetCoreExtensions
    {
        public static IServiceCollection AddInMemoryUrlShortenerExpiryOperationalStore(this IServiceCollection services)
        {
            services.AddSingleton<IUrlShortenerExpiryOperationalStore, InMemoryUrlShortenerExpiryOperationalStore>(); // We must explicitly register Foo
            return services;
        }
       
        public static IServiceCollection AddInMemoryUrlShortenerOperationalStore(this IServiceCollection services)
        {
            services.AddSingleton<IUrlShortenerOperationalStore, InMemoryUrlShortenerOperationalStore>(); // We must explicitly register Foo
            return services;
        }
    }
}
