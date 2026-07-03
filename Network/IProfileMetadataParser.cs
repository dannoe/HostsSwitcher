using System.Collections.Generic;

namespace Barbar.HostsSwitcher.Network
{
    public interface IProfileMetadataParser
    {
        IEnumerable<ProfileMatchRule> ParseMatchRules(string profileFilePath);
    }
}