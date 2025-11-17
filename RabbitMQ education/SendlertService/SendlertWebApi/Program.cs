using Dal;
using Dal.UnitOfWork;
using Domain.RepositoryInterfaces;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using RabbitMQ.Client;
using SendlertService.Services;
using Services.Contracts;
using System.Diagnostics;
using IConnectionFactory = RabbitMQ.Client.IConnectionFactory;
using UnitOfWork = Dal.UnitOfWork.UnitOfWork;
var builder = WebApplication.CreateBuilder(args);

var myActiveSource = new ActivitySource("MyApp");
var resourceBuilder = ResourceBuilder.CreateDefault().AddService(serviceName: "MyApp", serviceVersion: "1.0.0");

builder.Services.AddOpenTelemetry()
    .WithTracing(b =>
    {
        b
        .SetResourceBuilder(resourceBuilder)
        .AddSource(myActiveSource.Name)
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddOtlpExporter(o =>
        {
            o.Endpoint = new Uri("http://localhost:4317"); 
        })
        .AddConsoleExporter();
    }).WithMetrics(b =>
    {
        b.AddAspNetCoreInstrumentation()
         .AddHttpClientInstrumentation()
         .AddRuntimeInstrumentation()
         .AddPrometheusExporter(); 
    }); ;


builder.Services.AddControllers();

builder.Services.AddSingleton<IConnectionFactory>(sp =>
{
    return new ConnectionFactory
    {
        HostName = "localhost",
        Port = 5672,

        UserName = "guest",
        Password = "guest",
    };
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<DatabaseDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Database"))
);

var hangfireConnectionString = builder.Configuration.GetConnectionString("HangfireConnectionString");

builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UsePostgreSqlStorage(options => options.UseNpgsqlConnection(hangfireConnectionString)));

builder.Services.AddHangfireServer();

builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IOutboxMessageRepository, OutboxMessageRepository>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IMessageProducer, RabbitMQProducer>();
builder.Services.AddScoped<IJob, Job>();

var app = builder.Build();

//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}
app.UseSwagger();
app.UseSwaggerUI();

app.UseHangfireDashboard("/hangfire");

app.UseHttpsRedirection();

app.UseAuthorization();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<DatabaseDbContext>();
    await dbContext.Database.MigrateAsync();

    var recurringJobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();

    recurringJobManager.AddOrUpdate<IJob>(
        "myJob",
        service => service.Execute(),
        Cron.MinuteInterval(3)
    );
}

app.MapControllers();

app.MapPrometheusScrapingEndpoint();
app.MapGet("/", () => "Hello Observability: Tracing + Metrics!");

app.Run();
