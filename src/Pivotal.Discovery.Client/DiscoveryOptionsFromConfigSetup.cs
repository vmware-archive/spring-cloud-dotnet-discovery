﻿//
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
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using ST = SteelToe.Discovery.Client;

namespace Pivotal.Discovery.Client
{
    public class DiscoveryOptionsFromConfigSetup : ConfigureOptions<DiscoveryOptions>
    {
        public const string EUREKA_URI_SUFFIX = "/eureka/";
        public const string EUREKA_PREFIX = "eureka";
        public const string EUREKA_CLIENT_CONFIGURATION_PREFIX = "eureka:client";
        public const string EUREKA_INSTANCE_CONFIGURATION_PREFIX = "eureka:instance";
        private const string VCAP_SERVICES_EUREKA_PREFIX = "vcap:services:p-service-registry:0";
        private const string VCAP_APPLICATION_PREFIX = "vcap:application";
        private const int DEFAULT_NONSECUREPORT = 80;

        public DiscoveryOptionsFromConfigSetup(IConfiguration config)
            : base(options => ConfigureDiscovery(config, options))
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }
        }

        internal static void ConfigureDiscovery(IConfiguration config, DiscoveryOptions options)
        {
            var clientConfigsection = config.GetSection(EUREKA_PREFIX);
            int childCount = clientConfigsection.GetChildren().Count();
            if (childCount > 0)
            {
                ST.DiscoveryOptionsFromConfigSetup.ConfigureDiscovery(config, options, new EurekaClientOptions(), new EurekaInstanceOptions());
                InitializeCloudFoundry(options, config);
            }
            else
            {
                options.ClientType = DiscoveryClientType.UNKNOWN;
            }

        }

   
        private static void InitializeCloudFoundry(DiscoveryOptions options, IConfiguration config)
        {

            var clientEurekasection = config.GetSection(EUREKA_CLIENT_CONFIGURATION_PREFIX);
            // Check for cloudfoundry binding vcap:services:p-service-registry:0
            var vcapEurekaSection = config.GetSection(VCAP_SERVICES_EUREKA_PREFIX);
            int childCount = vcapEurekaSection.GetChildren().Count();
            if (childCount > 0)
            {
                var clientOptions = options.ClientOptions as EurekaClientOptions;
                if (clientOptions == null)
                {
                    return;
                }

                var uri = GetUri(vcapEurekaSection, clientEurekasection, clientOptions.EurekaServerServiceUrls);
                if (!uri.EndsWith(EUREKA_URI_SUFFIX))
                {
                    uri = uri + EUREKA_URI_SUFFIX;
                }
               
                clientOptions.EurekaServerServiceUrls = uri;
                clientOptions.AccessTokenUri = GetAccessTokenUri(vcapEurekaSection, clientEurekasection, clientOptions.AccessTokenUri);
                clientOptions.ClientId = GetClientId(vcapEurekaSection, clientEurekasection, clientOptions.ClientId);
                clientOptions.ClientSecret = GetClientSecret(vcapEurekaSection, clientEurekasection, clientOptions.ClientSecret);

                var instOptions = options.RegistrationOptions as EurekaInstanceOptions;
                if (instOptions == null)
                {
                    return;
                }

                var vcapAppSection = config.GetSection(VCAP_APPLICATION_PREFIX);
                instOptions.HostName = GetApplicationUri(vcapAppSection, clientEurekasection, instOptions.HostName);
                string instance_id = GetInstanceId(vcapAppSection, clientEurekasection, null);
                if (string.IsNullOrEmpty(instance_id))
                {
                    instance_id = Environment.GetEnvironmentVariable("CF_INSTANCE_GUID");
                }
                if (!string.IsNullOrEmpty(instance_id))
                {
                    instOptions.InstanceId = instOptions.HostName + ":" + instance_id;
                    instOptions.MetadataMap.Add("instanceId", instance_id);
                }

                instOptions.NonSecurePort = DEFAULT_NONSECUREPORT;

            }

        }

  
        private static string GetApplicationUri(IConfigurationSection primary, IConfigurationSection secondary, string def)
        {
            return GetSetting("uris:0", primary, secondary, def);
        }

        private static string GetInstanceId(IConfigurationSection primary, IConfigurationSection secondary, string def)
        {
            return GetSetting("instance_id", primary, secondary, def);
        }

        private static string GetUri(IConfigurationSection primary, IConfigurationSection secondary, string def)
        {
            return GetSetting("credentials:uri", primary, secondary, def);
        }

        private static string GetClientSecret(IConfigurationSection primary, IConfigurationSection secondary, string def)
        {
            return GetSetting("credentials:client_secret", primary, secondary, def);

        }

        private static string GetClientId(IConfigurationSection primary, IConfigurationSection secondary, string def)
        {
            return GetSetting("credentials:client_id", primary, secondary, def);
        }

        private static string GetAccessTokenUri(IConfigurationSection primary, IConfigurationSection secondary, string def)
        {
            return GetSetting("credentials:access_token_uri", primary, secondary, def);
        }

        private static string GetSetting(string key, IConfigurationSection primary, IConfigurationSection secondary, string def)
        {
            // First check for key in primary
            var setting = primary[key];
            if (!string.IsNullOrEmpty(setting))
            {
                return setting;
            }

            // Next check for key in secondary
            setting = secondary[key];
            if (!string.IsNullOrEmpty(setting))
            {
                return setting;
            }

            return def;
        }

    }
}
