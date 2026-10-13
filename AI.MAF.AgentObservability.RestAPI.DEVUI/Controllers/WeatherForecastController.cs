using Microsoft.Agents.AI;
using Microsoft.AspNetCore.Mvc;

namespace AI.MAF.AgentObservability.RestAPI.DEVUI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        [HttpGet("api/chat")]
        public async Task<IResult> Get([FromQuery] string request,
            [FromKeyedServices("DeveloperAgent")] AIAgent querySupportAgent)
        {
            var response = await querySupportAgent.RunAsync(request);
            return Results.Ok(response.Text);
        }

    }
}
