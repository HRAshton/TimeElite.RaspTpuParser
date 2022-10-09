using System;
using System.Net.Http;
using HRAshton.TimeElite.RaspTpuParser.Helpers;
using HRAshton.TimeElite.RaspTpuParser.Parsers;
using Microsoft.Extensions.DependencyInjection;

namespace HRAshton.TimeElite.RaspTpuParser.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRaspTpuParser(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<HttpClient>();
            serviceCollection.AddScoped<IRaspTpuIcalConverter, RaspTpuIcalConverter>();

            serviceCollection.AddScoped<PageParser>();
            serviceCollection.AddScoped<RaspTpuDecryptor>();
            serviceCollection.AddScoped<XorKeyFetcher>();

            serviceCollection.AddScoped<PageParser>();
            serviceCollection.AddScoped<HttpClient>();

            return serviceCollection;
        }
    }
}