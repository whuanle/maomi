namespace Demo3.ConfigCenter.Hubs
{
    /// <summary>
    /// 客户端的信息
    /// </summary>
    public class ClientInfo
    {
        public string ConnectionId { get; set; }
        public string AppName { get; set; }
        public string Namespace { get; set; }
        public string GroupName { get; set; }

        public string IpAddress { get; set; }
    }
}
