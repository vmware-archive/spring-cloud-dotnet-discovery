# SCS Service Discovery .NET Client

This project contains the Pivotal Discovery Client.  This client provides a generalized interface to service registries.  

Currently the client only supports [Spring Cloud Services Eureka Server](http://docs.pivotal.io/spring-cloud-services/service-registry/), but in the future we will add support for others in the future.

## Provider Package Name and Feeds

`Pivotal.Discovery.Client`

[Development feed (Less Stable)](https://www.myget.org/gallery/steeltoedev) - https://www.myget.org/gallery/steeltoedev

[Master feed (Stable)](https://www.myget.org/gallery/steeltoemaster) - https://www.myget.org/gallery/steeltoemaster

[Release or Release Candidate feed](https://www.nuget.org/) - https://www.nuget.org/

## Basic ASP.NET Core Usage
You should have a good understanding of how the new .NET [Configuration model](http://docs.asp.net/en/latest/fundamentals/configuration.html) works before starting to use the client. A basic understanding of the `ConfigurationBuilder` and how to add providers to the builder is necessary in order to configure the client.  You should also have a good understanding of how the ASP.NET Core [Startup](https://docs.asp.net/en/latest/fundamentals/startup.html) class is used in configuring the application services and the middleware used in the app. Specfically pay particular attention to the usage of the `Configure` and `ConfigureServices` methods. Its also important you have a good understanding of how to setup and use a Spring Cloud Services Eureka Server.  Detailed information on its usage can be found [here](http://docs.pivotal.io/spring-cloud-services/index.html).

In order to use the Discovery client you need to do the following:
```
1. Create and bind a service instance of the Service Registry to your application.
2. Confiure the settings the Discovery client will use to register servives in the service registry.
3. Configure the settings the Discovery client will use to discover services in the service registry.
4. Add the CloudFoundry configuration provider to the ConfigurationBuilder.    
5. Add and Use the Discovery client in the application.
``` 
## Create & Bind Service Registry
You can create a Service Registry instance using either the CloudFoundry command line tool (i.e. cf) or via the PCF Apps Manager. Below illustrates the command line option:
```
1. cf target -o myorg -s development
2. cf create-service p-service-registry standard myDiscoveryService 
3. cf bind-service myApp myDiscoveryService
4. cf restage myApp

```
Once you have bound the service to the app, the providers settings have been setup in `VCAP_SERVICES` and will be picked up automatically when the app is started by using the `CloudFoundry` configuration provider at startup.

## Eureka Settings needed to Register
Below is an example of the clients settings in JSON that are necessary to get the application to register a service named `fortuneService` with a Eureka Server. The `eureka:client:shouldFetchRegistry` setting instructs the client not to fetch the registry as the app will not be needing to discover services; it only wants to register a service. The default for this property is true.

```
{
 "spring": {
    "application": {
      "name":  "fortuneService"
    }
  },
  "eureka": {
    "client": {
      "shouldFetchRegistry": false
    }
  }
  .....
}
```
## Eureka Settings needed to Discover
Below is an example of the clients settings in JSON that are necessary to get the application to fetch the service registry from the Eureka Server  at startup.  The `eureka:client:shouldRegisterWithEureka` instructs the client to not register any services in the registry, as the app will not be offering up any services; it only wants to discover.

```
{
"spring": {
    "application": {
      "name": "fortuneUI"
    }
  },
  "eureka": {
    "client": {
      "shouldRegisterWithEureka": false
    }
  }
  .....
}
```

For a complete list of client settings see the documentation in the [IEurekaClientConfig](https://github.com/SteeltoeOSS/Discovery/blob/master/src/Steeltoe.Discovery.Eureka.Client/IEurekaClientConfig.cs) and [IEurekaInstanceConfig](https://github.com/SteeltoeOSS/Discovery/blob/master/src/Steeltoe.Discovery.Eureka.Client/IEurekaInstanceConfig.cs) files.

## Add the CloudFoundry Configuration Provider
Next we add the CloudFoundry Configuration provider to the builder (e.g. `AddCloudFoundry()`). This is needed in order to pickup the VCAP_ Service bindings and add them to the Configuration. Here is some sample code illustrating how this is done:
```
#using Pivotal.Extensions.Configuration;
...

var builder = new ConfigurationBuilder()
    .SetBasePath(basePath)
    .AddJsonFile("appsettings.json")
    .AddEnvironmentVariables()                   
    .AddCloudFoundry();
          
var config = builder.Build();
...
```
Normally in an ASP.NET Core application, the above C# code is would be included in the constructor of the `Startup` class. For example, you might see something like this:
```
#using Pivotal.Extensions.Configuration;

public class Startup {
    .....
    public IConfigurationRoot Configuration { get; private set; }
    public Startup(IHostingEnvironment env)
    {
        // Set up configuration sources.
        var builder = new ConfigurationBuilder()
            .SetBasePath(env.ContentRootPath)
            .AddJsonFile("appsettings.json")
            .AddEnvironmentVariables()
            .AddCloudFoundry();

        Configuration = builder.Build();
    }
    ....
```
## Add and Use the Discovery Client 
The next step is to Add and Use the Discovery Client.  You do these two things in  `ConfigureServices(..)` and the `Configure(..)` methods of the startup class.  The AddDiscoveryClient() call configures and adds the client to the ServiceCollection and the UseDiscoveryClient() call starts the DiscoveryClient (i.e. registers/fetches services).
```
#using Pivotal.Discovery.Client;

public class Startup {
    .....
    public IConfigurationRoot Configuration { get; private set; }
    public Startup(...)
    {
      .....
    }
    public void ConfigureServices(IServiceCollection services)
    {
        // Add Pivotal Discovery Client service
        services.AddDiscoveryClient(Configuration);

        // Add framework services.
        services.AddMvc();
        ...
    }
    public void Configure(IApplicationBuilder app, ....)
    {
        ....
        app.UseStaticFiles();
        app.UseMvc();
        
        // Use the Pivotal Discovery Client service
        app.UseDiscoveryClient();
    }
    ....
```
## Discovering Services
Once the app has started, the Discovery client will begin to operate in the background, both registering services and periodically fetching the service registry from the server.

The simplest way of using the registry to lookup services when using the `HttpClient` is to use the Pivotal `DiscoveryHttpClientHandler`. For example, see below the `FortuneService` class. It is intended to be used to retrieve Fortunes from a Fortune micro service. The micro service is registered under the name `fortuneService`.  

First, notice that the `FortuneService` constructor takes the `IDiscoveryClient` as a parameter. This is the discovery client interface which you use to lookup services in the service registry. Upon app startup, it is registered with the DI service so it can be easily used in any controller, view or service in your app.  Notice that the constructor code makes use of the client by creating an instance of the `DiscoveryHttpClientHandler`, giving it a reference to the `IDiscoveryClient`. 

Next, notice that when the `RandomFortuneAsync()` method is called, you see that the `HttpClient` is created with the handler. The handlers role is to intercept any requests made and evaluate the URL to see if the host portion of the URL can be resolved from the service registry.  In this case it will attempt to resolve the "fortuneService" name into an actual `host:port` before allowing the request to continue. If the name can't be resolved the handler will still allow the request to continue, but in this case, the request will fail.

Of course you don't have to use the handler, you can make lookup requests directly on the `IDiscoveryClient` interface if you need to.

```
using Pivotal.Discovery.Client;
....
public class FortuneService : IFortuneService
{
    DiscoveryHttpClientHandler _handler;
    private const string RANDOM_FORTUNE_URL = "http://fortuneService/api/fortunes/random";
    public FortuneService(IDiscoveryClient client)
    {
        _handler = new DiscoveryHttpClientHandler(client);
    }
    public async Task<string> RandomFortuneAsync()
    {
        var client = GetClient();
        return await client.GetStringAsync(RANDOM_FORTUNE_URL);
    }
    private HttpClient GetClient()
    {
        var client = new HttpClient(_handler, false);
        return client;
    }
}
``` 
