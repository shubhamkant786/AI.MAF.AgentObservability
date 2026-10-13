using Azure.Monitor.OpenTelemetry.Exporter;
using Microsoft.Agents.AI.Hosting;
using Microsoft.Extensions.AI;
using OpenAI;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Scalar.AspNetCore;
using System.ClientModel;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
const string applicationInsightsConnectionString = "";
const string SourceName = "Agent.ApplicationInsightsSample";

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();


const string deploymentName = "gpt-5.2";
const string endpointOpenAI = "https://shubham-ms-foundry.services.ai.azure.com/openai/v1";
const string apiKey = $"";

var openAIClient = new OpenAIClient(new ApiKeyCredential(apiKey),
         new OpenAIClientOptions() { Endpoint = new Uri(endpointOpenAI) });


IChatClient chatClient = openAIClient.GetChatClient(deploymentName).AsIChatClient()
    .AsBuilder()
    .UseOpenTelemetry(configure: c => c.EnableSensitiveData = true, sourceName: SourceName)
    .Build();
builder.Services.AddSingleton(chatClient);

var developerAgent = builder.AddAIAgent(
    name: "DeveloperAgent",
    instructions:
        """
        You are a developer Agent.
        Your answers must be concise, professional, and limited strictly to development query and provide sample code only.        
        Keep responses concise — 3-5 sentences per turn. Be polite and opinionated.        
        """,
    chatClient);

builder.Services.AddOpenTelemetry()
    .WithTracing(t => t
        .ConfigureResource(r => r.AddService("multi-agent-travel-planner"))
        .AddSource(SourceName)
        .AddSource("Microsoft.Extensions.AI*")
        .AddSource("OpenAI*")
        .AddSource("Experimental.OpenAI*")
        .AddSource("Azure.AI.OpenAI*"))
    .WithMetrics(m => m
        .ConfigureResource(r => r.AddService("multi-agent-travel-planner"))
        .AddMeter(SourceName)
        .AddMeter("Microsoft.Extensions.AI*")
        .AddMeter("OpenAI*"))
    .UseAzureMonitorExporter(options => options.ConnectionString = applicationInsightsConnectionString)
    .WithLogging();



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();