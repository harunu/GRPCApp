# Blazor WebAssembly with gRPC Server
gRPC-Web has been used in a WCF-like approach and cover communication in between ASP.NET Core service and Blazor WebAssembly client.  

## 1. Prepare the GRPC Service - GRPC Service
Add NuGet packages:
* [Grpc.AspNetCore.Web](https://www.nuget.org/packages/Grpc.AspNetCore.Web) 
* [Google Protobuf](https://github.com/protocolbuffers/protobuf)

Register protos file:

```csharp
syntax = "proto3";
import "google/protobuf/timestamp.proto";
option csharp_namespace = "GrpcService.Protos";
package datas;
service DatasService {
rpc GetUsageData(Request) returns (stream UsageDataModel) {}
}
message Request{
string filters=1;
}
message UsageDataModel {
  google.protobuf.Timestamp Time = 1;
  string meterusage = 2;    
}```

Edit CsProj file like below:
    <Protobuf Include="Protos\datas.proto" GrpcServices="Server" /> 
```

## 2. BlazorGrpcWebApp.Server - Prepare the ASP.NET Core host 
Add NuGet packages:
* [Grpc.AspNetCore.Web](https://www.nuget.org/packages/Grpc.AspNetCore.Web) 
* [protobuf-net.Grpc.AspNetCore](https://www.nuget.org/packages/protobuf-net.Grpc.AspNetCore/)

Register `CodeFirstGrpc()` and `GrpcWeb()` services in `Startup.cs` ConfigureServices() method:

```csharp
services.AddCodeFirstGrpc(config => { config.ResponseCompressionLevel = System.IO.Compression.CompressionLevel.Optimal; });
```

Add `GrpcWeb` middleware in between `UseRouting()` and `UseEndpoints()`:

```csharp
app.UseGrpcWeb(new GrpcWebOptions() { DefaultEnabled = true });
```

## 3. BlazorGrpcWebApp.Shared- Define the service contract (code-first)
Add [System.ServiceModel.Primitives](https://www.nuget.org/packages/System.ServiceModel.Primitives/) NuGet package.

Define the interface of our sample service:

```csharp
    [ServiceContract]
    public interface IDataService
    {
        Task<List<PostData>> GetDatasFromGrpc();
        Task<string> GetTest();
    }
```

## 4. BlazorGrpcWebApp.Server - Implement and publish the service
Implement our service:

```csharp
    public class StreamService : IDataService
    {
        public async Task<List<PostData>> GetDatasFromGrpc()
        {      ....
        }
   }
```

Publish the service in `Startup.cs`:

```csharp
app.UseEndpoints(endpoints =>
{
    endpoints.MapGrpcService<MyService>();
    endpoints.MapGrpcService<StreamService>();

    endpoints.MapControllers();
    endpoints.MapFallbackToFile("index.html");
});
```

## 5. BlazorGrpcWebApp.Client (Blazor Web Assembly) - consume the service
Add NuGet packages:
* [Grpc.Net.Client](https://www.nuget.org/packages/Grpc.Net.Client)
* [Grpc.Net.Client.Web](https://www.nuget.org/packages/Grpc.Net.Client.Web) 
* [protobuf-net.Grpc](https://www.nuget.org/packages/protobuf-net.Grpc)


### 5. Consumption via dependency injection
Register a GrpcChannel in your `Program.cs` (or `Startup.cs:ConfigureServices()`)
```csharp
builder.Services.AddSingleton(services =>
{
	// Get the service address from appsettings.json
	var config = services.GetRequiredService<IConfiguration>();
	var backendUrl = config["BackendUrl"];

	// If no address is set then fallback to the current webpage URL
	if (string.IsNullOrEmpty(backendUrl))
	{
		var navigationManager = services.GetRequiredService<NavigationManager>();
		backendUrl = navigationManager.BaseUri;
	}

	// Create a channel with a GrpcWebHandler that is addressed to the backend server.
	//
	// GrpcWebText is used because server streaming requires it. 
	var httpHandler = new GrpcWebHandler(GrpcWebMode.GrpcWeb, new HttpClientHandler());

	return GrpcChannel.ForAddress(
		backendUrl,
		new GrpcChannelOptions
		{
			HttpHandler = httpHandler,
		});
});
```

Register the individual services 
```csharp
builder.Services.AddTransient<IDataService>(services =>
{
    var grpcChannel = services.GetRequiredService<GrpcChannel>();
    return grpcChannel.CreateGrpcService<IDataService>();
});

And now we can consume the services whereever needed (e.g. from .razor file):

@inject IDataService DataConsumptionService
@code {
    async Task LoadRemoteData()
    {
        this.result = await DataConsumptionService.GetDatasFromGrpc();
    }
}

```
# References, Credits
* [Use gRPC in browser apps | Microsoft Docs](https://docs.microsoft.com/en-us/aspnet/core/grpc/browser)
* [protobuf-net.Grpc - Getting Started](https://protobuf-net.github.io/protobuf-net.Grpc/gettingstarted)
