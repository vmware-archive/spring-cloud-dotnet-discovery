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

using System;

using ST = SteelToe.Discovery.Client;

namespace Pivotal.Discovery.Client
{
    public class DiscoveryHttpClientHandler : ST.DiscoveryHttpClientHandlerBase
    {
        private IDiscoveryClient _client;

        public DiscoveryHttpClientHandler(IDiscoveryClient client) : base()
        {
            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            _client = client;
        }

        internal protected override Uri LookupService(Uri current)
        {
            if (!current.IsDefaultPort)
            {
                return current;
            }

            var instances = _client.GetInstances(current.Host);
            if (instances.Count > 0)
            {
                int indx = _random.Next(instances.Count);
                return new Uri(instances[indx].Uri, current.PathAndQuery);
            }

            return current;

        }
    }
}
