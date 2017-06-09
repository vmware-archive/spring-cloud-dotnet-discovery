using Steeltoe.CloudFoundry.Connector.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pivotal.Discovery.Client
{
    public class DiscoveryClientConfigurer
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

        internal DiscoveryOptions Configure(IServiceInfo si, DiscoveryOptions config)
        {
            UpdateConfiguration(si, config);
            return config;
        }

        internal void UpdateConfiguration(IServiceInfo si, DiscoveryOptions config)
        {
            EurekaServiceInfo eurekaInfo = si as EurekaServiceInfo;
            if (eurekaInfo != null)
            {
                UpdateConfiguration(eurekaInfo, config);
            }
        }

        internal void UpdateConfiguration(EurekaServiceInfo si, DiscoveryOptions config)
        {
            if (config == null)
            {
                return;
            }
            config.ClientType = DiscoveryClientType.EUREKA;

            var clientOptions = config.ClientOptions as EurekaClientOptions;
            if (clientOptions == null)
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

            var instOptions = config.RegistrationOptions as EurekaInstanceOptions;
            if (instOptions == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(instOptions.RegistrationMethod) ||
                ROUTE_REGISTRATIONMETHOD.Equals(instOptions.RegistrationMethod, StringComparison.OrdinalIgnoreCase))
            {
                UpdateWithDefaultsForRoute(si, instOptions, clientOptions);
                return;
            }

            if (DIRECT_REGISTRATIONMETHOD.Equals(instOptions.RegistrationMethod, StringComparison.OrdinalIgnoreCase))
            {
                UpdateWithDefaultsForDirect(si, instOptions, clientOptions);
                return;
            }
            if (HOST_REGISTRATIONMETHOD.Equals(instOptions.RegistrationMethod, StringComparison.OrdinalIgnoreCase))
            {
                UpdateWithDefaultsForHost(si, instOptions, clientOptions, instOptions.HostName);
                return;
            }

        }
        internal void UpdateWithDefaultsForHost(EurekaServiceInfo si, EurekaInstanceOptions instOptions, EurekaClientOptions clientOptions, string hostName)
        {
            UpdateWithDefaults(si, instOptions, clientOptions);
            instOptions.HostName = hostName;
            instOptions.InstanceId = hostName + ":" + si.ApplicationInfo.InstanceId;
        }

        internal void UpdateWithDefaultsForDirect(EurekaServiceInfo si, EurekaInstanceOptions instOptions, EurekaClientOptions clientOptions)
        {
            UpdateWithDefaults(si, instOptions, clientOptions);
            instOptions.HostName = si.ApplicationInfo.InternalIP;
            instOptions.NonSecurePort = si.ApplicationInfo.Port;
            instOptions.InstanceId = si.ApplicationInfo.InternalIP + ":" + si.ApplicationInfo.InstanceId;
        }

        internal void UpdateWithDefaultsForRoute(EurekaServiceInfo si, EurekaInstanceOptions instOptions, EurekaClientOptions clientOptions)
        {
            UpdateWithDefaults(si, instOptions, clientOptions);
            // TODO: Java connector does this: instOptions.SecurePortEnabled = true;
            instOptions.InstanceId = si.ApplicationInfo.ApplicationUris[0] + ":" + si.ApplicationInfo.InstanceId;

        }
        internal void UpdateWithDefaults(EurekaServiceInfo si, EurekaInstanceOptions instOptions, EurekaClientOptions clientOptions)
        {
            instOptions.HostName = si.ApplicationInfo.ApplicationUris[0];
            instOptions.IpAddress = si.ApplicationInfo.InternalIP;
    
            var map = instOptions.MetadataMap;
            map[CF_APP_GUID] = si.ApplicationInfo.ApplicationId;
            map[CF_INSTANCE_INDEX] = si.ApplicationInfo.InstanceIndex.ToString();
            map[INSTANCE_ID] = si.ApplicationInfo.InstanceId;
            map[ZONE] = UNKNOWN_ZONE;
        }
    }
}
