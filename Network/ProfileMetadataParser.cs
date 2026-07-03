using System;
using System.Collections.Generic;
using System.IO;

namespace Barbar.HostsSwitcher.Network
{
    public class ProfileMetadataParser : IProfileMetadataParser
    {
        private const string DirectivePrefix = "# HostsSwitcher:";

        public IEnumerable<ProfileMatchRule> ParseMatchRules(string profileFilePath)
        {
            var rules = new List<ProfileMatchRule>();

            if (!File.Exists(profileFilePath))
                return rules;

            try
            {
                foreach (var line in File.ReadLines(profileFilePath))
                {
                    var trimmedLine = line.Trim();
                    if (!trimmedLine.StartsWith(DirectivePrefix, StringComparison.OrdinalIgnoreCase))
                        continue;

                    var directiveContent = trimmedLine.Substring(DirectivePrefix.Length).Trim();
                    var parts = directiveContent.Split('=');
                    if (parts.Length != 2)
                        continue;

                    var matchType = parts[0].Trim();
                    var matchValue = parts[1].Trim();

                    if (string.IsNullOrEmpty(matchType) || string.IsNullOrEmpty(matchValue))
                        continue;

                    rules.Add(new ProfileMatchRule(matchType, matchValue));
                }
            }
            catch
            {
            }

            return rules;
        }
    }
}