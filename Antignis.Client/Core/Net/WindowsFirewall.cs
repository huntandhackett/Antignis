using NetFwTypeLib;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Antignis.Client.Core.Net
{

    internal class WindowsFirewall
    {
        private const int TCP = 6;

        public static Core.Models.WindowsFirewallSetting GetFirewallSettings(int[] portList)
        {

            INetFwPolicy2 firewallPolicy = Activator.CreateInstance(
                Type.GetTypeFromProgID("HNetCfg.FwPolicy2")) as INetFwPolicy2;

            int profileTypes = firewallPolicy.CurrentProfileTypes;

            // Get the on/off state for the various firewall profile, such as public, private, domain
            bool fwPrivateEnabled = firewallPolicy.FirewallEnabled[NET_FW_PROFILE_TYPE2_.NET_FW_PROFILE2_PRIVATE];
            bool fwPublicEnabled = firewallPolicy.FirewallEnabled[NET_FW_PROFILE_TYPE2_.NET_FW_PROFILE2_PUBLIC];
            bool fwDomainEnabled = firewallPolicy.FirewallEnabled[NET_FW_PROFILE_TYPE2_.NET_FW_PROFILE2_DOMAIN];

            // Get the default action (block/ allow) for inbound traffic per profile type
            NET_FW_ACTION_ fwPrivateDefaultInboundAction = firewallPolicy.DefaultInboundAction[NET_FW_PROFILE_TYPE2_.NET_FW_PROFILE2_PRIVATE];
            NET_FW_ACTION_ fwPublicDefaultInboundAction = firewallPolicy.DefaultInboundAction[NET_FW_PROFILE_TYPE2_.NET_FW_PROFILE2_PUBLIC];
            NET_FW_ACTION_ fwDomainDefaultInboundAction = firewallPolicy.DefaultInboundAction[NET_FW_PROFILE_TYPE2_.NET_FW_PROFILE2_DOMAIN];

            Core.Models.WindowsFirewallSetting wfwSettings = new Core.Models.WindowsFirewallSetting();
            //wfwSettings.rules = rules;
            wfwSettings.DomainProfileEnabled = fwDomainEnabled;
            wfwSettings.PublicProfileEnabled = fwPublicEnabled;
            wfwSettings.PrivateProfileEnabled = fwPrivateEnabled;
            wfwSettings.DomainProfileDefaultBlockAction = fwDomainDefaultInboundAction.ToString();
            wfwSettings.PublicProfileDefaultBlockAction = fwPublicDefaultInboundAction.ToString();
            wfwSettings.PrivateProfileDefaultBlockAction = fwPrivateDefaultInboundAction.ToString();

            return wfwSettings;
        }

        public static List<Core.Models.WindowsFirewallRule> GetFirewallRules(int[] portList)
        {

            List<Core.Models.WindowsFirewallRule> fwRules = new List<Core.Models.WindowsFirewallRule>();
            INetFwPolicy2 firewallPolicy = Activator.CreateInstance(
                    Type.GetTypeFromProgID("HNetCfg.FwPolicy2")) as INetFwPolicy2;

            INetFwRules fwrules = firewallPolicy.Rules;
            foreach (INetFwRule rule in fwrules)
            {
                // Only inbound direction is relevant for now
                if (rule.Direction != NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_IN)
                    continue;

                // Skip rules bound to applications for now
                if (rule.ApplicationName != null && rule.ApplicationName != "System")
                    continue;

                // Skip if no local ports are specified
                if (rule.LocalPorts == null)
                    continue;

                // Skip rules that are not enabled
                if (!rule.Enabled)
                    continue;

                List<string> LocalPorts = new List<string>();

                // Parse the ports
                if (rule.LocalPorts != "*")
                {

                    // For now, only TCP ports
                    if (rule.Protocol != TCP)
                        continue;

                    List<string> tmp = ParsePortList(rule.LocalPorts);

                    // If local or remote port is not in the predefined port list, do not include it
                    IEnumerable<string> portsInScope = tmp.Where(c => portList.Contains(Convert.ToInt32(c)));

                    if (portsInScope.Count() <= 0)
                        continue;

                    LocalPorts.AddRange(portsInScope);
                }
                else
                {
                    LocalPorts.Add(rule.LocalPorts);
                }

                // Parse remote addresses
                List<string> remoteAddresses = new List<string>();
                if (rule.RemoteAddresses == "*")
                    remoteAddresses.Add(rule.RemoteAddresses);

                if (rule.RemoteAddresses.IndexOf(',') > 0)
                {
                    string[] splits = rule.RemoteAddresses.Split(new[] { ',' });
                    foreach (string split in splits)
                        remoteAddresses.Add(split);
                }

                // Create a seperate rule for every remote address and every port
                foreach (string remoteAddr in remoteAddresses)
                {
                    foreach (string lp in LocalPorts)
                    {
                        // Skip if this port and remote addr have already been added
                        if (fwRules.Where(c => c.LocalPorts == lp && c.RemoteAddresses == remoteAddr).Count() > 0)
                            continue;

                        fwRules.Add(new Core.Models.WindowsFirewallRule()
                        {
                            Name = rule.Name,
                            Action = rule.Action.ToString(),
                            LocalPorts = lp,
                            RemoteAddresses = remoteAddr,
                            RuleEnabled = rule.Enabled,
                            Interfaces = rule.Interfaces,
                            Profiles = ((NET_FW_PROFILE_TYPE2_)rule.Profiles).ToString()

                        });
                    }
                }
            }

            return fwRules;
        }

        /// <summary>
        /// A port parser that takes the following arguments as inputs and returns an array of single ports:
        /// 1,2,3,4,77,8888
        /// 2,33,4488-5000
        /// 
        /// If a range is detected (by use of a dash), the complete range is calculated as well
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static List<string> ParsePortList(string input)
        {
            List<string> results = new List<string>();
            List<string> tmpInput = new List<string>();

            System.Text.RegularExpressions.Regex rexRange = new System.Text.RegularExpressions.Regex(@"^\d{1,5}\-\d{1,5}$");
            System.Text.RegularExpressions.Regex rexPort = new System.Text.RegularExpressions.Regex(@"^\d{1,5}$");

            // Check for multiple values. Comma is delimter
            if (input.IndexOf(',') > 0)
            {
                tmpInput.AddRange(input.Split(new char[] { ',' }));

                // Check for items that are a range. 
                List<string> ranges = tmpInput.Where(r => rexRange.IsMatch(r)).ToList();

                // Calculate the effective range
                foreach (string range in ranges)
                {
                    string[] splits = range.Split(new char[] { '-' });
                    int lowpart, highpart;
                    lowpart = Convert.ToInt32(splits[0]);
                    highpart = Convert.ToInt32(splits[1]);

                    List<int> expRange = Enumerable.Range(lowpart, highpart - lowpart).ToList();
                    expRange.ForEach(r => results.Add(r.ToString()));
                }

                // Check single port 
                List<string> singleports = tmpInput.Where(r => rexPort.IsMatch(r)).ToList();
                foreach (string s in singleports)
                {
                    results.Add(s);
                }
            }
            else
            {
                results.Add(input);
            }

            return results;

        }
    }
}
