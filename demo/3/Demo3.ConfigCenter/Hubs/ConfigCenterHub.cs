using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Runtime;
using System.Text.Json.Nodes;

namespace Demo3.ConfigCenter.Hubs
{
    public partial class ConfigCenterHub : Hub
    {
        private static readonly ConcurrentDictionary<string, ClientInfo> _clients = new();
        private static readonly ConcurrentDictionary<string, JsonObject> _settings = new();

        #region 断开连接
        public override async Task OnConnectedAsync()
        {
            ClientInfo clientnInfo = GetInfo();

            await base.Groups.AddToGroupAsync(clientnInfo.ConnectionId, clientnInfo.GroupName);
            _clients[clientnInfo.GroupName] = clientnInfo;
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            ClientInfo clientnInfo = GetInfo();

            await base.Groups.RemoveFromGroupAsync(clientnInfo.ConnectionId, clientnInfo.GroupName);
            _clients.TryRemove(clientnInfo.ConnectionId, out _);
        }

        private ClientInfo GetInfo()
        {
            var feature = base.Context.Features.Get<IHttpConnectionFeature>();
            var httpContext = base.Context.GetHttpContext();

            ArgumentNullException.ThrowIfNull(feature);
            ArgumentNullException.ThrowIfNull(httpContext);

            // 从 header 中查询信息
            var appName = httpContext.Request.Headers["AppName"].FirstOrDefault();
            var namespaceName = httpContext.Request.Headers["Namespace"].FirstOrDefault();

            ArgumentNullException.ThrowIfNull(appName);
            ArgumentNullException.ThrowIfNull(namespaceName);

            // 获取客户端通讯地址
            var remoteAddress = feature.RemoteIpAddress;
            ArgumentNullException.ThrowIfNull(remoteAddress);
            var remotePort = feature.RemotePort;

            return new ClientInfo
            {
                ConnectionId = base.Context.ConnectionId,
                AppName = appName,
                Namespace = namespaceName,
                IpAddress = $"{remoteAddress.MapToIPv4().ToString()}:{remotePort}"
            };
        }

        #endregion

        /// <summary>
        /// 获取程序配置
        /// </summary>
        /// <returns></returns>
        public async Task<JsonObject> GetAsync()
        {
            ClientInfo clientnInfo = GetInfo();
            if(_settings.TryGetValue(clientnInfo.GroupName, out var v))
            {
                return v;
            }
            var dic = new Dictionary<string, JsonNode>().ToList();
            return new JsonObject(dic);
        }

        /// <summary>
        /// 更新缓存
        /// </summary>
        /// <param name="appName"></param>
        /// <param name="namespaceName"></param>
        /// <param name="json"></param>
        /// <returns></returns>
        public void UpdateCache(string appName, string namespaceName, JsonObject json)
        {
            var groupName = $"{appName}-{namespaceName}";
            _settings[groupName] = json;
        }
    }
}
