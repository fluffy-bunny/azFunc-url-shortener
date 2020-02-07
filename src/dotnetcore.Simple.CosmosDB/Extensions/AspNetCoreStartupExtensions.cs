using System;
using System.Text;
using CosmosDB.Simple.Store.Abstracts;
using CosmosDB.Simple.Store.Configuration;
using CosmosDB.Simple.Store.DbContext;
using CosmosDB.Simple.Store.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace CosmosDB.Simple.Store.Extensions
{
    public static class AspNetCoreStartupExtensions
    {
        /// <summary>
        ///     Add ToDo Store
        /// </summary>
        /// <param name="builder">The IIdentity Server Builder</param>
        /// <param name="setupAction"></param>
        /// <returns></returns>
        public static IServiceCollection AddSimpleItemStore2(
            this IServiceCollection services,
            Action<CosmosDbConfiguration> setupAction)
        {
            services.Configure(setupAction);
            services.AddTransient<ISimpleItemDbContext<Item>, DocumentDBRepository<Item>>();
            return services;
        }
        public static IServiceCollection AddSimpleItemStore<T>(
           this IServiceCollection services,
           Action<CosmosDbConfiguration> setupAction)
            where T : class
        {
            services.Configure(setupAction);
            services.AddTransient<ISimpleItemDbContext<T>, DocumentDBRepository<T>>();
            return services;
        }
    }
}
