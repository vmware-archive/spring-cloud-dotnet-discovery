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
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.OptionsModel;
using System;
using ST = SteelToe.Discovery.Client;

namespace Pivotal.Discovery.Client
{
    public static class DiscoveryServiceCollectionExtensions
    {

        public static IServiceCollection AddDiscoveryClient(this IServiceCollection services, DiscoveryOptions discoveryOptions)
        {
    
            return ST.DiscoveryServiceCollectionExtensions.AddDiscoveryClient(services, typeof(IDiscoveryClient), discoveryOptions, DiscoveryClientFactory.CreateDiscoveryClient);
        }

        public static IServiceCollection AddDiscoveryClient(this IServiceCollection services, Action<DiscoveryOptions> setupOptions)
        {
            return ST.DiscoveryServiceCollectionExtensions.AddDiscoveryClient(services, typeof(IDiscoveryClient), setupOptions, DiscoveryClientFactory.CreateDiscoveryClient);
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

            ConfigureOptions<DiscoveryOptions> configOptions = new DiscoveryOptionsFromConfigSetup(config);
            return ST.DiscoveryServiceCollectionExtensions.AddDiscoveryClient(services, typeof(IDiscoveryClient), configOptions, DiscoveryClientFactory.CreateDiscoveryClient);

        }

    }
}
