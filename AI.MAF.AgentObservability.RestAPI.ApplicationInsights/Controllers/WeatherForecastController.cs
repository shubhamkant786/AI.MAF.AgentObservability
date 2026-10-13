using Microsoft.Agents.AI;
using Microsoft.AspNetCore.Mvc;
using OpenTelemetry.Trace;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace AI.MAF.AgentObservability.RestAPI.ApplicationInsights.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    const string SourceName = "Agent.ApplicationInsightsSample";
    private ActivitySource _activitySource;
    private Meter _developerAgentMeter;
    private readonly Counter<long> _developerAgentCallCounter;
    public WeatherForecastController()
    {
        _activitySource = new(SourceName);
        _developerAgentMeter = new(SourceName);
        _developerAgentCallCounter = _developerAgentMeter.CreateCounter<long>("chats.processed", "chat", "Number of developerAgent chats processed");
    }
    [HttpGet("api/chat")]
    public async Task<IResult> Get([FromQuery] string request,
        [FromKeyedServices("DeveloperAgent")] AIAgent querySupportAgent)
    {
        using var activity = _activitySource.StartActivity("developerAgent-api-chat");
        try
        {
            activity.AddEvent(new ActivityEvent("developerAgent-api-chat-started"));
            var session = await querySupportAgent.CreateSessionAsync();
            activity?.SetTag("session.id", session);
            activity?.SetTag("agent.id", querySupportAgent.Id);
            activity?.SetTag("user.id", HttpContext.User.Identities.First().Name);

            var response = await querySupportAgent.RunAsync(request, session);

            activity.AddEvent(new ActivityEvent("developerAgent-api-chat-completed"));
            activity.Duration.TotalMicroseconds.ToString();
            activity?.SetStatus(ActivityStatusCode.Ok);
            _developerAgentCallCounter.Add(1, new KeyValuePair<string, object?>("status", "success"));

            return Results.Ok(response.Text);
        }
        catch (Exception ex)
        {
            _developerAgentCallCounter.Add(1, new KeyValuePair<string, object?>("status", "failure"));
            activity?.AddException(ex);
            activity?.SetStatus(ActivityStatusCode.Error);
            return Results.InternalServerError();
        }
    }

}

