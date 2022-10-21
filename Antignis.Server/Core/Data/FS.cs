using System;
using System.IO;
using System.Security.AccessControl;

namespace Antignis.Server.Core.Data
{
    internal class FS
    {
        /// <summary>
        /// Create a new directory with fullcontrol for domain admins and writefiles permission for authenticated users
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        public static bool CreateWriteOnlyDirectory(string location)
        {

            bool result = false;

            System.Security.Principal.SecurityIdentifier domainSid = System.Security.Principal.WindowsIdentity.GetCurrent().User.AccountDomainSid;

            if (domainSid == null)
            {
                Util.Logger.Log($"Please use this function from a domain joined user");
                return false;
            }

            System.Security.Principal.SecurityIdentifier domainAdminGroup = new System.Security.Principal.SecurityIdentifier(
                    System.Security.Principal.WellKnownSidType.AccountDomainAdminsSid, domainSid);

            System.Security.Principal.SecurityIdentifier authenticatedUser = new System.Security.Principal.SecurityIdentifier(
                    System.Security.Principal.WellKnownSidType.AuthenticatedUserSid, domainSid);

            try
            {

                //  Check if directory exists. Error out if it does
                if (Directory.Exists(location))
                {
                    // This message has already been given at sanity check
                    Util.Logger.Log($" The directory at '{location}' already exists. Please specify a new location");
                    return false;
                }

                // Create directory
                DirectoryInfo dirInfo = Directory.CreateDirectory(location);

                // Remove inherited permissions and grant fullcontrol access to Domain Admins and writeonly permissions to authenticated users
                DirectorySecurity dirACL = dirInfo.GetAccessControl();
                dirACL.SetAccessRuleProtection(true, false);
                dirACL.AddAccessRule(new FileSystemAccessRule(domainAdminGroup, FileSystemRights.FullControl, AccessControlType.Allow));

                // Make sure the user can write files
                dirACL.AddAccessRule(new FileSystemAccessRule(authenticatedUser, FileSystemRights.CreateFiles, AccessControlType.Allow));

                // Make sure that users can read permissions to restore the ACL
                dirACL.AddAccessRule(new FileSystemAccessRule(authenticatedUser, FileSystemRights.ReadPermissions, AccessControlType.Allow));

                // Write ACL
                dirInfo.SetAccessControl(dirACL);

                Core.Util.Logger.LogDebug($"[CreateWriteOnlyDirectory] Granted Fullcontrol for Domain Administrators on {location}");
                Core.Util.Logger.LogDebug($"[CreateWriteOnlyDirectory] Granted CreateFiles permission for Authenticated Users on {location}");

                result = true;
            }
            catch (UnauthorizedAccessException uEx)
            {
                Util.Logger.Log("Access denied. Are you sure this account has enough access?");
                Core.Util.Logger.LogDebug($"[CreateWriteOnlyDirectory] {uEx.StackTrace}");
            }
            catch (Exception ex)
            {
                Core.Util.Logger.Log(ex.Message);
                Core.Util.Logger.LogDebug($"[CreateWriteOnlyDirectory] {ex.StackTrace}");
            }

            return result;
        }
    }
}
