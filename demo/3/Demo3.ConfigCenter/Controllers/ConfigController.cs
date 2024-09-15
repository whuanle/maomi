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
        private readonly ConfigCenterHub _configCenter;
        private readonly IHubContext<ConfigCenterHub> _hubContext;

        public ConfigController(IHubContext<ConfigCenterHub> hubContext, ConfigCenterHub configCenter)
        {
            _hubContext = hubContext;
            _configCenter = configCenter;
        }

        [HttpPost("update")]
        public async Task<string> Update(string appName, string namespaceName, [FromBody] JsonObject json)
        {
            var groupName = $"{appName}-{namespaceName}";
            _configCenter.UpdateCache(appName, namespaceName, json);
            await _hubContext.Clients.Group(groupName).SendAsync("Publish", json);
            return "已更新配置";
        }
    }
}