// Copyright 2017 the original author or authors.
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

using Microsoft.Extensions.Configuration;
using Steeltoe.CloudFoundry.Connector.Services;
using Steeltoe.Discovery.Eureka;
using System;

namespace Pivotal.Discovery.Eureka
{
    public class PivotalEurekaConfigurer
    {
        internal const string EUREKA_URI_SUFFIX = "/eureka/";

        internal const string ROUTE_REGISTRATIONMETHOD = "route";
        internal const string DIRECT_REGISTRATIONMETHOD = "direct";
        internal const string HOST_REGISTRATIONMETHOD = "hostname";

        internal const string CF_APP_GUID = "cfAppGuid";
        internal const string CF_INSTANCE_INDEX = "cfInstanceIndex";
        internal const string SURGICAL_ROUTING_HEADER = "X-CF-APP-INSTANCE";
        internal const string INSTANCE_ID = "instanceId";
        internal const string ZONE = "zone";
        internal const string UNKNOWN_ZONE = "unknown";
        internal const int DEFAULT_NONSECUREPORT = 80;
        internal const int DEFAULT_SECUREPORT = 443;

        public static void UpdateConfiguration(IConfiguration config, EurekaServiceInfo si, EurekaClientOptions clientOptions)
        {
            if (clientOptions == null || si == null)
            {
                return;
            }

            var uri = si.Uri;

            if (!uri.EndsWith(EUREKA_URI_SUFFIX))
            {
                uri = uri + EUREKA_URI_SUFFIX;
            }

            clientOptions.EurekaServerServiceUrls = uri;
            clientOptions.AccessTokenUri = si.TokenUri;
            clientOptions.ClientId = si.ClientId;
            clientOptions.ClientSecret = si.ClientSecret;
        }

        public static void UpdateConfiguration(IConfiguration config, EurekaServiceInfo si, EurekaInstanceOptions instOptions)
        {
            if (instOptions == null)
            {
                return;
            }

            EurekaPostConfigurer.UpdateConfiguration(config, instOptions);

            if (si == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(instOptions.RegistrationMethod) ||
                ROUTE_REGISTRATIONMETHOD.Equals(instOptions.RegistrationMethod, StringComparison.OrdinalIgnoreCase))
            {
                UpdateWithDefaultsForRoute(si, instOptions);
                return;
            }

            if (DIRECT_REGISTRATIONMETHOD.Equals(instOptions.RegistrationMethod, StringComparison.OrdinalIgnoreCase))
            {
                UpdateWithDefaultsForDirect(si, instOptions);
                return;
            }

            if (HOST_REGISTRATIONMETHOD.Equals(instOptions.RegistrationMethod, StringComparison.OrdinalIgnoreCase))
            {
                UpdateWithDefaultsForHost(si, instOptions, instOptions.HostName);
                return;
            }
        }

        private static void UpdateWithDefaultsForHost(EurekaServiceInfo si, EurekaInstanceOptions instOptions, string hostName)
        {
            UpdateWithDefaults(si, instOptions);
            instOptions.HostName = hostName;
            instOptions.InstanceId = hostName + ":" + si.ApplicationInfo.InstanceId;
        }

        private static void UpdateWithDefaultsForDirect(EurekaServiceInfo si, EurekaInstanceOptions instOptions)
        {
            UpdateWithDefaults(si, instOptions);
            instOptions.PreferIpAddress = true;
            instOptions.NonSecurePort = si.ApplicationInfo.Port;
            instOptions.SecurePort = si.ApplicationInfo.Port;
            instOptions.InstanceId = si.ApplicationInfo.InternalIP + ":" + si.ApplicationInfo.InstanceId;
        }

        private static void UpdateWithDefaultsForRoute(EurekaServiceInfo si, EurekaInstanceOptions instOptions)
        {
            UpdateWithDefaults(si, instOptions);
            instOptions.NonSecurePort = DEFAULT_NONSECUREPORT;
            instOptions.SecurePort = DEFAULT_SECUREPORT;
            instOptions.InstanceId = si.ApplicationInfo.ApplicationUris[0] + ":" + si.ApplicationInfo.InstanceId;
        }

        private static void UpdateWithDefaults(EurekaServiceInfo si, EurekaInstanceOptions instOptions)
        {

            if (si.ApplicationInfo.ApplicationUris != null &&
                si.ApplicationInfo.ApplicationUris.Length > 0)
            {
                instOptions.HostName = si.ApplicationInfo.ApplicationUris[0];
            }
            else
            {
                instOptions.HostName = si.ApplicationInfo.InternalIP;
            }

            instOptions.IpAddress = si.ApplicationInfo.InternalIP;
            var map = instOptions.MetadataMap;
            map[CF_APP_GUID] = si.ApplicationInfo.ApplicationId;
            map[CF_INSTANCE_INDEX] = si.ApplicationInfo.InstanceIndex.ToString();
            map[INSTANCE_ID] = si.ApplicationInfo.InstanceId;
            map[ZONE] = UNKNOWN_ZONE;
        }
    }
}
