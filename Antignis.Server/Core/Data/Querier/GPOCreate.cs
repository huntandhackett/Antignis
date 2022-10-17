using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Antignis.Server.Core.Data.Querier
{
    public partial class GPOCreate : Form
    {
        private readonly Dictionary<string, string> map = new Dictionary<string, string>();

        /// <summary>
        /// Boolean to know if GPO creation was successful
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Error message if error occured
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// ldap client to talk to AD
        /// </summary>
        private ADDS.LDAP ldapClient { get; set; }

        /// <summary>
        /// list of strings with all the hosts in scope
        /// </summary>
        private List<string> hosts { get; set; }

        public GPOCreate(ADDS.LDAP ldapclient, List<string> hosts)
        {
            InitializeComponent();
            ldapClient = ldapclient;
            this.hosts = hosts;

            map.Add("ssh", "22");
            map.Add("telnet", "23");
            map.Add("wmi", "135");
            map.Add("smb", "445");
            map.Add("rdp", "3389");
            map.Add("vnc", "5900");
            map.Add("teamviewer", "5938");
            map.Add("winrm", "5985, 5986");
            map.Add("wsman", "5985, 5986");
        }

        /// <summary>
        /// Load the configured prefix
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GPOCreate_Load(object sender, EventArgs e)
        {
            // Load prefixes
            txtPolicyName.Text = Properties.Settings.Default.GPONamePrefix;
        }

        /// <summary>
        /// Displays help on what kind of ports can be entered
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lnklblHelp_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MessageBox.Show("Enter ports. " +
                "Use commas (,) to enter multiple ports.\r\n" +
                "Example:\r\n\tSingle port: 445\r\n\t" +
                "Multiple ports: 135, 445, 3389\r\n\r\n" +
                "Common protocols and their ports:" +
                "\r\n\tFTP: 21" +
                "\r\n\tSSH: 22" +
                "\r\n\tTelnet: 23" +
                "\r\n\tWMI: 135" +
                "\r\n\tSMB: 445" +
                "\r\n\tRDP: 3389" +
                "\r\n\tWinRM: 5985, 5986" +
                "\r\n\tVNC: 5900" +
                "\r\n\tTeamviewer: 5938" +
                "\r\n\r\nMore can be found on the following link: https://en.wikipedia.org/wiki/List_of_TCP_and_UDP_port_numbers"
                , "Help", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Logic for creating the policy
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSubmit_Click(object sender, EventArgs e)
        {
            List<int> Ports = new List<int>();

            // sanity checks
            string name = txtPolicyName.Text;
            string description = txtPolicyDescription.Text;
            string ports = txtPorts.Text.Replace(" ", null);

            // Show warning if policy name is empty or the same as the configured prefix
            if (string.IsNullOrEmpty(name) || name == Properties.Settings.Default.GPONamePrefix)
            {
                MessageBox.Show("Please enter a new policy name");
                return;
            }

            // Show warning if description is empty
            if (string.IsNullOrEmpty(description))
            {
                MessageBox.Show("Please enter a policy description");
                return;
            }

            // Show warning if ports is empty
            if (string.IsNullOrEmpty(ports))
            {
                MessageBox.Show("Please enter the ports to be blocked");
                return;
            }

            // Check if we can parse the ports to an int array
            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(@"^(?:\d{1,5}(?:,\d{1,5})?)+$");
            if (!regex.IsMatch(ports))
            {
                MessageBox.Show("Given input for ports is invalid.\r\n.Use digits only or use commas(,) to enter multiple ports.\r\nExample:\r\n\tSingle port: 445\r\n\tMultiple ports: 135, 445, 3389");
                return;
            }

            // Check if the name is LDAP safe
            if (!ldapClient.IsValidString(name))
            {
                MessageBox.Show("Given input for name contains illegal characters. Please use AZ-az 0-9 characters");
                return;
            }

            // Check if group already exists
            if (ldapClient.ObjectExists("group", name))
            {
                MessageBox.Show("The name already exists. Please use another name");
                return;
            }

            try
            {
                btnSubmit.Text = "Creating GPO...";
                btnSubmit.Enabled = false;


                string[] portSplits = ports.Split(',');
                foreach (string p in portSplits)
                    Ports.Add(Convert.ToInt32(p));

                // Create new AD groups for the GPO
                Models.GPOGroups groups = ADDS.GPO.CreateGroups(name, description, hosts, ldapClient);

                // Create the GPO
                ADDS.GPO.Create(name, description, Ports,
                    groups.ScopeGroupName,
                    groups.UserBypassGroupName,
                    groups.ComputerBypassGroupName,
                    ldapClient,
                    "");

                MessageBox.Show("GPO created!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Success = true;
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                Success = false;
            }


            Close();
        }


        /// <summary>
        /// Prefill portnumbers based on what the users writes in the policy name
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtPolicyName_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtPolicyName.Text))
                return;

            // Split on space
            string[] splits = txtPolicyName.Text.Split(' ');
            if (splits.Count() <= 0)
                return;

            foreach (string spl in splits)
            {
                string key = spl.ToLower();
                if (map.ContainsKey(key))
                {
                    string port = map[key];
                    if (txtPorts.Text.Contains(port))
                        continue;

                    // seperate by comma
                    if (!string.IsNullOrEmpty(txtPorts.Text))
                        txtPorts.Text += ", ";

                    // Add port
                    txtPorts.Text += port;
                }
            }
        }
    }
}
