using Aspire.Hosting.Yarp.Transforms;
using Microsoft.AspNetCore.DataProtection.KeyManagement;

var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("cache");


var apiKey = builder.AddParameter("ollama-api-key", value: "no-api-key", secret: true);

var ollama = builder.AddOpenAI("ollama")
    .WithApiKey(apiKey)
    .WithEndpoint("http://localhost:11434/v1");

//var chatModel = ollama.AddModel("ollama-chat-client", "llama3.2");
var chatModel = ollama.AddModel("ollama-chat-client", "gpt-oss:120b-cloud");


var apiService = builder.AddProject<Projects.AiAgentAspireApp_ApiService>("apiservice")
     .WithReference(chatModel)
    .WaitFor(chatModel)
    .WithHttpHealthCheck("/health");

var productapi = builder.AddProject<Projects.PrductApi>("prductapi");

var gateway = builder.AddYarp("gateway")
    .WithConfiguration(yarp =>
    {

        yarp.AddRoute("/agent/api/{**catch-all}", apiService.GetEndpoint("http"))
        .WithTransformPathRemovePrefix("/agent/api");

        yarp.AddRoute("/product/api/{**catch-all}", productapi.GetEndpoint("http"))
        .WithTransformPathRemovePrefix("/product/api"); ;

    });
    



var webapp = builder.AddProject<Projects.AiAgentAspireApp_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(cache)
    .WaitFor(cache)
    //.WithReference(apiService)
    //.WaitFor(apiService)
     .WithReference(gateway)
    .WaitFor(gateway)
    .WithReference(chatModel)
    .WaitFor(chatModel);

var tunnel = builder.AddDevTunnel("dev-tunnel")
    .WithAnonymousAccess();

tunnel.WithReference(gateway)
    .WaitFor(gateway);

tunnel.WithReference(webapp)
    .WaitFor(webapp);

builder.Build().Run();
