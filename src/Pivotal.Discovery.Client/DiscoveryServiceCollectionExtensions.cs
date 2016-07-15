//
// Copyright 2015 the original author or authors.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SteelToe.CloudFoundry.Connector;
using SteelToe.CloudFoundry.Connector.Services;
using System;
using System.Collections.Generic;

namespace Pivotal.Discovery.Client
{
    public static class DiscoveryServiceCollectionExtensions
    {

        public static IServiceCollection AddDiscoveryClient(this IServiceCollection services, DiscoveryOptions discoveryOptions)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }
            if (discoveryOptions == null)
            {
                throw new ArgumentNullException(nameof(discoveryOptions));
            }

            if (discoveryOptions.ClientType == DiscoveryClientType.UNKNOWN)
            {
                throw new ArgumentException("Client type UNKNOWN");
            }

            return DoAdd(services, null, discoveryOptions);
        }

        public static IServiceCollection AddDiscoveryClient(this IServiceCollection services, Action<DiscoveryOptions> setupOptions)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (setupOptions == null)
            {
                throw new ArgumentNullException(nameof(setupOptions));
            }

            var options = new DiscoveryOptions();
            setupOptions(options);

            if (options.ClientType == DiscoveryClientType.UNKNOWN)
            {
                throw new ArgumentException("Client type UNKNOWN");
            }
            return DoAdd(services, null, options);

        }

        public static IServiceCollection AddDiscoveryClient(this IServiceCollection services, IConfiguration config)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            IServiceInfo info = GetSingletonDiscoveryServiceInfo(config);
            DiscoveryOptions options = CreateDiscoveryOptions(info, config);
            return DoAdd(services, info, options);
        }

        public static IServiceCollection AddDiscoveryClient(this IServiceCollection services, IConfiguration config, string serviceName)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (string.IsNullOrEmpty(serviceName))
            {
                throw new ArgumentNullException(nameof(serviceName));
            }

            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            IServiceInfo info = GetRequiredDiscoveryServiceInfo(config, serviceName);
            DiscoveryOptions options = CreateDiscoveryOptions(info, config);

            return DoAdd(services, info, options);
        }


        private static IServiceCollection DoAdd(IServiceCollection services, IServiceInfo info, DiscoveryOptions config)
        {
            DiscoveryClientFactory factory = new DiscoveryClientFactory(info, config);
            services.Add(new ServiceDescriptor(typeof(IDiscoveryClient), factory.Create, ServiceLifetime.Singleton));
            return services;
        }

        internal static IServiceInfo GetRequiredDiscoveryServiceInfo(IConfiguration config, string serviceName)
        {
            var info = config.GetServiceInfo(serviceName);
            if (info == null)
            {
                throw new ConnectorException(string.Format("No service with name: {0} found.", serviceName));
            }

            if (!IsRecognizedDiscoveryService(info))
            {
                throw new ConnectorException(string.Format("Service with name: {0} unrecognized Discovery ServiceInfo.", serviceName));
            }

            return info;
        }

        internal static IServiceInfo GetSingletonDiscoveryServiceInfo(IConfiguration config)
        {
            List<IServiceInfo> results = new List<IServiceInfo>();

            // Note: Could be other discovery type services in future
            AddDiscoveryServiceInfo<EurekaServiceInfo>(config, results);

            if (results.Count > 0)
            {
                if (results.Count != 1)
                {
                    throw new ConnectorException(string.Format("Multiple discovery service types bound to application."));
                }
                return results[0];
            }

            return null;
        }

        internal static void AddDiscoveryServiceInfo<SI>(IConfiguration config, List<IServiceInfo> services) where SI : class
        {

            var discService = config.GetServiceInfos<SI>();
            foreach (IServiceInfo info in discService)
            {
                services.Add(info);
            }

        }

        internal static bool IsRecognizedDiscoveryService(IServiceInfo info)
        {
            return (info as EurekaServiceInfo) != null;
        }

        private static DiscoveryOptions CreateDiscoveryOptions(IServiceInfo info, IConfiguration config)
        {
            DiscoveryOptions options = new DiscoveryOptions(config);
            if (info != null)
            {
                if (info is EurekaServiceInfo)
                {
                    options.ClientType = DiscoveryClientType.EUREKA;
                }
            }

            return options;
        }

    }
}
