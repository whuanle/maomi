namespace Demo3.ConfigCenter.Hubs
{
    /// <summary>
    /// 客户端的信息
    /// </summary>
    public class ClientInfo
    {
        /// <summary>
        /// SignalR 连接的 id
        /// </summary>
        public string ConnectionId { get; set; }

        /// <summary>
        /// 应用名称
        /// </summary>
        public string AppName { get; set; }

        /// <summary>
        /// 命名空间
        /// </summary>
        public string Namespace { get; set; }

        /// <summary>
        /// 分组名称
        /// </summary>
        public string GroupName => $"{AppName}-{Namespace}";

        /// <summary>
        /// 客户端的 IP 地址
        /// </summary>
        public string IpAddress { get; set; }
    }
}
