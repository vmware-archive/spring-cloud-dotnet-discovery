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

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SteelToe.CloudFoundry.Connector.Services;
using System;
using System.Collections.Generic;
using ST = SteelToe.Discovery.Client;

namespace Pivotal.Discovery.Client
{
    internal class DiscoveryClientFactory : ST.DiscoveryClientFactory
    {
        protected IServiceInfo _info;
        protected DiscoveryClientConfigurer _configurer = new DiscoveryClientConfigurer();

        internal DiscoveryClientFactory()
        {
        }
        public DiscoveryClientFactory(IServiceInfo sinfo, DiscoveryOptions config) :
            base(config)
        {
            _info = sinfo;
        }

        internal protected override void ConfigureOptions()
        {
            _configurer.Configure(_info, _config as DiscoveryOptions);
        }


        internal protected override object CreateClient(IApplicationLifetime lifeCycle = null, ILoggerFactory logFactory = null)
        {
            var logger = logFactory?.CreateLogger<DiscoveryClientFactory>();
            var config = _config as DiscoveryOptions;

            if (config == null)
            {
                logger?.LogWarning("Failed to create DiscoveryClient, no DiscoveryOptions");
                return _unknown;
            }

            if (config.ClientType == DiscoveryClientType.EUREKA)
            {
                var clientOpts = config.ClientOptions as EurekaClientOptions;
                if (clientOpts == null)
                {
                    logger?.LogWarning("Failed to create DiscoveryClient, no EurekaClientOptions");
                    return _unknown;
                }

                var instOpts = config.RegistrationOptions as EurekaInstanceOptions;
                var httpClient = new EurekaHttpClient(clientOpts, logFactory);
                return new EurekaDiscoveryClient(clientOpts, instOpts, httpClient, lifeCycle, logFactory);
            }
            else
            {
                logger?.LogWarning("Failed to create DiscoveryClient, unknown ClientType: {0}", config.ClientType.ToString());
            }


            return _unknown;
        }
        private static UnknownDiscoveryClient _unknown = new UnknownDiscoveryClient();
    }
    public class UnknownDiscoveryClient : IDiscoveryClient
    {
        public string Description
        {
            get
            {
                return "Unknown";
            }
        }

        public IList<string> Services
        {
            get
            {
                return new List<string>();
            }
        }

        public IList<IServiceInstance> GetInstances(string serviceId)
        {
            return new List<IServiceInstance>();
        }

        public IServiceInstance GetLocalServiceInstance()
        {
            return null;
        }

        public void ShutdownAsync()
        {

        }
    }
}

