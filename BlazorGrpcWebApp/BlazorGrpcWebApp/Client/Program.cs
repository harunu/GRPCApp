using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using BlazorGrpcWebApp.Client;
using Grpc.Net.Client.Web;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Components;
using ProtoBuf.Grpc.Client;
using BlazorGrpcWebApp.Shared.Interfaces;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// gRPC Channel
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
            //CompressionProviders = ...,
            //Credentials = ...,
            //DisposeHttpClient = ...,
            //HttpClient = ...,
            //LoggerFactory = ...,
            //MaxReceiveMessageSize = ...,
            //MaxSendMessageSize = ...,
        });
});

builder.Services.AddTransient<IMyService>(services =>
{
    var grpcChannel = services.GetRequiredService<GrpcChannel>();
    return grpcChannel.CreateGrpcService<IMyService>();
});

builder.Services.AddTransient<IDataService>(services =>
{
    var grpcChannel = services.GetRequiredService<GrpcChannel>();
    return grpcChannel.CreateGrpcService<IDataService>();
});


await builder.Build().RunAsync();
