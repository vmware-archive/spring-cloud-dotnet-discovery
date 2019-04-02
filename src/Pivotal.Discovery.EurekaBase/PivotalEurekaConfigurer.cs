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

using Microsoft.Extensions.Configuration;
using Steeltoe.CloudFoundry.Connector.Services;
using Steeltoe.Discovery.Eureka;
using System;

namespace Pivotal.Discovery.Eureka
{
    [Obsolete("Use the Steeltoe.Discovery.EurekaBase packages!")]
    public class PivotalEurekaConfigurer
    {
        [Obsolete("Use the Steeltoe.Discovery.EurekaBase packages!")]
        public static void UpdateConfiguration(IConfiguration config, EurekaServiceInfo si, EurekaClientOptions clientOptions)
        {
            Steeltoe.Discovery.Eureka.EurekaPostConfigurer.UpdateConfiguration(config, si, clientOptions);
        }

        [Obsolete("Use the Steeltoe.Discovery.EurekaBase packages!")]
        public static void UpdateConfiguration(IConfiguration config, EurekaServiceInfo si, EurekaInstanceOptions instOptions)
        {
            Steeltoe.Discovery.Eureka.EurekaPostConfigurer.UpdateConfiguration(config, si, instOptions);
        }
    }
}
