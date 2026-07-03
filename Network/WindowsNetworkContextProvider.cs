using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;

namespace Barbar.HostsSwitcher.Network
{
    public class WindowsNetworkContextProvider : INetworkContextProvider
    {
        public IEnumerable<NetworkContext> GetActiveNetworkContexts()
        {
            var contexts = new List<NetworkContext>();

            try
            {
                var interfaces = NetworkInterface.GetAllNetworkInterfaces()
                    .Where(ni => ni.OperationalStatus == OperationalStatus.Up)
                    .Where(ni => ni.NetworkInterfaceType != NetworkInterfaceType.Loopback);

                foreach (var networkInterface in interfaces)
                {
                    var ipProperties = networkInterface.GetIPProperties();

                    var gatewayAddresses = ipProperties.GatewayAddresses
                        .Where(g => g.Address != null && g.Address.ToString() != "0.0.0.0")
                        .Select(g => g.Address.ToString())
                        .ToList();

                    if (gatewayAddresses.Count == 0)
                        continue;

                    var dnsSuffix = ipProperties.DnsSuffix ?? string.Empty;
                    var adapterName = networkInterface.Name ?? string.Empty;

                    foreach (var gateway in gatewayAddresses)
                    {
                        contexts.Add(new NetworkContext(adapterName, gateway, dnsSuffix));
                    }
                }
            }
            catch
            {
            }

            return contexts;
        }
    }
}