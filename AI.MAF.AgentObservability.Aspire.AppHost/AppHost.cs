var builder = DistributedApplication.CreateBuilder(args);

var devAgentBuilder = builder.AddProject<Projects.AI_MAF_AgentObservability_RestAPI_DEVUI>("DeveloperAgent")
    .WithUrls(context =>
    {
        var baseUrl = context.Urls.FirstOrDefault();
        if (baseUrl is not null)
        {
            context.Urls.Add(new()
            {
                Url = baseUrl.Url.TrimEnd('/') + "/devui",
                DisplayText = "DevUI Visual App"
            });
        }
    })
    .WithHttpHealthCheck("/health");

builder.AddProject<Projects.AI_MAF_AgentObservability_RestAPI_ApplicationInsights>("ai-maf-agentobservability-restapi-applicationinsights");

builder.Build().Run();
