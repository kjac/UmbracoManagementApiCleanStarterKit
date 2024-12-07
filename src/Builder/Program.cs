// See https://aka.ms/new-console-template for more information

using Builder;
using Builder.Builders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);
builder.Services
    .AddHttpClient()
    .AddLogging(loggingBuilder => loggingBuilder.AddConsole())
    .AddSingleton<ApiTokenService>()
    .AddTransient<DataTypesBuilder>()
    .AddTransient<TemplatesBuilder>()
    .AddTransient<CompositionsBuilder>()
    .AddTransient<ElementTypesBuilder>()
    .AddTransient<DocumentTypesBuilder>()
    .AddTransient<MediaBuilder>()
    .AddTransient<DocumentsBuilder>()
    .AddTransient<DictionaryItemsBuilder>()
    .Configure<UmbracoConfiguration>(config =>
    {
        // Umbraco config - change this to fit your custom setup
        config.Host = "https://localhost:44302";
        config.ClientId = "umbraco-back-office-my-client";
        config.ClientSecret = "my-client-secret";
    });

using IHost host = builder.Build();

var dictionaryItemsBuilder = host.Services.GetRequiredService<DictionaryItemsBuilder>();
await dictionaryItemsBuilder.BuildAsync();

var templatesBuilder = host.Services.GetRequiredService<TemplatesBuilder>();
await templatesBuilder.BuildAsync();

var mediaBuilder = host.Services.GetRequiredService<MediaBuilder>();
await mediaBuilder.BuildAsync();

var dataTypeBuilder = host.Services.GetRequiredService<DataTypesBuilder>();
await dataTypeBuilder.BuildAsync();

var compositionsBuilder = host.Services.GetRequiredService<CompositionsBuilder>();
await compositionsBuilder.BuildAsync();

var elementTypesBuilder = host.Services.GetRequiredService<ElementTypesBuilder>();
await elementTypesBuilder.BuildAsync();

var documentTypesBuilder = host.Services.GetRequiredService<DocumentTypesBuilder>();
await documentTypesBuilder.BuildAsync();

await dataTypeBuilder.UpdateDocumentTypesAsync();

var documentBuilder = host.Services.GetRequiredService<DocumentsBuilder>();
await documentBuilder.BuildAsync();

await dataTypeBuilder.UpdateDocumentsAsync();
