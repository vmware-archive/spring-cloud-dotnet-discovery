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

using Microsoft.AspNet.Hosting;
using Microsoft.Extensions.Logging;
using SteelToe.Discovery.Eureka.AppInfo;
using SteelToe.Discovery.Eureka.Transport;
using System.Collections.Generic;
using ST = SteelToe.Discovery.Client;

namespace Pivotal.Discovery.Client
{
    public class EurekaDiscoveryClient : ST.EurekaDiscoveryClientBase, IDiscoveryClient
    {
        internal EurekaDiscoveryClient(EurekaClientOptions clientOptions, EurekaInstanceOptions instOptions, IEurekaHttpClient httpClient, IApplicationLifetime lifeCycle = null, ILoggerFactory logFactory = null)
            : base(clientOptions, instOptions, httpClient, lifeCycle, logFactory)
        {
        }
        public override string Description
        {
            get
            {
                return "Spring Cloud Services Eureka Client";
            }
        }

        public IList<IServiceInstance> GetInstances(string serviceId)
        {
            IList<InstanceInfo> infos = Client.GetInstancesByVipAddress(serviceId, false);
            List<IServiceInstance> instances = new List<IServiceInstance>();
            foreach (InstanceInfo info in infos)
            {
                instances.Add(new EurekaServiceInstance(info));
            }
            return instances;
        }

        public IServiceInstance GetLocalServiceInstance()
        {
            return new ThisServiceInstance(InstConfig.GetHostName(false),
                InstConfig.SecurePortEnabled, InstConfig.MetadataMap, InstConfig.NonSecurePort, InstConfig.AppName);
        }

    }

    public class ThisServiceInstance : ST.ThisServiceInstance, IServiceInstance
    {
        public ThisServiceInstance(string host, bool isSecure, IDictionary<string, string> metadata, int port, string serviceId)
            : base(host, isSecure, metadata, port, serviceId)
        {
        }
    }

    public class EurekaServiceInstance : ST.EurekaServiceInstance, IServiceInstance
    {

        internal EurekaServiceInstance(InstanceInfo info) : base(info)
        {

        }
    }
}
