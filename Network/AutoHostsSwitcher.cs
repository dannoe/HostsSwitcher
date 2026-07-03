using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Barbar.HostsSwitcher.Provider;

namespace Barbar.HostsSwitcher.Network
{
    public class AutoHostsSwitcher : IAutoHostsSwitcher
    {
        private readonly IHostProvider _hostProvider;
        private readonly INetworkContextProvider _networkContextProvider;
        private readonly IProfileMetadataParser _metadataParser;
        private readonly List<INetworkProfileMatcher> _matchers;

        public AutoHostsSwitcher(
            IHostProvider hostProvider,
            INetworkContextProvider networkContextProvider,
            IProfileMetadataParser metadataParser,
            IEnumerable<INetworkProfileMatcher> matchers)
        {
            this._hostProvider = hostProvider ?? throw new ArgumentNullException(nameof(hostProvider));
            this._networkContextProvider = networkContextProvider ?? throw new ArgumentNullException(nameof(networkContextProvider));
            this._metadataParser = metadataParser ?? throw new ArgumentNullException(nameof(metadataParser));
            this._matchers = matchers?.ToList() ?? new List<INetworkProfileMatcher>();
        }

        public AutoSwitchResult EvaluateAndSwitch()
        {
            try
            {
                var activeContexts = _networkContextProvider.GetActiveNetworkContexts().ToList();
                if (activeContexts.Count == 0)
                {
                    return new AutoSwitchResult(AutoSwitchStatus.NoMatch,
                        "No active network adapters with gateways found");
                }

                var hostsDirectory = _hostProvider.GetHostsDirectory();
                var profileFiles = _hostProvider.GetHostFiles()
                    .Where(f => !string.Equals(f, "hosts", StringComparison.OrdinalIgnoreCase))
                    .OrderBy(f => f)
                    .ToList();

                var candidateProfiles = new List<string>();

                foreach (var profile in profileFiles)
                {
                    var profilePath = Path.Combine(hostsDirectory, profile);
                    var rules = _metadataParser.ParseMatchRules(profilePath).ToList();

                    if (rules.Count == 0)
                        continue;

                    foreach (var rule in rules)
                    {
                        foreach (var matcher in _matchers)
                        {
                            foreach (var context in activeContexts)
                            {
                                if (matcher.Matches(context, rule))
                                {
                                    candidateProfiles.Add(profile);
                                    goto NextProfile;
                                }
                            }
                        }
                    }

                    NextProfile: ;
                }

                if (candidateProfiles.Count == 0)
                {
                    return new AutoSwitchResult(AutoSwitchStatus.NoMatch,
                        "No hosts profile matches the current network");
                }

                var selectedProfile = candidateProfiles[0];

                if (IsProfileAlreadyActive(selectedProfile, hostsDirectory))
                {
                    if (candidateProfiles.Count > 1)
                    {
                        return new AutoSwitchResult(
                            AutoSwitchStatus.AmbiguousMatch,
                            $"Already using '{selectedProfile}' (ambiguous: {candidateProfiles.Count} profiles matched)",
                            selectedProfile,
                            candidateProfiles.ToArray());
                    }

                    return new AutoSwitchResult(AutoSwitchStatus.AlreadyActive, $"Already using '{selectedProfile}'",
                        selectedProfile);
                }

                _hostProvider.ReplaceHosts(selectedProfile);

                if (candidateProfiles.Count > 1)
                {
                    return new AutoSwitchResult(
                        AutoSwitchStatus.AmbiguousMatch,
                        $"Switched to '{selectedProfile}' (ambiguous: {candidateProfiles.Count} profiles matched)",
                        selectedProfile,
                        candidateProfiles.ToArray());
                }

                return new AutoSwitchResult(AutoSwitchStatus.Switched, $"Switched to '{selectedProfile}'",
                    selectedProfile);
            }
            catch (Exception ex)
            {
                return new AutoSwitchResult(AutoSwitchStatus.Error, $"Auto-switch error: {ex.Message}");
            }
        }

        private bool IsProfileAlreadyActive(string profileName, string hostsDirectory)
        {
            try
            {
                var hostsPath = Path.Combine(hostsDirectory, "hosts");
                var profilePath = Path.Combine(hostsDirectory, profileName);

                if (!File.Exists(hostsPath) || !File.Exists(profilePath))
                    return false;

                var hostsContent = File.ReadAllText(hostsPath);
                var profileContent = File.ReadAllText(profilePath);

                return string.Equals(hostsContent, profileContent, StringComparison.Ordinal);
            }
            catch
            {
                return false;
            }
        }
    }
}