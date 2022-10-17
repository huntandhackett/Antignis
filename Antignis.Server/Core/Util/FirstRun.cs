using Microsoft.GroupPolicy;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;


namespace Antignis.Server.Core.Util
{
    internal sealed class Startup
    {

        // This class is used to determine if the user starts the tool for the first time
        // and if so, guide the user to ingest most of the data from AD

        /// <summary>
        /// This function checks if all needed info is present
        /// If information is missing, a flow is started that guides the user to enter all needed information
        /// </summary>
        /// <returns></returns>
        public static bool CheckData(Core.ADDS.LDAP ldapClient)
        {

            if (Properties.Settings.Default.FirstRunCompleted)
                return true;

            bool result = true;
            bool hasBastion = false;
            bool hasUsers = false;

            Console.WriteLine("\r\nIn order for this tool to function correctly, a few details must be configured:");
            Console.WriteLine("\t - Required: A group with IT-only administrative accounts");
            Console.WriteLine("\t - Required: The distinguishedName of the Organization Unit in which groups will be created");
            Console.WriteLine("\t - Optional: A group with IT-only bastion hosts\r\n");

            // Ask for the group containing admin users
            if (string.IsNullOrEmpty(Properties.Settings.Default.AdminGroupDN))
            {
                Console.Write("\r\nDo you have a dedicated group with IT-only admin accounts in it? (Y/n): ");
                string ans = Console.ReadLine();

                if (ans.ToLower() == "y")
                {
                    Console.Write("What is the name of the group: ");
                    ans = Console.ReadLine();

                    // Search AD for the group. Fetch the objectSID
                    //var objSid = ldapClient.GetObjectSID(ans);
                    string groupDN = ldapClient.GetObjectDistinguishedName(ans, ADDS.LDAP.ObjectType.group);

                    if (groupDN == null)
                    {
                        Console.WriteLine("Could not find the group. Check spelling or create the group and run the tool again.");
                        return false;
                    }
                    else
                    {
                        Properties.Settings.Default.AdminGroupDN = groupDN;
                        Properties.Settings.Default.Save();
                        hasUsers = true;
                    }

                }
                else
                {
                    Console.WriteLine("Please create an IT admin group with all IT users in it and run the tool again.");
                    return false;
                }
            }
            else { hasUsers = true; }

            // Ask for the group containing bastion hosts
            if (string.IsNullOrEmpty(Properties.Settings.Default.BastionGroupDN))
            {
                Console.Write("\r\nDo you use bastion hosts/ stepping stones for administrative and privileged IT work? (Y/n): ");
                string ans = Console.ReadLine();

                if (ans.ToLower() == "y")
                {
                    Console.Write("Add all the hosts into a single AD group - if not already - and enter the name of the group: ");
                    ans = Console.ReadLine();

                    // Search AD for the group. Fetch the objectSID
                    //var objSid = ldapClient.GetObjectSID(ans);
                    string groupDN = ldapClient.GetObjectDistinguishedName(ans, ADDS.LDAP.ObjectType.group);

                    if (groupDN == null)
                    {
                        Console.WriteLine("Could not find the group. Check spelling or create the group and run the tool again.");
                        return false;
                    }
                    else
                    {
                        Properties.Settings.Default.BastionGroupDN = groupDN;
                        Properties.Settings.Default.Save();
                        hasBastion = true;
                    }
                }
                else
                {
                    Properties.Settings.Default.BastionGroupDN = "-";
                }
            }
            else { hasBastion = true; }


            // Ask for the OU to store groups in
            if (string.IsNullOrEmpty(Properties.Settings.Default.GroupOU))
            {
                Console.Write("\r\nEnter the distinguished name of the Organizational Unit where new groups should be created. This path should start with OU=: ");
                string ans = Console.ReadLine();

                if (!ans.ToLower().StartsWith("ou="))
                {
                    Console.WriteLine("Input should be a distinguishedName.\r\nThis article might be helpful:\r\n\thttps://support.xink.io/support/solutions/articles/1000246165-how-to-find-the-distinguishedname-of-an-ou-");
                    return false;
                }
                else
                {
                    Properties.Settings.Default.GroupOU = ans;
                    Properties.Settings.Default.Save();
                }
            }

            // Ask if access should be limited to admin accounts, bastion hosts or both
            if (hasBastion && hasUsers)
            {
                Console.WriteLine("\r\nTo bypass block rules, exceptions can be made for user accounts or the combination of user account and bastion host.");
                Console.WriteLine("If you choose to do both, only connections from a user account in the admin group originating from a bastion host in the bastion host group, will be accepted.");
                Console.Write("Do you want to limit access to admin user accounts (A) or both? (B)? (B): ");
                string limitAccessTo = Console.ReadLine();

                switch (limitAccessTo.ToLower())
                {
                    case "a":
                        Properties.Settings.Default.LimitAccessToAdminAccounts = true;
                        Properties.Settings.Default.LimitAccessToBastionHosts = false;
                        break;

                    case "b":
                        Properties.Settings.Default.LimitAccessToAdminAccounts = true;
                        Properties.Settings.Default.LimitAccessToBastionHosts = true;
                        break;

                    default:
                        Console.WriteLine("Invalid answer. Access will be limited to user accounts only.");
                        Properties.Settings.Default.LimitAccessToAdminAccounts = true;
                        Properties.Settings.Default.LimitAccessToBastionHosts = false;
                        break;
                }
            }


            Properties.Settings.Default.FirstRunCompleted = true;
            Properties.Settings.Default.Save();
            return result;

        }

        /// <summary>
        /// Imports workstation from AD into the database
        /// </summary>
        /// <param name="ldapClient"></param>
        /// <param name="dbClient"></param>
        public static void ImportWorkStationsFromAD(Core.ADDS.LDAP ldapClient, Core.SQL.Database dbClient)
        {
            //if (Properties.Settings.Default.WorkstationsImported)
            //    return;

            // Search workstations in AD and import results into the database
            List<Models.Host> workstations = ldapClient.GetWindowsWorkstationObjects();
            dbClient.AddHostRecord(workstations);

            Properties.Settings.Default.WorkstationsImported = true;
            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// Imports servers from AD into the database
        /// </summary>
        /// <param name="ldapClient"></param>
        /// <param name="dbClient"></param>
        public static void ImportServersFromAD(Core.ADDS.LDAP ldapClient, Core.SQL.Database dbClient)
        {
            // if (Properties.Settings.Default.ServersImported)
            //     return;

            // Search servers in AD and import results into the database
            List<Models.Host> servers = ldapClient.GetWindowsServerObjects();
            dbClient.AddHostRecord(servers);

            Properties.Settings.Default.ServersImported = true;
            Properties.Settings.Default.Save();

        }

        /// <summary>
        /// Removes all settings to start over a configuration
        /// </summary>
        public static void ResetConfig()
        {
            Properties.Settings.Default.ServersImported = false;
            Properties.Settings.Default.WorkstationsImported = false;
            Properties.Settings.Default.FirstRunCompleted = false;
            Properties.Settings.Default.LimitAccessToAdminAccounts = false;
            Properties.Settings.Default.LimitAccessToBastionHosts = false;
            Properties.Settings.Default.GroupOU = String.Empty;
            Properties.Settings.Default.AdminGroupDN = String.Empty;
            Properties.Settings.Default.BastionGroupDN = String.Empty;

            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// Function to check if RSAT is installed. Returns false is not
        /// </summary>
        /// <returns></returns>
        public static bool RSATInstalled()
        {
            // Really simple test: create an instance of the GPDomain class.
            // If an FileNotFound Exception occurs, RSAT is not installed
            bool result = false;

            try
            {
                GPDomain gpDomain = new GPDomain();
                result = true;
            }
            catch (FileNotFoundException fEx)
            {
                Logger.Log("RSAT is not installed: " + fEx.Message);
            }
            catch (UnauthorizedAccessException uEx)
            {
                Logger.Log("Not enough access to create GPOs in this domain: " + uEx.Message);
            }
            catch (Exception ex)
            {
                Logger.LogDebug("Other error occured while creating GPDomain instance:\r\n\t" + ex.Message);
            }

            return result;
        }

        /// <summary>
        /// Creates a GPO that creates a firewall rules baseline that block inbound traffic on known RCE ports
        /// </summary>
        public static void CreateFirewallBaseline(Core.ADDS.LDAP ldapClient, Core.SQL.Database dbClient, bool isServer = false)
        {
            string endpoint = isServer ? "Servers" : "Workstations";

            string groupName = $"{Properties.Settings.Default.LDAPGroupNamePrefix} Domain {endpoint} Baseline";
            string policyName = $"{Properties.Settings.Default.GPONamePrefix} Domain {endpoint} Baseline";

            string description = "Provides a firewall configuration baseline for all hosts in scope.";

            // Fetch all RCE ports
            List<int> RCEports = Core.Net.Utils.GetRCEPorts();
            RCEports.Sort();

            // Remove ports that needs to be skipped
            RCEports = SubtractPortsFromRCEPortConfig(RCEports, isServer);

            // Query all workstations
            List<Models.Host> hosts = dbClient.GetHosts(isServer);
            if (hosts.Count <= 0)
                return;

            // Check if dedicated AD group already exists, create if not
            string ADGroupDN = ldapClient.GetObjectDistinguishedName(groupName, Core.ADDS.LDAP.ObjectType.group);
            if (ADGroupDN == null)
            {
                ADGroupDN = ldapClient.CreateGroup(groupName, Properties.Settings.Default.GroupOU,
                    $"Contains all {endpoint.ToLower()} that have their firewall configured via Group Policy using Antignis");
            }

            // Add all workstations to AD group
            List<string> computerDNs = new List<string>();
            Parallel.ForEach(hosts, (computer) =>
            {
                string computerDN = ldapClient.GetAttribute(computer.DNSHostname, "distinguishedName");
                if (!string.IsNullOrEmpty(computerDN))
                    computerDNs.Add(computerDN);

                Logger.LogDebug($"[CreatePolicy] Resolved '{computer}' to '{computerDN}'");
            });
            ldapClient.AddGroupMember(ADGroupDN, computerDNs);
            Logger.LogVerbose($"[CreatePolicy] Added {computerDNs.Count} computer accounts to '{ADGroupDN}'");

            // Some static info about the groups
            string computerBypassGroupName = $"{Properties.Settings.Default.LDAPGroupNamePrefix} {endpoint} Baseline Computer Bypass";
            string userBypassGroupName = $"{Properties.Settings.Default.LDAPGroupNamePrefix} {endpoint} Baseline User Bypass";
            string Description = $"Group used as baseline to block traffic on the following ports: {string.Join(",", RCEports)}";
            string bypassDescription = "Windows Firewall block bypass for ports blocked in Baseline GPO";

            string userGroup = string.Empty;
            string bastionGroup = string.Empty;

            // Create dedicated AD groups for exclusion groups as well
            if (Properties.Settings.Default.LimitAccessToAdminAccounts)
            {
                userGroup = ldapClient.CreateGroup(userBypassGroupName, Properties.Settings.Default.GroupOU, bypassDescription);
                Logger.LogVerbose($"[CreatePolicy] Got AD group: {userBypassGroupName}");

                // Add IT admin group as member of this group
                ldapClient.AddGroupMember(userGroup, Properties.Settings.Default.AdminGroupDN);
            }

            if (Properties.Settings.Default.LimitAccessToBastionHosts)
            {
                bastionGroup = ldapClient.CreateGroup(computerBypassGroupName, Properties.Settings.Default.GroupOU, bypassDescription);
                Logger.LogVerbose($"[CreatePolicy] Got AD group: {computerBypassGroupName}");

                // Add bastion host group as member of this group
                ldapClient.AddGroupMember(bastionGroup, Properties.Settings.Default.BastionGroupDN);
            }

            // Create the policy

            Core.ADDS.GPO.Create(policyName, description, RCEports, groupName, userGroup, bastionGroup, ldapClient, "Baseline");
        }

        /// <summary>
        /// Removes ports from the list that are put on the ignore list
        /// </summary>
        /// <param name="isServer">true if server config. The ports to remove from the list varies between workstations and servers</param>
        /// <returns></returns>
        private static List<int> SubtractPortsFromRCEPortConfig(List<int> RCEPortList, bool isServer)
        {
            System.Collections.Specialized.StringCollection input = isServer ? Properties.Settings.Default.SkipPortsServerBaseline : Properties.Settings.Default.SkipPortsWorkstationBaseline;
            if (input == null)
                return RCEPortList;

            foreach (string i in input)
            {
                try
                {
                    RCEPortList.Remove(Convert.ToInt32(i));
                }
                catch
                { // casting exception. Skip the port, do nothing
                }
            }

            return RCEPortList;
        }

    }
}
