@ECHO OFF

:: Run unit tests 
cd test\Pivotal.Discovery.Client.Test
dotnet test
cd ..\..

