using System.Collections.Generic;

namespace Barbar.HostsSwitcher.Network
{
    public interface INetworkContextProvider
    {
        IEnumerable<NetworkContext> GetActiveNetworkContexts();
    }
}