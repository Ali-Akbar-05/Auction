using System.Net;
using MassTransit;
using MongoDB.Driver;
using MongoDB.Entities;
using Polly;
using Polly.Extensions.Http;
using SearchService.Consumers;
using SearchService.Data;
using SearchService.Models;
using SearchService.Service;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
// Add services to the container.
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddHttpClient<AuctionSvcHttpClient>()
.AddPolicyHandler(GetPolicy());

builder.Services.AddMassTransit(x =>
{
    x.AddConsumersFromNamespaceContaining<AuctionCreatedConsumer>();
    x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("search", false));

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.ReceiveEndpoint("search-auction-created", e =>
        {
            e.UseMessageRetry(r => r.Interval(5, 5));
            e.ConfigureConsumer<AuctionCreatedConsumer>(context);
        });
        cfg.ConfigureEndpoints(context);
    });
});



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{

}
app.MapControllers();

try
{
    await DbInitializer.InitDb(app);
}
catch (Exception ex)
{
    Console.WriteLine(ex);
}

app.Run();



static IAsyncPolicy<HttpResponseMessage> GetPolicy()
=> HttpPolicyExtensions
.HandleTransientHttpError()
.OrResult(msg => msg.StatusCode == HttpStatusCode.NotFound)
.WaitAndRetryForeverAsync(_ => TimeSpan.FromSeconds(3));