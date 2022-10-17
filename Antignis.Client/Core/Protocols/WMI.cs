using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;

namespace Antignis.Client.Core.Protocols
{
    internal class WMI
    {
        /// <summary>
        /// Lists shares that are exposed on this computer
        /// </summary>
        /// <returns></returns>
        public static List<Core.Models.FileShare> ListShares()
        {
            List<string> blocklist = new List<string>() { "ADMIN$", "IPC$" };
            blocklist.AddRange(GetDrives());

            List<Core.Models.FileShare> Shares = new List<Core.Models.FileShare>();
            ManagementClass wmi_shares = new ManagementClass("Win32_Share");
            try
            {
                ManagementObjectCollection specificShares = wmi_shares.GetInstances();
                foreach (ManagementObject share in specificShares)
                {
                    string sharename = share.Properties["name"].Value.ToString();

                    if (!blocklist.Contains(sharename.ToUpper()))
                        Shares.Add(new Models.FileShare()
                        {
                            Name = sharename
                        });
                }
            }
            catch (Exception ex)
            {
                Misc.WriteBad(ex.Message);
            }
            finally
            {
                wmi_shares.Dispose();
            }

            return Shares;
        }

        /// <summary>
        /// Returns all logical drives as a sharename 
        /// </summary>
        /// <returns></returns>
        private static List<string> GetDrives()
        {
            List<string> driveList = new List<string>();
            var drives = System.IO.Directory.GetLogicalDrives();
            foreach (var drive in drives)
            {
                driveList.Add(drive.Replace(":\\", "$"));
            }

            return driveList;
        }

        /// <summary>
        /// Lists server roles enabled on this computer
        /// </summary>
        /// <returns></returns>
        public static List<Core.Models.Role> ListServerRoles()
        {
            List<Core.Models.Role> Features = new List<Core.Models.Role>();

            ManagementClass W32Features = null;
            try
            {
                W32Features = new ManagementClass("Win32_ServerFeature");
                ManagementObjectCollection featureCollection = W32Features.GetInstances();
                foreach (ManagementObject feature in featureCollection)
                {
                    //int ID = Convert.ToInt32();
                    Features.Add(new Core.Models.Role()
                    {
                        Name = feature.Properties["Name"].Value.ToString()
                    });
                }
            }
            catch (Exception ex)
            {
                // Check for Invalid Class message
                Misc.WriteBad(ex.Message);
            }
            finally
            {
                W32Features.Dispose();
            }

            return Features;
        }
    }
}
