using System;

namespace Barbar.HostsSwitcher.Network
{
    public class GatewayNetworkProfileMatcher : INetworkProfileMatcher
    {
        public string GetMatchType()
        {
            return "gateway";
        }

        public bool Matches(NetworkContext context, ProfileMatchRule rule)
        {
            if (context == null || rule == null)
                return false;

            if (!string.Equals(rule.MatchType, GetMatchType(), StringComparison.OrdinalIgnoreCase))
                return false;

            return string.Equals(context.DefaultGateway, rule.MatchValue, StringComparison.OrdinalIgnoreCase);
        }
    }
}