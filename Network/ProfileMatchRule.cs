using System;

namespace Barbar.HostsSwitcher.Network
{
    public class ProfileMatchRule
    {
        public string MatchType { get; set; }
        public string MatchValue { get; set; }

        public ProfileMatchRule(string matchType, string matchValue)
        {
            MatchType = matchType ?? string.Empty;
            MatchValue = matchValue ?? string.Empty;
        }

        public override string ToString()
        {
            return $"{MatchType}={MatchValue}";
        }
    }
}