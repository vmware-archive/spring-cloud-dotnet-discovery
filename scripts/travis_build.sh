#!/bin/bash

echo Code is built in Unit Tests

cd src/Pivotal.Discovery.EurekaBase
dotnet restore --configfile ../../nuget.config
cd ../..

cd src/Pivotal.Discovery.ClientCore
dotnet restore --configfile ../../nuget.config
cd ../..

cd src/Pivotal.Discovery.ClientAutofac
dotnet restore --configfile ../../nuget.config
cd ../..