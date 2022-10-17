using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Antignis.Server.Core.Data.Querier
{
    public partial class DataQuerier : Form
    {
        #region syntaxhighlights

        private readonly List<string> sqlKeywords = new List<string>()
            {
                "select",
                "from",
                "where",
                "distinct",
                "inner",
                "left",
                "join",
                "outer",
                "and",
                "or",
                "on",
                "in",
                "not",
                "as",
                "alias"

            };
        private readonly Font fRegular = new Font("Courier New", 10, FontStyle.Regular);
        private readonly Color cRegular = Color.Black;
        private readonly Font fKeyword = new Font("Courier New", 10, FontStyle.Bold);
        private readonly Color cKeyword = Color.Blue;
        private readonly Regex rTokens = new Regex(@"([ \t{}():;\-])");
        private readonly Regex rReplace = new Regex(@"(\w+)\r?\n(\w+)");

        /// <summary>
        /// Logic for adding syntax highlighting 
        /// </summary>
        /// <param name="line"></param>
        private void SyntaxHighlight(string line)
        {
            // when a \r\n is added between two ascii chars, add a space after the \n
            // This helps in matching keywords, where otherwise the keywords wont be recognized
            if (rReplace.IsMatch(line))
                line = rReplace.Replace(line, "$1 \r\n$2");


            String[] tokens = rTokens.Split(line);
            foreach (string token in tokens)
            {
                string testValue = token.ToLower().Trim('\r', '\n', '\t', '-', '/', '#');
                string value = token;
                // Set the tokens default color and font.  
                rtbQuery.SelectionColor = cRegular;
                rtbQuery.SelectionFont = fRegular;

                // Check whether the token is a keyword.   
                if (sqlKeywords.Contains(testValue))
                {
                    rtbQuery.SelectionColor = cKeyword;
                    rtbQuery.SelectionFont = fKeyword;
                    value = token.ToUpper();
                }

                rtbQuery.SelectedText = value;
            }
        }

        #endregion

        private readonly Core.SQL.Database dbclient;
        private readonly Core.ADDS.LDAP ldapClient;

        private bool formLoaded = false;
        private bool HasEmptyrows = true;

        public DataQuerier(Core.SQL.Database dbclient, Core.ADDS.LDAP ldapClient)
        {
            InitializeComponent();
            this.dbclient = dbclient;
            this.ldapClient = ldapClient;
        }

        /// <summary>
        /// Executes query against database
        /// </summary>
        private void ExecuteQuery()
        {
            // Remove empty prepopulated rows
            if (HasEmptyrows)
            {
                HasEmptyrows = false;
                dataGridView1.Columns.Clear();
                dataGridView1.Rows.Clear();
            }

            // Check if parameters are present
            if (rtbQuery.Text.Contains("@"))
            {
                MessageBox.Show("Query contains paramters. Replace parameters with values.");
                return;
            }

            // Query database
            DataSet dSet = null;

            try
            {
                dSet = dbclient.QueryDatabaseDS(rtbQuery.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error executing query: " + ex.Message);
                return;
            }

            if (dSet == null || dSet.Tables.Count <= 0)
            {
                MessageBox.Show("Query yielded no resuls.");
                return;
            }

            // Add columns to gridview based on the columns in the dataset
            dataGridView1.AutoGenerateColumns = true;
            dataGridView1.DataSource = dSet;
            dataGridView1.DataMember = dSet.Tables[0].TableName;

            // Resize columns
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
            dataGridView1.AllowUserToOrderColumns = true;
            dataGridView1.AllowUserToResizeColumns = true;
        }

        /// <summary>
        /// Creates a new GPO
        /// </summary>
        private void CreateGPO()
        {
            DataSet datasource = (DataSet)dataGridView1.DataSource;

            // Sanity check
            if (!SanityCheckGPOAdd(datasource))
                return;

            // Determine the index of the DNSHostName column
            int colIndex = datasource.Tables[0].Columns.IndexOf("DNSHostName");

            // Add all hostnames to a list
            List<string> hostnames = new List<string>();
            foreach (DataRow row in datasource.Tables[0].Rows)
            {
                object[] items = row.ItemArray;
                hostnames.Add(items[colIndex].ToString());
            }

            // Ask userinput which ports should be blocked
            GPOCreate gPOCreate = new GPOCreate(ldapClient, hostnames);
            try
            {
                gPOCreate.ShowDialog();
                if (!gPOCreate.Success && !string.IsNullOrEmpty(gPOCreate.ErrorMessage))
                    MessageBox.Show(gPOCreate.ErrorMessage, "Error while creating GPO", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                gPOCreate.Dispose();
            }
        }

        /// <summary>
        /// Loads the data upon opening the form
        /// </summary>
        private void LoadData()
        {
            GetDefaultCMBItems();
            formLoaded = true;

            // Preload the first query
            if (comboBox1.Items.Count > 0)
            {
                Models.Query selectedItem = (Models.Query)comboBox1.Items[0];
                SyntaxHighlight(selectedItem.QueryString);
            }

            // Make the datagridview look like one
            int numberOfRows = 15;
            for (int i = 0; i < numberOfRows; i++)
            {
                dataGridView1.Rows.Add(new DataGridViewRow());
            }
        }

        /// <summary>
        /// Returns a list with default builtin query types
        /// </summary>
        /// <returns></returns>
        private void GetDefaultCMBItems()
        {
            List<Models.Query> list = dbclient.GetQueries();
            comboBox1.DataSource = list;
            comboBox1.DisplayMember = "Name";
        }

        /// <summary>
        /// Checks if data is present to create GPO
        /// </summary>
        /// <param name="datasource"></param>
        /// <returns></returns>
        private bool SanityCheckGPOAdd(DataSet datasource)
        {
            bool result = true;

            if (datasource == null)
            {
                MessageBox.Show("There's no data to create a policy for");
                return false;
            }

            // Check if there's tables in the dataset
            if (datasource.Tables.Count <= 0)
            {
                MessageBox.Show("There's no data to create a policy for");
                return false;
            }

            // Check if there's rows in the dataset
            if (datasource.Tables[0].Rows.Count <= 0)
            {
                MessageBox.Show("There's no data to create a policy for");
                return false;
            }

            // Check if the DNSHostName field is present
            if (!datasource.Tables[0].Columns.Contains("DNSHostName"))
            {
                MessageBox.Show("The query should include the 'DNSHostName' column");
                return false;
            }

            return result;
        }

        /// <summary>
        /// Saves query to the database
        /// </summary>
        private void SaveQuery()
        {
            // ask name for the query
            Core.Util.InputDialog dlg = new Util.InputDialog();
            DialogResult res = dlg.Show();

            if (res == DialogResult.OK)
            {
                string queryname = dlg.result;

                // Check if name already exists
                if (comboBox1.Items.Contains(queryname))
                {
                    MessageBox.Show("That name already exists.");
                }
                else
                {
                    dbclient.AddQuery(new Models.Query()
                    {
                        Name = queryname,
                        QueryString = rtbQuery.Text
                    });
                }
            }


            dlg = null;

            // Load items
            GetDefaultCMBItems();
        }

        /// <summary>
        /// Load selected query in textbox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            Models.Query selectedItem = (Models.Query)comboBox1.SelectedItem;

            if (formLoaded)
            {
                rtbQuery.Text = string.Empty;
                SyntaxHighlight(selectedItem.QueryString);
            }

        }

        /// <summary>
        /// Save query to database
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSaveQuery_Click(object sender, EventArgs e)
        {
            SaveQuery();
        }

        /// <summary>
        /// Creates a GPO based on dataset in gridview
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCreatePolicy_Click(object sender, EventArgs e)
        {
            CreateGPO();
        }


        /// <summary>
        /// Populates combobox and adds empty rows to the gridview
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataQuerier_Load(object sender, EventArgs e)
        {
            LoadData();
        }

        /// <summary>
        /// Eventhandler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnQuery_Click(object sender, EventArgs e)
        {
            ExecuteQuery();
        }

        /// <summary>
        /// Open URL to blogpost
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://www.huntandhackett.com/blog/introducing-antignis-a-data-driven-tool-to-configure-windows-hostbased-firewall");
        }
    }
}
