using Microsoft.GroupPolicy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Antignis.Server.Core.ADDS
{

    internal sealed class GPO
    {
        #region StaticProperties

        private const string POL_VERSION = "v2.28";
        private const string REG_RULE_LOCATION = @"SOFTWARE\Policies\Microsoft\WindowsFirewall\FirewallRules";
        private const string REG_CSR_LOCATION = @"SOFTWARE\Policies\Microsoft\WindowsFirewall\ConSecRules";

        #endregion

        /// <summary>
        /// Returns a string with all ports concatenated that can be used as filter for inbound firewall rules within a GPO
        /// </summary>
        /// <param name="ports"></param>
        /// <returns></returns>
        private static string GetLocalPortString(List<int> ports)
        {
            string fwPortString = string.Empty;
            ports.ForEach(c => fwPortString += $"LPort={c}|");
            return fwPortString.TrimEnd('|');
        }

        /// <summary>
        /// Returns a string with all ports concatenated that can be used as filter for CSR rules within a GPO
        /// </summary>
        /// <param name="ports"></param>
        /// <returns></returns>
        private static string GetCSRPortString(List<int> ports)
        {
            string csrPortString = string.Empty;
            ports.ForEach(c => csrPortString += $"EP1Port={c}|");
            return csrPortString.TrimEnd('|');
        }

        /// <summary>
        /// Returns the config string that is used within the GPO to block inbound access on given ports
        /// </summary>
        private static string GetInboundBlockRule(string LocalPortString, string policyName)
        {
            return $"{POL_VERSION}|Action=Block|Active=TRUE|Dir=In|Protocol=6|{LocalPortString}|Name={policyName}|";
        }

        /// <summary>
        /// Returns a CSR that requires both computer and user to be authenticated
        /// </summary>
        /// <param name="CsrPortString"></param>
        /// <param name="protocolName"></param>
        /// <returns></returns>
        private static string GetConnectionSecurityRuleForBastionAndUser(string CsrPortString, string policyName)
        {
            return $"{POL_VERSION}|Action=SecureServer|Name=CSR_{policyName}" +
                    $"|Desc=|Protocol=6|Active=TRUE|{CsrPortString}|" +
                    "Auth1Set=ComputerKerberos|Auth2Set=UserKerberos|Crypto2Set={E5A5D32A-4BCE-4e4d-B07F-4AB1BA7E5FE2}" +
                    "|EmbedCtxt=|";
        }

        /// <summary>
        /// Returns a CSR that requires computer to be authenticated
        /// </summary>
        /// <param name="CsrPortString"></param>
        /// <param name="protocolName"></param>
        /// <returns></returns>
        private static string GetConnectionSecurityRuleForBastion(string CsrPortString, string policyName)
        {
            return $"{POL_VERSION}|Action=SecureServer|Name=CSR_{policyName}" +
                    $"|Desc=|Protocol=6|Active=TRUE|{CsrPortString}|" +
                    "Auth1Set=ComputerKerberos|Crypto2Set={E5A5D32A-4BCE-4e4d-B07F-4AB1BA7E5FE2}" +
                    "|EmbedCtxt=|";
        }

        /// <summary>
        /// Returns a CSR that requires user to be authenticated
        /// </summary>
        /// <param name="CsrPortString"></param>
        /// <param name="protocolName"></param>
        /// <returns></returns>
        private static string GetConnectionSecurityRuleForUser(string CsrPortString, string policyName)
        {
            return $"{POL_VERSION}|Action=SecureServer|Name=CSR_{policyName}" +
                    $"|Desc=|Protocol=6|Active=TRUE|{CsrPortString}|" +
                    "Auth1Set=UserKerberos|Crypto2Set={E5A5D32A-4BCE-4e4d-B07F-4AB1BA7E5FE2}" +
                    "|EmbedCtxt=|";
        }

        /// <summary>
        /// Creates a bypass setting where users and computers in selected groups have access
        /// </summary>
        /// <param name="localPortString"></param>
        /// <param name="excludeUserGroupSID"></param>
        /// <param name="excludeBastionGroupSID"></param>
        /// <returns></returns>
        private static string GetBypassSettingForBastionAndUser(string localPortString, string policyName,
            string excludeUserGroupSID, string excludeBastionGroupSID)
        {
            return $"{POL_VERSION}|Action=ByPass|Active=TRUE|Dir=In|Protocol=6|{localPortString}|" +
                    $"Name=Allow rule for: {policyName}|Desc=Admin hosts and users can bypass the block rule" +
                    $"|RMauth=O:LSD:(A;;CC;;;{excludeBastionGroupSID})|" +
                    $"RUAuth=O:LSD:(A;;CC;;;{excludeUserGroupSID})|" +
                    "Security=Authenticate|Security2_9=An-NoEncap|";
        }

        /// <summary>
        /// Creates a bypass setting where computers in given group have access
        /// </summary>
        /// <param name="localPortString"></param>
        /// <param name="humanReadablePortString"></param>
        /// <param name="excludeBastionGroup"></param>
        /// <param name="excludeBastionGroupSID"></param>
        /// <returns></returns>
        private static string GetBypassSettingForBastion(string localPortString, string policyName,
            string excludeBastionGroup, string excludeBastionGroupSID)
        {
            return $"{POL_VERSION}|Action=ByPass|Active=TRUE|Dir=In|Protocol=6|{localPortString}|" +
                    $"Name=Allow rule for: {policyName}|Desc=Admin hosts can bypass the block rule" +
                    $"|RMauth=O:LSD:(A;;CC;;;{excludeBastionGroupSID})|" +
                    "Security=Authenticate|Security2_9=An-NoEncap|";
        }

        /// <summary>
        /// Creates a bypass setting where users in given group have access
        /// </summary>
        /// <param name="localPortString"></param>
        /// <param name="humanReadablePortString"></param>
        /// <param name="excludeUserGroup"></param>
        /// <param name="excludeUserGroupSID"></param>
        /// <returns></returns>
        private static string GetBypassSettingForUser(string localPortString, string policyName,
            string excludeUserGroup, string excludeUserGroupSID)
        {
            return $"{POL_VERSION}|Action=ByPass|Active=TRUE|Dir=In|Protocol=6|{localPortString}|" +
                    $"Name=Allow rule for: {policyName}|Desc=Admin users can bypass the block rule" +
                    $"|RUAuth=O:LSD:(A;;CC;;;{excludeUserGroupSID})|" +
                    "Security=Authenticate|Security2_9=An-NoEncap|";
        }

        /// <summary>
        /// Creates a new GPO with firewall settings included. Returns true when successfull, false when errored
        /// </summary>
        /// <param name="displayName">Displayname of the GPO</param>
        /// <param name="description">Description of the GPO</param>
        /// <param name="ports">The ports that will be blocked</param>
        /// <param name="groupToAddInScope">The group to be added to the scope of the GPO, on which the GPO will be applied</param>
        /// <param name="excludeUsergroup">Name of the group with user accounts that can bypass the blockrule</param>
        /// <param name="excludeComputerGroup">Name of the group with computer accounts that can bypass the blockrule</param>
        /// <param name="ldapClient">Connected and authenticated LDAP client</param>
        /// <param name="protocolName">Protocolname. Can be RDP, WIN-RM or anything else. Is not used for techical purposes, only declaritive</param>
        /// <returns></returns>
        public static bool Create(string displayName, string description, List<int> ports, string groupToAddInScope, string excludeUsergroup, string excludeComputerGroup, LDAP ldapClient, string protocolName)
        {
            bool result = false;

            // Sort ports ascending, if not already
            ports.Sort();
            string humanReadablePortString = string.Join(", ", ports);

            try
            {
                // Create GPO object
                GPDomain domain = new GPDomain();
                Gpo newGPO = domain.CreateGpo(displayName);
                newGPO.Description = description;

                // Get the strings that identify which ports needs to be restricted for both inbound rules as well as CSR
                string localPortString = GetLocalPortString(ports);
                string csrPortString = GetCSRPortString(ports);

                // Lookup the SIDs of the bypasgroups
                string excludeUsergroupSID = string.Empty;
                string excludeComputerGroupSID = string.Empty;
                string groupInScopeSID = string.Empty;

                // Lookup the SID of all the groups
                if (!string.IsNullOrEmpty(excludeUsergroup) && Properties.Settings.Default.LimitAccessToAdminAccounts)
                {
                    excludeUsergroupSID = ldapClient.GetObjectSID(excludeUsergroup, LDAP.ObjectType.group);
                    if (string.IsNullOrEmpty(excludeUsergroupSID))
                        throw new Exception($"Could not resolve SID of user group: {excludeUsergroup}");
                }


                if (!string.IsNullOrEmpty(excludeComputerGroup) && Properties.Settings.Default.LimitAccessToBastionHosts)
                {
                    excludeComputerGroupSID = ldapClient.GetObjectSID(excludeComputerGroup, LDAP.ObjectType.group);
                    if (string.IsNullOrEmpty(excludeComputerGroupSID))
                        throw new Exception($"Could not resolve SID of computer group: {excludeComputerGroup}");
                }


                groupInScopeSID = ldapClient.GetObjectSID(groupToAddInScope, LDAP.ObjectType.group);
                if (string.IsNullOrEmpty(groupInScopeSID))
                    throw new Exception($"Could not resolve SID of group that should be added in scope: {groupToAddInScope}");

                /*
                 * The GPO consists of three parts:
                 *  - The block rule that limits access on the given ports
                 *  - A connection security rule that establish an IPSEC connection between users and/ or hosts for the given range of ports
                 *  - A bypass rule that allows incoming connections from a given group of users and/ or hosts            
                 */

                // Create the blockrule
                string blockRule = GetInboundBlockRule(localPortString, displayName);

                // For IPSec connection, both user and computer must be authenticated.
                string connectionSecurityRule = GetConnectionSecurityRuleForBastionAndUser(csrPortString, displayName);

                // Depening if user account, hosts or both must be evaluated during authorization, create the appropiate bypassrule
                string bypassRule = string.Empty;
                if (!string.IsNullOrEmpty(excludeComputerGroup) && !string.IsNullOrEmpty(excludeUsergroup))
                {
                    bypassRule = GetBypassSettingForBastionAndUser(localPortString, displayName, excludeUsergroupSID, excludeComputerGroupSID);
                }
                else if (!string.IsNullOrEmpty(excludeUsergroup) && string.IsNullOrEmpty(excludeComputerGroup))
                {
                    bypassRule = GetBypassSettingForUser(localPortString, humanReadablePortString, excludeUsergroup, excludeUsergroupSID);
                }
                else if (string.IsNullOrEmpty(excludeUsergroup) && !string.IsNullOrEmpty(excludeComputerGroup))
                {
                    bypassRule = GetBypassSettingForBastion(localPortString, humanReadablePortString, excludeComputerGroup, excludeComputerGroupSID);
                }

                // Build the policy
                ComputerConfiguration computerConfig = newGPO.Computer;
                PolicySettings polSettings = computerConfig.Policy;
                RegistryPolicy regPol = polSettings.GetRegistry(false);

                Guid g = new Guid();
                g = Guid.NewGuid();
                regPol.WriteStringValue(Microsoft.Win32.RegistryHive.LocalMachine, REG_RULE_LOCATION, g.ToString(), blockRule);
                g = Guid.NewGuid();
                regPol.WriteStringValue(Microsoft.Win32.RegistryHive.LocalMachine, REG_RULE_LOCATION, g.ToString(), bypassRule);
                g = Guid.NewGuid();
                regPol.WriteStringValue(Microsoft.Win32.RegistryHive.LocalMachine, REG_CSR_LOCATION, g.ToString(), connectionSecurityRule);
                regPol.Save(true);

                // Add Group to GPO scope
                GPPermissionCollection GPOPermissionsCollection = newGPO.GetSecurityInfo();

                // Make sure that computers in scope can apply the policy                
                GPPermission GPOPermission = new GPPermission(groupInScopeSID, GPPermissionType.GpoApply, false);
                GPOPermissionsCollection.Add(GPOPermission);

                // authenticated users should not be able to apply the policy. Check if authenticated users is in the
                // permission collection and remove the entry.
                // Add the group to the collection with GpoRead permissions.
                IEnumerable<GPPermission> trustee = GPOPermissionsCollection.Where(t => t.Trustee.Sid.IsWellKnown(
                    System.Security.Principal.WellKnownSidType.AuthenticatedUserSid));

                if (trustee.Any())
                    GPOPermissionsCollection.RemoveTrustee(trustee.First().Trustee.Name);

                // Make sure that authenticated users can read the policy
                GPPermission UserReadPermission = new GPPermission("S-1-5-11", GPPermissionType.GpoRead, false);
                GPOPermissionsCollection.Add(UserReadPermission);

                // Set security info
                newGPO.SetSecurityInfo(GPOPermissionsCollection);

                if (!newGPO.IsAclConsistent())
                    newGPO.MakeAclConsistent();

                result = true;
            }
            catch (Exception ex)
            {
                Util.Logger.Log($"Error creating GPO: {ex.Message}");
            }

            return result;
        }

        /// <summary>
        /// Creates all the AD groups needed for the GPO.
        /// Will return a class with all properties set based on the configuration set by the user
        /// </summary>
        public static Models.GPOGroups CreateGroups(string name, string description, List<string> ComputerNames, LDAP ldapClient)
        {
            Models.GPOGroups groups = new Models.GPOGroups();
            string ScopeGroup, UserGroup, ComputerGroup;

            string _name = name.Replace(Properties.Settings.Default.GPONamePrefix, null);

            string computerBypassGroupName = $"{Properties.Settings.Default.LDAPGroupNamePrefix}{_name} - Computer Bypass";
            string userBypassGroupName = $"{Properties.Settings.Default.LDAPGroupNamePrefix}{_name} - User Bypass";
            string bypassDescription = $"Windows Firewall block bypass for GPO: {name}";

            try
            {
                // Create group to scope the GPO on
                ScopeGroup = ldapClient.GetObjectDistinguishedName(name, LDAP.ObjectType.group);
                if (string.IsNullOrEmpty(ScopeGroup))
                    ScopeGroup = ldapClient.CreateGroup(name, Properties.Settings.Default.GroupOU, description);

                // Add computer objects as member
                List<string> computerDNs = new List<string>();
                Parallel.ForEach(ComputerNames, (computer) =>
                {
                    string computerDN = ldapClient.GetAttribute(computer, "distinguishedName");
                    if (!string.IsNullOrEmpty(computerDN))
                        computerDNs.Add(computerDN);
                });
                ldapClient.AddGroupMember(ScopeGroup, computerDNs);

                // Create dedicated groups for exclusion
                if (Properties.Settings.Default.LimitAccessToAdminAccounts)
                {
                    // Create group and add group with admin users as member
                    UserGroup = ldapClient.CreateGroup(userBypassGroupName, Properties.Settings.Default.GroupOU, bypassDescription);
                    ldapClient.AddGroupMember(UserGroup, Properties.Settings.Default.AdminGroupDN);
                }

                if (Properties.Settings.Default.LimitAccessToBastionHosts)
                {
                    // Create group and add group with bastion hosts as member
                    ComputerGroup = ldapClient.CreateGroup(computerBypassGroupName, Properties.Settings.Default.GroupOU, bypassDescription);
                    ldapClient.AddGroupMember(ComputerGroup, Properties.Settings.Default.BastionGroupDN);
                }

                groups.ScopeGroupName = name;
                groups.ComputerBypassGroupName = computerBypassGroupName;
                groups.UserBypassGroupName = userBypassGroupName;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return groups;
        }
    }
}
