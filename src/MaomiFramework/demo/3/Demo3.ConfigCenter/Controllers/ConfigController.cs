using Demo3.ConfigCenter.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Text.Json.Nodes;

namespace Demo3.ConfigCenter.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ConfigController : ControllerBase
    {
        private readonly ConfigCenterHub _hub;

        public ConfigController(ConfigCenterHub hub)
        {
            _hub = hub;
        }

        [HttpPost("update")]
        public async Task<string> Update(string appName, string namespaceName, [FromBody] JsonObject json)
        {
            await _hub.PublishAsync(appName, namespaceName, json);
            return "已更新配置";
        }
    }
}