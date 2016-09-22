using Steeltoe.CloudFoundry.Connector.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pivotal.Discovery.Client
{
    public class DiscoveryClientConfigurer
    {
        private const string EUREKA_URI_SUFFIX = "/eureka/";
        private const int DEFAULT_NONSECUREPORT = 80;

        internal DiscoveryOptions Configure(IServiceInfo si, DiscoveryOptions configuration)
        {
            UpdateConfiguration(si, configuration);
            return configuration;
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
            
            instOptions.HostName = si.ApplicationInfo.ApplicationUris[0]; 
            string instance_id = si.ApplicationInfo.InstanceId; 
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
}
