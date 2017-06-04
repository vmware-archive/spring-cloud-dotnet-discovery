#!/bin/bash

# Run unit tests 
cd test/Pivotal.Discovery.Client.Test
dotnet restore --configfile ../../nuget.config
dotnet xunit -verbose -framework netcoreapp1.1
if [[ $? != 0 ]]; then exit 1 ; fi
cd ../..

