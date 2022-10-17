using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Management;
using System.Text;

namespace Antignis.Server.Core.ADDS
{
    public sealed class LDAP
    {

        #region Properties

        /// <summary>
        /// Username to authenticate to AD
        /// </summary>
        private string username { get; set; }

        /// <summary>
        /// Password to authenticate to AD
        /// </summary>
        private string password { get; set; }

        /// <summary>
        /// Name of the AD domain
        /// </summary>
        private string domain { get; set; }

        /// <summary>
        /// Full FQDN of the domain name
        /// </summary>
        public string domainFQDN { get; set; }

        /// <summary>
        /// Distinguishedname of the authenticated useraccount
        /// </summary>
        private string userDistinguishedName { get; set; }

        /// <summary>
        /// DirectoryEntry that points to user account DN
        /// </summary>
        private DirectoryEntry DirectoryEntry;

        /// <summary>
        /// FQDN of the domaincontroller
        /// </summary>
        private string domainController;

        /// <summary>
        /// DisintguishedName of the domain
        /// </summary>
        private string domainDistinguishedName;

        /// <summary>
        /// Used to determine whether we use integrated authentication to interact with AD
        /// </summary>
        private readonly bool useIntegratedAuth = true;

        /// <summary>
        /// Use LDAP over TLS by default. 
        /// </summary>
        private readonly bool UseLDAPS = true;

        /// <summary>
        /// Boolean to identify if connection to LDAP could be made
        /// </summary>
        public bool IsConnected = false;

        #endregion

        #region LDAPhelpers

        /// <summary>
        /// Creates a new LDAP path, while taking all parameters such as DC and protocol into account
        /// </summary>
        /// <param name="DN"></param>
        /// <returns></returns>
        private string GetLDAPPath(string DN)
        {
            string toConnect = "LDAP://" + domainController;
            if (UseLDAPS)
                toConnect += ":636";

            // Add DN to connectionString
            toConnect += "/" + DN;

            return toConnect;
        }

        /// <summary>
        /// method to test whether a connection to LDAP can be made by doing an actual search
        /// </summary>
        /// <returns></returns>
        private bool TestLDAPConnection()
        {
            bool result = false;

            try
            {
                DirectoryEntry dirEntry = getDirEntry();
                DirectorySearcher dirSearcher = new DirectorySearcher(dirEntry);
                //dirSearcher.Filter = string.Format("(&(sAMAccountName={0}))", this.username);
                dirSearcher.Filter = "(&(objectClass=domain))";
                SearchResult sResult = dirSearcher.FindOne();

                if (DirectoryEntry == null)
                    DirectoryEntry = sResult.GetDirectoryEntry();

                result = !string.IsNullOrEmpty(DirectoryEntry.Path);

                if (dirEntry != null)
                    dirEntry.Dispose();

                if (dirSearcher != null)
                    dirSearcher.Dispose();

            }
            catch (Exception)
            {
                // just ignore it for now
            }

            return result;
        }

        /// <summary>
        /// Cleanup
        /// </summary>
        public void Dispose()
        {
            username = string.Empty;
            password = string.Empty;
            domain = string.Empty;

            try
            {
                DirectoryEntry.Close();
                DirectoryEntry.Dispose();
            }
            catch { }
        }

        /// <summary>
        /// Extracts value of given attribute from searchresult into a string.
        /// Returns empty string if attribute is not present
        /// </summary>
        /// <param name="sResult"></param>
        /// <param name="attribute"></param>
        /// <returns></returns>
        private string GetAttributeValue(SearchResult sResult, string attribute)
        {
            string result = "";
            try
            {
                object ldapObjAttr = sResult.Properties[attribute][0];

                // Some objects can be byte[]            
                if (ldapObjAttr.GetType() == (new byte[1]).GetType())
                {
                    string asciiString = Encoding.ASCII.GetString((byte[])ldapObjAttr);
                    ldapObjAttr = asciiString;
                }

                result = ldapObjAttr.ToString();
            }
            catch
            {
                // Do nothing
            }


            return result;
        }

        /// <summary>
        /// Returns an authenticated directoryContext object
        /// </summary>
        /// <returns></returns>
        private DirectoryContext GetDirectoryContext(DirectoryContextType ctxType = DirectoryContextType.Domain)
        {

            DirectoryContext ctx = new DirectoryContext(ctxType);

            if (!useIntegratedAuth)
                ctx = new DirectoryContext(ctxType, domain, username, password);

            return ctx;
        }

        /// <summary>
        /// returns authenticated DirectoryEntry based on provided information
        /// </summary>
        /// <returns></returns>
        private DirectoryEntry getDirEntry(string distinguishedName = null)
        {
            string dn = string.IsNullOrEmpty(distinguishedName) ? domainDistinguishedName : distinguishedName;
            string toConnect = GetLDAPPath(dn);

            DirectoryEntry dirEntry;
            if (useIntegratedAuth)
            {
                dirEntry = new DirectoryEntry(toConnect);
            }
            else
            {
                dirEntry = new DirectoryEntry(toConnect, username, password);
            }

            return dirEntry;

        }

        /// <summary>
        /// extracts username and domain from  user provided input        
        /// </summary>
        private void extractUsernames()
        {
            if (username.Contains("\\"))
            {
                username = username.Split('\\')[1];

                if (string.IsNullOrEmpty(domain))
                    domain = username.Split('\\')[0];
            }
        }

        /// <summary>
        /// Extracts user domain name
        /// </summary>
        private void getDomainName()
        {
            domain = System.Environment.UserDomainName;
        }

        /// <summary>
        /// Extract user name
        /// </summary>
        private void getUserName()
        {
            username = System.Environment.UserName;
        }

        /// <summary>
        /// Retrieves name of a rnadom domain controller
        /// </summary>
        /// <returns></returns>
        private List<string> getDomainControllerNames()
        {
            List<string> result = new List<string>();

            DirectoryContext ctx = GetDirectoryContext(DirectoryContextType.Domain);

            // Fetch a static domain controller to avoid replication and latency issues
            Domain domain = Domain.GetDomain(ctx);
            DomainControllerCollection domainControllers = domain.DomainControllers;

            foreach (DomainController dc in domainControllers)
            {
                result.Add(dc.Name);
            }

            // cleanup
            if (domain != null)
                domain.Dispose();

            return result;
        }

        /// <summary>
        /// Returns distinguishedname of the target domain
        /// </summary>
        /// <returns></returns>
        private string getDomainDistinguishedName()
        {

            string result = string.Empty;
            DirectoryContext ctx = GetDirectoryContext(DirectoryContextType.Domain);

            Domain domain = Domain.GetDomain(ctx);
            result = domain.GetDirectoryEntry().Path.Split('/').Last();

            // cleanup
            if (domain != null)
                domain.Dispose();

            return result;
        }

        /// <summary>
        /// Initialization method that is called from within the constructor
        /// This method will lookup relevant information needed to interact with LDAP, such as domain distinguishedName
        /// and will select a designated domaincontroller to interact with
        /// 
        /// An LDAP test connection will be made. IF this fails, an exception will be thrown
        /// </summary>
        /// <exception cref="Exception"></exception>
        private void Init()
        {

            domainDistinguishedName = getDomainDistinguishedName();
            //this.Attribute = attributeName;
            //this.AttributeUpperRange = getUpperRangeAttribute(this.Attribute);

            List<string> DomainControllers = getDomainControllerNames();
            domainController = DomainControllers[new Random().Next(0, DomainControllers.Count - 1)];

            // Fetch the full domain FQDN
            domainFQDN = GetDomainFQDNFromDN();


            IsConnected = TestLDAPConnection();

        }

        /// <summary>
        /// Generates a FQDN domain name based on domain DN
        /// </summary>
        /// <returns></returns>
        private string GetDomainFQDNFromDN()
        {
            List<string> parts = new List<string>();
            string[] splits = domainDistinguishedName.Split(',');
            foreach (string s in splits)
                parts.Add(s.Split('=')[1]);

            return String.Join(".", parts.ToArray());
        }

        /// <summary>
        /// Method to query the AD and returns the result in a searchResultCollection
        /// </summary>
        /// <param name="query"></param>
        /// <param name="propertiesToAdd"></param>
        /// <param name="scope"></param>
        /// <returns></returns>
        private SearchResultCollection Searcher(string query, string[] propertiesToAdd, SearchScope scope = SearchScope.Subtree)
        {
            string result = string.Empty;
            SearchResultCollection sResult = null;
            try
            {
                DirectorySearcher directorySearcher = new DirectorySearcher(DirectoryEntry);

                directorySearcher.Filter = query;
                directorySearcher.SearchScope = scope;
                directorySearcher.PropertiesToLoad.AddRange(propertiesToAdd);
                sResult = directorySearcher.FindAll();

                if (directorySearcher != null)
                    directorySearcher.Dispose();

            }
            catch (Exception e)
            {
                Util.Logger.Log("[-]" + e.Message.ToString());
            }
            return sResult;
        }

        /// <summary>
        /// Changes the rootPath of the directoryentry to a new DN
        /// PAth should NOT start with LDAP://
        /// </summary>
        /// <param name="path"></param>
        private void SetLDAPPath(string path)
        {
            string toConnect = GetLDAPPath(path);
            DirectoryEntry.Path = toConnect;
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Establish connection to LDAP
        /// </summary>
        /// <param name="attributeName"></param>
        public LDAP(bool UseLDAPS)
        {
            this.UseLDAPS = UseLDAPS;

            if (string.IsNullOrEmpty(username))
            {
                getUserName();
                getDomainName();
            }

            Init();

        }

        /// <summary>
        /// Establish connection to LDAP using username, password, domain
        /// </summary>
        /// <param name="Username"></param>
        /// <param name="Password"></param>
        /// <param name="Domain"></param>
        /// <param name="UseLDAPS"></param>
        public LDAP(string Username, string Password, string Domain, bool UseLDAPS)
        {
            username = Username;
            password = Password;
            domain = Domain;
            useIntegratedAuth = false;
            this.UseLDAPS = UseLDAPS;

            Init();
        }

        /// <summary>
        /// Will connect to LDAP using username, password. The domain will be extracted from the username if present
        /// if not, the domain name will be lookup up.
        /// </summary>
        /// <param name="Username"></param>
        /// <param name="Password"></param>
        /// <param name="UseLDAPS"></param>
        public LDAP(string Username, string Password, bool UseLDAPS)
        {
            username = Username;
            password = Password;

            extractUsernames();
            if (string.IsNullOrEmpty(domain))
                getDomainName();

            useIntegratedAuth = false;
            this.UseLDAPS = UseLDAPS;

            Init();
        }

        #endregion

        #region PublicMethods

        /// <summary>
        /// Returns a list with Windows Workstations.
        /// It queries AD for computer object with Windows wildcard in attribute operatingsystem,
        /// but must not contain any "server" in the same attribute
        /// </summary>
        /// <returns></returns>
        public List<Core.Models.Host> GetWindowsWorkstationObjects()
        {
            // Set default path to domain root
            SetLDAPPath(domainDistinguishedName);

            // Build the query
            string query = $"(&(objectClass=computer)(operatingSystem=*Windows*)(!(operatingSystem=*server*)))";
            string[] properties = new string[] { "operatingSystem", "Name", "dNSHostName" };

            SearchResultCollection searchCol = Searcher(query, properties);

            if (searchCol == null || searchCol.Count <= 0)
            {
                return null;
            }

            List<Core.Models.Host> Workstations = new List<Core.Models.Host>();
            foreach (SearchResult result in searchCol)
            {
                string dnsHostname = GetAttributeValue(result, "dNSHostName");
                string operatingSystem = GetAttributeValue(result, "operatingSystem");
                Workstations.Add(new Core.Models.Host
                {
                    IsServerOS = false,
                    DNSHostname = dnsHostname,
                    OperatingSystem = operatingSystem

                });
            }

            return Workstations;
        }

        /// <summary>
        /// Returns a list with Windows Server.
        /// It queries AD for computer object with Windows wildcard in attribute operatingsystem,
        /// but must contain any "server" in the same attribute
        /// </summary>
        /// <returns></returns>
        public List<Core.Models.Host> GetWindowsServerObjects()
        {
            // Set default path to domain root
            SetLDAPPath(domainDistinguishedName);

            // Build the query
            string query = $"(&(objectClass=computer)(operatingSystem=*Windows*)((operatingSystem=*server*)))";
            string[] properties = new string[] { "operatingSystem", "Name", "dNSHostName" };

            SearchResultCollection searchCol = Searcher(query, properties);

            if (searchCol == null || searchCol.Count <= 0)
            {
                return null;
            }

            List<Core.Models.Host> Servers = new List<Core.Models.Host>();
            foreach (SearchResult result in searchCol)
            {
                string dnsHostname = GetAttributeValue(result, "dNSHostName");
                string operatingSystem = GetAttributeValue(result, "operatingSystem");
                Servers.Add(new Core.Models.Host
                {
                    IsServerOS = true,
                    DNSHostname = dnsHostname,
                    OperatingSystem = operatingSystem

                });
            }


            return Servers;
        }

        /// <summary>
        /// Finds object with givenName and returns readable objectSid in string format
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string GetObjectSID(string name, ObjectType objectType)
        {
            // Set default path to domain root
            SetLDAPPath(domainDistinguishedName);

            string sid = "";
            string[] props = new string[] { "objectSid" };
            SearchResultCollection coll = Searcher($"(&(|(name={name})(displayName={name})(distinguishedName={name}))(objectClass={objectType}))", props);

            if (coll != null && coll.Count > 0)
            {
                // Only return the sid of the first object
                byte[] data = new byte[] { };
                data = (byte[])coll[0].Properties["objectSid"][0];
                sid = new System.Security.Principal.SecurityIdentifier(data, 0).ToString();
            }

            if (coll != null)
                coll.Dispose();

            return sid;
        }

        /// <summary>
        /// Queries the AD for object with given objectClass and given objectName
        /// returns true if object can be found
        /// </summary>
        /// <param name="objectClass"></param>
        /// <param name="objectName"></param>
        /// <returns></returns>
        public bool ObjectExists(string objectClass, string objectName)
        {
            bool result = false;

            SetLDAPPath(domainDistinguishedName);

            string[] props = new string[] { "distinguishedName" };
            SearchResultCollection coll = Searcher($"(&(objectClass={objectClass})(name={objectName}))", props);

            result = coll != null && coll.Count > 0;

            if (coll != null)
                coll.Dispose();

            return result;

        }


        /// <summary>
        /// Returns value of a given attribute for the given user.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="attribute"></param>
        /// <param name="objectClass"></param>
        /// <returns></returns>
        public string GetAttribute(string name, string attribute, ObjectType objectType = ObjectType.NULL)
        {
            // Set default path to domain root
            SetLDAPPath(domainDistinguishedName);

            string result = "";

            string query = objectType == ObjectType.NULL ?
                     $"(|(name={name})(displayName={name})(dnsHostName={name})(distinguishedName={name}))" :
                     $"(&(|(name={name})(displayName={name})(dnsHostName={name})(distinguishedName={name}))(objectClass={objectType}))";

            string[] props = new string[] { attribute };
            SearchResultCollection coll = Searcher(query, props);

            // Only return the value of the attribute for the first result
            try
            {
                if (coll != null || coll.Count > 0)
                    result = coll[0].Properties[attribute][0].ToString();
            }
            catch { }

            if (coll != null)
                coll.Dispose();

            return result;
        }

        /// <summary>
        /// Creates AD Security group and returns distinguishedName
        /// Thx: http://www.codeproject.com/Articles/18102/Howto-Almost-Everything-In-Active-Directory-via-C
        /// </summary>
        /// <param name="groupName"></param>
        /// <param name="OrganizationUnit"></param>
        public string CreateGroup(string groupName, string OrganizationUnit, string Description)
        {
            //exists = ObjectExists("group", groupName);
            string dn = GetObjectDistinguishedName(groupName, ObjectType.group);

            // return DN
            if (dn != null)
                return dn;

            // Change path to given orgunit
            SetLDAPPath(OrganizationUnit);

            try
            {
                // create group entry
                DirectoryEntry group = DirectoryEntry.Children.Add("CN=" + groupName, "group");

                string sam = groupName.Replace(" ", null);

                // set properties
                group.Properties["sAmAccountName"].Value = sam;
                group.Properties["description"].Value = Description;

                // save group
                group.CommitChanges();
                group.Dispose();

                return $"CN={groupName},{OrganizationUnit}";
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
            finally
            {

                // change default path back to default
                SetLDAPPath(domainDistinguishedName);
            }
        }

        /// <summary>
        /// Adds member to group. 
        /// </summary>
        /// <param name="groupDN"></param>
        /// <param name="MemberDNList">Nullable</param>
        /// <param name="MemberDN"></param>
        public void AddGroupMember(string groupDN, string MemberDN)
        {
            SetLDAPPath(groupDN);

            try
            {
                DirectoryEntry.Properties["member"].Add(MemberDN);
                DirectoryEntry.CommitChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                SetLDAPPath(domainDistinguishedName);
            }
        }

        /// <summary>
        /// Adds multiple members to group. 
        /// </summary>
        /// <param name="groupDN"></param>
        /// <param name="MemberDNList">Nullable</param>
        /// <param name="MemberDN"></param>
        public void AddGroupMember(string groupDN, List<string> MembersDN)
        {
            SetLDAPPath(groupDN);

            try
            {
                DirectoryEntry.Properties["member"].AddRange(MembersDN.ToArray());
                DirectoryEntry.CommitChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                SetLDAPPath(domainDistinguishedName);
            }
        }

        /// <summary>
        /// Enum with objectTypes
        /// </summary>
        public enum ObjectType
        {
            group,
            user,
            domain,
            computer,
            NULL
        }

        /// <summary>
        /// Searches AD for object with given ObjectType and name and returns the distinguishedname.
        /// Result will be null if not found
        /// </summary>
        /// <param name="name">Name of the object</param>
        /// <param name="objectType">Type of the object</param>
        /// <returns></returns>
        public string GetObjectDistinguishedName(string name, ObjectType objectType)
        {
            string result = string.Empty;

            // Make sure that we search the whole domain
            SetLDAPPath(domainDistinguishedName);

            // Built query, search AD 
            string ldapQuery = $"(&(objectClass={objectType})(name={name}))";
            SearchResultCollection sResult = Searcher(ldapQuery, new string[] { "distinguishedname" });

            // return null if nothing was found
            if (sResult == null || sResult.Count <= 0)
                return null;

            // We expect unique values, use the first index of the collection
            result = GetAttributeValue(sResult[0], "distinguishedName");

            return result;
        }

        /// <summary>
        /// Creates computer objects for testing in the configured OU
        /// </summary>
        /// <param name="hosts"></param>
        public void CreateComputerObjects(List<Core.Models.Host> hosts)
        {
            SetLDAPPath(Properties.Settings.Default.ComputerOUForTest);

            try
            {
                foreach (Models.Host host in hosts)
                {
                    try
                    {
                        string samaccountname = host.DNSHostname.Split('.')[0];

                        // create computer entry
                        DirectoryEntry newComputer = DirectoryEntry.Children.Add("CN=" + samaccountname, "computer");

                        // set properties
                        newComputer.Properties["sAmAccountName"].Value = $"{samaccountname}$";
                        newComputer.Properties["description"].Value = host.IsServerOS ? "Server" : "Workstation";
                        newComputer.Properties["dnsHostname"].Value = host.DNSHostname;
                        newComputer.Properties["operatingSystem"].Value = host.OperatingSystem;
                        newComputer.Properties["userAccountControl"].Value = 0x1020;


                        // save computer object
                        newComputer.CommitChanges();
                        newComputer.Dispose();
                    }
                    catch
                    {
                        // Dont handle it, skip it
                    }

                }
            }

            finally
            {

                // change default path back to default
                SetLDAPPath(domainDistinguishedName);
            }
        }

        /// <summary>
        /// Checks if input is LDAP safe
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public bool IsValidString(string input)
        {
            bool result = true;
            List<char> chars = new List<char>()
            {
                ',',
                '+',
                '"',
                '\\',
                '<',
                '>',
                ';',
                '/',
                ':',
                '*',
                '?',
                '|',
                '~',
                '!',
                '@',
                '$',
                '%',
                '^',
                '&',
                '\'',
                '[',
                ']',
                '=',
            };

            int count = input.ToCharArray().Where(c => chars.Contains(c)).Count();
            result = count <= 0;
            return result;
        }

        #endregion

        #region static methods

        /// <summary>
        /// Small check to see if computer is joined to the domain
        /// </summary>
        /// <returns></returns>
        public static bool IsComputerJoinedToDomain()
        {
            var result = true;

            try
            {
                ManagementObject ComputerSystem;
                using (ComputerSystem = new ManagementObject(String.Format("Win32_ComputerSystem.Name='{0}'", Environment.MachineName)))
                {
                    ComputerSystem.Get();
                    UInt16 DomainRole = (UInt16)ComputerSystem["DomainRole"];
                    result = (DomainRole != 0 & DomainRole != 2);
                }
            }
            catch (Exception ex)
            {
                result = false;
            }

            return result;
        }

        /// <summary>
        /// Test if the current loggedOnuser is joined to the domain
        /// </summary>
        /// <returns></returns>
        public static bool IsUserJoinedToDomain()
        {
            var result = true;

            try
            {
                System.Security.Principal.SecurityIdentifier domainSid =
                    System.Security.Principal.WindowsIdentity.GetCurrent().User.AccountDomainSid;
                result = domainSid != null;
            }
            catch (Exception ex)
            {
                result = false;
            }

            return result;
        }


        #endregion

    }
}
