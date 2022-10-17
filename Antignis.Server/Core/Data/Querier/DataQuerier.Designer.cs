namespace Antignis.Server.Core.Data.Querier
{
    partial class DataQuerier
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DataQuerier));
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.btnCreatePolicy = new System.Windows.Forms.Button();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.DNSHostName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.IPAddress = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.OperatingSystem = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.InteractGridPanel = new System.Windows.Forms.TableLayoutPanel();
            this.interactQueryPanel = new System.Windows.Forms.TableLayoutPanel();
            this.btnQuery = new System.Windows.Forms.Button();
            this.btnSaveQuery = new System.Windows.Forms.Button();
            this.querySelectpanel = new System.Windows.Forms.TableLayoutPanel();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.rootPanel = new System.Windows.Forms.TableLayoutPanel();
            this.dataPanel = new System.Windows.Forms.TableLayoutPanel();
            this.rtbQuery = new System.Windows.Forms.RichTextBox();
            this.menupanel = new System.Windows.Forms.TableLayoutPanel();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.InteractGridPanel.SuspendLayout();
            this.interactQueryPanel.SuspendLayout();
            this.querySelectpanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.rootPanel.SuspendLayout();
            this.dataPanel.SuspendLayout();
            this.menupanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.label1.Location = new System.Drawing.Point(3, 64);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(216, 20);
            this.label1.TabIndex = 3;
            this.label1.Text = "SQL query:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.label2.Location = new System.Drawing.Point(3, 26);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(216, 20);
            this.label2.TabIndex = 4;
            this.label2.Text = "Select predefined query: ";
            // 
            // btnCreatePolicy
            // 
            this.btnCreatePolicy.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnCreatePolicy.Location = new System.Drawing.Point(3, 3);
            this.btnCreatePolicy.Name = "btnCreatePolicy";
            this.btnCreatePolicy.Size = new System.Drawing.Size(197, 69);
            this.btnCreatePolicy.TabIndex = 6;
            this.btnCreatePolicy.Text = "Create Policy";
            this.btnCreatePolicy.UseVisualStyleBackColor = true;
            this.btnCreatePolicy.Click += new System.EventHandler(this.btnCreatePolicy_Click);
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToDeleteRows = false;
            this.dataGridView1.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.DNSHostName,
            this.IPAddress,
            this.OperatingSystem});
            this.dataGridView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridView1.Location = new System.Drawing.Point(3, 418);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.ReadOnly = true;
            this.dataGridView1.RowHeadersWidth = 62;
            this.dataGridView1.RowTemplate.Height = 28;
            this.dataGridView1.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.dataGridView1.ShowEditingIcon = false;
            this.dataGridView1.Size = new System.Drawing.Size(1157, 483);
            this.dataGridView1.TabIndex = 0;
            // 
            // DNSHostName
            // 
            this.DNSHostName.HeaderText = "DNSHostName";
            this.DNSHostName.MinimumWidth = 8;
            this.DNSHostName.Name = "DNSHostName";
            this.DNSHostName.ReadOnly = true;
            // 
            // IPAddress
            // 
            this.IPAddress.HeaderText = "IPAddress";
            this.IPAddress.MinimumWidth = 8;
            this.IPAddress.Name = "IPAddress";
            this.IPAddress.ReadOnly = true;
            // 
            // OperatingSystem
            // 
            this.OperatingSystem.HeaderText = "OperatingSystem";
            this.OperatingSystem.MinimumWidth = 8;
            this.OperatingSystem.Name = "OperatingSystem";
            this.OperatingSystem.ReadOnly = true;
            // 
            // InteractGridPanel
            // 
            this.InteractGridPanel.ColumnCount = 1;
            this.InteractGridPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.InteractGridPanel.Controls.Add(this.btnCreatePolicy, 0, 0);
            this.InteractGridPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.InteractGridPanel.Location = new System.Drawing.Point(3, 416);
            this.InteractGridPanel.Name = "InteractGridPanel";
            this.InteractGridPanel.RowCount = 2;
            this.InteractGridPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 75F));
            this.InteractGridPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.InteractGridPanel.Size = new System.Drawing.Size(203, 485);
            this.InteractGridPanel.TabIndex = 2;
            // 
            // interactQueryPanel
            // 
            this.interactQueryPanel.ColumnCount = 1;
            this.interactQueryPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.interactQueryPanel.Controls.Add(this.btnQuery, 0, 1);
            this.interactQueryPanel.Controls.Add(this.btnSaveQuery, 0, 1);
            this.interactQueryPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.interactQueryPanel.Location = new System.Drawing.Point(3, 256);
            this.interactQueryPanel.Name = "interactQueryPanel";
            this.interactQueryPanel.RowCount = 3;
            this.interactQueryPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.interactQueryPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 75F));
            this.interactQueryPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 75F));
            this.interactQueryPanel.Size = new System.Drawing.Size(203, 154);
            this.interactQueryPanel.TabIndex = 3;
            // 
            // btnQuery
            // 
            this.btnQuery.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnQuery.Location = new System.Drawing.Point(3, 82);
            this.btnQuery.Name = "btnQuery";
            this.btnQuery.Size = new System.Drawing.Size(197, 69);
            this.btnQuery.TabIndex = 4;
            this.btnQuery.Text = "Run Query";
            this.btnQuery.UseVisualStyleBackColor = true;
            this.btnQuery.Click += new System.EventHandler(this.btnQuery_Click);
            // 
            // btnSaveQuery
            // 
            this.btnSaveQuery.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnSaveQuery.Location = new System.Drawing.Point(3, 7);
            this.btnSaveQuery.Name = "btnSaveQuery";
            this.btnSaveQuery.Size = new System.Drawing.Size(197, 69);
            this.btnSaveQuery.TabIndex = 3;
            this.btnSaveQuery.Text = "Save Query";
            this.btnSaveQuery.UseVisualStyleBackColor = true;
            this.btnSaveQuery.Click += new System.EventHandler(this.btnSaveQuery_Click);
            // 
            // querySelectpanel
            // 
            this.querySelectpanel.ColumnCount = 3;
            this.querySelectpanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 222F));
            this.querySelectpanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 508F));
            this.querySelectpanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.querySelectpanel.Controls.Add(this.label1, 0, 1);
            this.querySelectpanel.Controls.Add(this.comboBox1, 1, 0);
            this.querySelectpanel.Controls.Add(this.label2, 0, 0);
            this.querySelectpanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.querySelectpanel.Location = new System.Drawing.Point(3, 3);
            this.querySelectpanel.Name = "querySelectpanel";
            this.querySelectpanel.RowCount = 2;
            this.querySelectpanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 55.29412F));
            this.querySelectpanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 44.70588F));
            this.querySelectpanel.Size = new System.Drawing.Size(1157, 84);
            this.querySelectpanel.TabIndex = 4;
            // 
            // comboBox1
            // 
            this.comboBox1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(225, 15);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(502, 28);
            this.comboBox1.TabIndex = 5;
            this.comboBox1.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::Antignis.Server.Properties.Resources.Antignis;
            this.pictureBox1.Location = new System.Drawing.Point(3, 3);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(203, 154);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 8;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.Click += new System.EventHandler(this.pictureBox1_Click);
            // 
            // rootPanel
            // 
            this.rootPanel.ColumnCount = 2;
            this.rootPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.rootPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 215F));
            this.rootPanel.Controls.Add(this.dataPanel, 0, 0);
            this.rootPanel.Controls.Add(this.menupanel, 1, 0);
            this.rootPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rootPanel.Location = new System.Drawing.Point(0, 0);
            this.rootPanel.Name = "rootPanel";
            this.rootPanel.RowCount = 1;
            this.rootPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.rootPanel.Size = new System.Drawing.Size(1384, 910);
            this.rootPanel.TabIndex = 9;
            // 
            // dataPanel
            // 
            this.dataPanel.ColumnCount = 1;
            this.dataPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.dataPanel.Controls.Add(this.querySelectpanel, 0, 0);
            this.dataPanel.Controls.Add(this.dataGridView1, 0, 2);
            this.dataPanel.Controls.Add(this.rtbQuery, 0, 1);
            this.dataPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataPanel.Location = new System.Drawing.Point(3, 3);
            this.dataPanel.Name = "dataPanel";
            this.dataPanel.RowCount = 3;
            this.dataPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 90F));
            this.dataPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 40F));
            this.dataPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 60F));
            this.dataPanel.Size = new System.Drawing.Size(1163, 904);
            this.dataPanel.TabIndex = 0;
            // 
            // rtbQuery
            // 
            this.rtbQuery.AcceptsTab = true;
            this.rtbQuery.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rtbQuery.Font = new System.Drawing.Font("Courier New", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rtbQuery.Location = new System.Drawing.Point(3, 93);
            this.rtbQuery.Name = "rtbQuery";
            this.rtbQuery.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.ForcedBoth;
            this.rtbQuery.Size = new System.Drawing.Size(1157, 319);
            this.rtbQuery.TabIndex = 5;
            this.rtbQuery.Text = "";
            this.rtbQuery.WordWrap = false;
            // 
            // menupanel
            // 
            this.menupanel.ColumnCount = 1;
            this.menupanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.menupanel.Controls.Add(this.pictureBox1, 0, 0);
            this.menupanel.Controls.Add(this.InteractGridPanel, 0, 2);
            this.menupanel.Controls.Add(this.interactQueryPanel, 0, 1);
            this.menupanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.menupanel.Location = new System.Drawing.Point(1172, 3);
            this.menupanel.Name = "menupanel";
            this.menupanel.RowCount = 3;
            this.menupanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 181F));
            this.menupanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 32.15297F));
            this.menupanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 67.84702F));
            this.menupanel.Size = new System.Drawing.Size(209, 904);
            this.menupanel.TabIndex = 1;
            // 
            // DataQuerier
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1384, 910);
            this.Controls.Add(this.rootPanel);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(1160, 966);
            this.Name = "DataQuerier";
            this.Text = "Antignis";
            this.Load += new System.EventHandler(this.DataQuerier_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.InteractGridPanel.ResumeLayout(false);
            this.interactQueryPanel.ResumeLayout(false);
            this.querySelectpanel.ResumeLayout(false);
            this.querySelectpanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.rootPanel.ResumeLayout(false);
            this.dataPanel.ResumeLayout(false);
            this.menupanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnCreatePolicy;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.TableLayoutPanel InteractGridPanel;
        private System.Windows.Forms.TableLayoutPanel interactQueryPanel;
        private System.Windows.Forms.TableLayoutPanel querySelectpanel;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.Button btnQuery;
        private System.Windows.Forms.Button btnSaveQuery;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.TableLayoutPanel rootPanel;
        private System.Windows.Forms.TableLayoutPanel dataPanel;
        private System.Windows.Forms.TableLayoutPanel menupanel;
        private System.Windows.Forms.DataGridViewTextBoxColumn DNSHostName;
        private System.Windows.Forms.DataGridViewTextBoxColumn IPAddress;
        private System.Windows.Forms.DataGridViewTextBoxColumn OperatingSystem;
        private System.Windows.Forms.RichTextBox rtbQuery;
    }
}