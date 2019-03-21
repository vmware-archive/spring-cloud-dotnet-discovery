// Copyright 2017 the original author or authors.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// https://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Steeltoe.Discovery.Eureka;
using Steeltoe.Discovery.Eureka.Transport;
using System;

namespace Pivotal.Discovery.Eureka
{
    [Obsolete("Use the Steeltoe.Discovery.EurekaBase packages!")]
    public class PivotalEurekaDiscoveryClient : Steeltoe.Discovery.Eureka.EurekaDiscoveryClient
    {
        public PivotalEurekaDiscoveryClient(
            IOptionsMonitor<EurekaClientOptions> clientConfig,
            IOptionsMonitor<EurekaInstanceOptions> instConfig,
            EurekaApplicationInfoManager appInfoManager,
            IEurekaHttpClient httpClient = null,
            ILoggerFactory logFactory = null,
            IEurekaDiscoveryClientHandlerProvider handlerProvider = null)
                : base(clientConfig, instConfig, appInfoManager, new PivotalEurekaHttpClient(clientConfig, handlerProvider, logFactory), logFactory)
        {
        }
    }
}
