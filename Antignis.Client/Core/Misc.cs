using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Antignis.Client.Core
{
    internal class Misc
    {

        /// <summary>
        /// Queries OS version name (e.g. Windows 10, Windows Server 2019) from the registry
        /// </summary>
        /// <returns></returns>
        public static string GetOSVersion()
        {
            string result = "Unknown";

            try
            {
                string subKey = @"SOFTWARE\Wow6432Node\Microsoft\Windows NT\CurrentVersion";
                RegistryKey key = Registry.LocalMachine;
                RegistryKey skey = key.OpenSubKey(subKey);
                result = (string)skey.GetValue("ProductName");

                if (skey != null)
                {
                    skey.Close();
                    skey.Dispose();
                }

                if (key != null)
                {
                    key.Close();
                    key.Dispose();
                }

            }
            catch (Exception)
            {
                // Skip for now
            }

            return result;
        }

        /// <summary>
        /// Generates a random alphanumeric ID
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string GenerateId(int length = 4)
        {
            string lCase = "abcdefghijklmnopqrstuvwxyz";
            string uCase = lCase.ToUpper();
            string digit = "0123456789";
            char[] chars = string.Join("", lCase, uCase, digit).ToCharArray();

            StringBuilder result = new StringBuilder();

            Random rnd = new Random();
            for (int i = 0; i < length; i++)
            {
                int index = rnd.Next(0, chars.Length - 1);
                result.Append(chars[index]);
            }

            return result.ToString();
        }

        /// <summary>
        /// Writes a generic message to the console
        /// </summary>
        /// <param name="msg"></param>
        public static void WriteGood(string msg)
        {
            if (Program.Verbose)
                Console.WriteLine("[+] {0}", msg);
        }

        /// <summary>
        /// Writes an error message to the console
        /// </summary>
        /// <param name="msg"></param>
        public static void WriteBad(string msg)
        {
            if (Program.Verbose)
                Console.WriteLine("[-] {0}", msg);
        }

        /// <summary>
        /// Used to update the same line with new data
        /// </summary>
        /// <param name="msg"></param>
        public static void WriteUpdate(string msg)
        {
            if (Program.Verbose)
                Console.Write("\r[*] {0}   ", msg);
        }

        /// <summary>
        /// Saves jsonFile to specified location with a unique ID as filename
        /// </summary>
        /// <param name="jsonObject"></param>
        /// <param name="saveLocation"></param>
        public static bool SaveFile(string jsonObject, string saveLocation)
        {
            string fName = string.Format(@"{0}\{1}.json", saveLocation, GenerateId(15));
            bool result = true;
            try
            {
                System.IO.File.WriteAllText(fName, jsonObject, Encoding.UTF8);
                SetACL(saveLocation, fName);
            }
            catch (Exception ex)
            {
                WriteBad(ex.Message + "\r\n\r\n" + ex.StackTrace);
                result = false;
            }
            WriteGood("Json file written to: " + fName);
            return result;
        }

        /// <summary>
        /// Copies ACL from parent to new file. 
        /// </summary>
        /// <param name="saveLocation"></param>
        /// <param name="fileName"></param>
        private static void SetACL(string saveLocation, string fileName)
        {

            System.IO.FileInfo fInfo = new System.IO.FileInfo(fileName);
            System.IO.DirectoryInfo dirInfo = new System.IO.DirectoryInfo(saveLocation);

            var dirACL = dirInfo.GetAccessControl();
            var fileAcl = fInfo.GetAccessControl();

            // SDDL or binary form would also work, bt requires additional seSecurityPrivilege
            foreach (System.Security.AccessControl.FileSystemAccessRule rule in
                dirACL.GetAccessRules(true, true, typeof(System.Security.Principal.NTAccount)))
            {
                fileAcl.AddAccessRule(rule);
            }

            fileAcl.PurgeAccessRules(System.Security.Principal.WindowsIdentity.GetCurrent().User);

            fileAcl.SetAccessRuleProtection(false, true);
            fInfo.SetAccessControl(fileAcl);
        }


        /// <summary>
        /// Checks whether the given regkey exists
        /// </summary>
        /// <param name="regkey"></param>
        /// <returns></returns>
        public static bool RegKeyExists()
        {
            bool result = true;

            //HKEY_LOCAL_MACHINE\SOFTWARE\HH\Run = 1
            string regkey = @"HKEY_LOCAL_MACHINE\SOFTWARE\HH";
            string value = "Run";
            try
            {
                object t = Registry.GetValue(regkey, value, false);
                result = Convert.ToInt32(t) == 1;
            }
            catch (Exception)
            {
                result = false;
            }


            return result;
        }

        /// <summary>
        /// Sets regkey value to 1
        /// </summary>
        public static void SetRegKey()
        {
            string regkey = @"HKEY_LOCAL_MACHINE\SOFTWARE\HH";
            string value = "Run";
            try
            {
                Registry.SetValue(regkey, value, 1, RegistryValueKind.DWord);
            }
            catch (Exception)
            {
                // Do nothing
            }
        }

        /// <summary>
        /// This function returns a list of all the installed programs
        /// </summary>
        /// <returns></returns>
        public static List<Core.Models.Program> GetInstalledPrograms()
        {
            // we need to check in two locations if OS is 64 bit
            bool is64Bit = Environment.Is64BitOperatingSystem;

            List<Core.Models.Program> result = new List<Core.Models.Program>();

            try
            {
                string registry_key = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
                string registry_key64 = @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall";

                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(registry_key))
                {
                    foreach (string subkey_name in key.GetSubKeyNames())
                    {
                        using (RegistryKey subkey = key.OpenSubKey(subkey_name))
                        {
                            if (subkey.GetValue("DisplayName") != null)
                            {
                                string name = subkey.GetValue("DisplayName").ToString();
                                if (result.Where(c => c.Name == name).Count() == 0)
                                    result.Add(new Models.Program() { Name = name });
                            }
                        }
                    }
                }

                if (is64Bit)
                {
                    using (RegistryKey key = Registry.LocalMachine.OpenSubKey(registry_key64))
                    {
                        foreach (string subkey_name in key.GetSubKeyNames())
                        {
                            using (RegistryKey subkey = key.OpenSubKey(subkey_name))
                            {
                                if (subkey.GetValue("DisplayName") != null)
                                {
                                    string name = subkey.GetValue("DisplayName").ToString();
                                    if (result.Where(c => c.Name == name).Count() == 0)
                                        result.Add(new Models.Program() { Name = name });
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Do nothing for now
            }

            return result;

        }
    }
}
