using System;

namespace Barbar.HostsSwitcher.Network
{
    public class NetworkContext
    {
        public string AdapterName { get; set; }
        public string DefaultGateway { get; set; }
        public string DnsSuffix { get; set; }
        public string NetworkName { get; set; }

        public NetworkContext(string adapterName, string defaultGateway, string dnsSuffix = null,
            string networkName = null)
        {
            AdapterName = adapterName ?? string.Empty;
            DefaultGateway = defaultGateway ?? string.Empty;
            DnsSuffix = dnsSuffix ?? string.Empty;
            NetworkName = networkName ?? string.Empty;
        }

        public override string ToString()
        {
            return $"Adapter: {AdapterName}, Gateway: {DefaultGateway}, DNS: {DnsSuffix}, Network: {NetworkName}";
        }
    }
}