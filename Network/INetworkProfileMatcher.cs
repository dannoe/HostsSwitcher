namespace Barbar.HostsSwitcher.Network
{
    public interface INetworkProfileMatcher
    {
        bool Matches(NetworkContext context, ProfileMatchRule rule);
        string GetMatchType();
    }
}