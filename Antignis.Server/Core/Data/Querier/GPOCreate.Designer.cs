namespace Antignis.Server.Core.Data.Querier
{
    partial class GPOCreate
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GPOCreate));
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.txtPorts = new System.Windows.Forms.TextBox();
            this.txtPolicyName = new System.Windows.Forms.TextBox();
            this.txtPolicyDescription = new System.Windows.Forms.TextBox();
            this.btnSubmit = new System.Windows.Forms.Button();
            this.lnklblHelp = new System.Windows.Forms.LinkLabel();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(40, 46);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(101, 20);
            this.label1.TabIndex = 0;
            this.label1.Text = "Policy name: ";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(40, 168);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(138, 20);
            this.label2.TabIndex = 0;
            this.label2.Text = "Policy description: ";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(40, 104);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(153, 20);
            this.label3.TabIndex = 0;
            this.label3.Text = "Ports to be blocked: ";
            // 
            // txtPorts
            // 
            this.txtPorts.Location = new System.Drawing.Point(247, 101);
            this.txtPorts.Name = "txtPorts";
            this.txtPorts.Size = new System.Drawing.Size(398, 26);
            this.txtPorts.TabIndex = 1;
            // 
            // txtPolicyName
            // 
            this.txtPolicyName.Location = new System.Drawing.Point(247, 43);
            this.txtPolicyName.Name = "txtPolicyName";
            this.txtPolicyName.Size = new System.Drawing.Size(398, 26);
            this.txtPolicyName.TabIndex = 0;
            this.txtPolicyName.TextChanged += new System.EventHandler(this.txtPolicyName_TextChanged);
            // 
            // txtPolicyDescription
            // 
            this.txtPolicyDescription.Location = new System.Drawing.Point(247, 165);
            this.txtPolicyDescription.Multiline = true;
            this.txtPolicyDescription.Name = "txtPolicyDescription";
            this.txtPolicyDescription.Size = new System.Drawing.Size(398, 97);
            this.txtPolicyDescription.TabIndex = 2;
            // 
            // btnSubmit
            // 
            this.btnSubmit.Location = new System.Drawing.Point(533, 281);
            this.btnSubmit.Name = "btnSubmit";
            this.btnSubmit.Size = new System.Drawing.Size(112, 60);
            this.btnSubmit.TabIndex = 3;
            this.btnSubmit.Text = "Create GPO";
            this.btnSubmit.UseVisualStyleBackColor = true;
            this.btnSubmit.Click += new System.EventHandler(this.btnSubmit_Click);
            // 
            // lnklblHelp
            // 
            this.lnklblHelp.AutoSize = true;
            this.lnklblHelp.Location = new System.Drawing.Point(189, 101);
            this.lnklblHelp.Name = "lnklblHelp";
            this.lnklblHelp.Size = new System.Drawing.Size(26, 20);
            this.lnklblHelp.TabIndex = 3;
            this.lnklblHelp.TabStop = true;
            this.lnklblHelp.Text = "[?]";
            this.lnklblHelp.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnklblHelp_LinkClicked);
            // 
            // GPOCreate
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(694, 353);
            this.Controls.Add(this.lnklblHelp);
            this.Controls.Add(this.btnSubmit);
            this.Controls.Add(this.txtPolicyName);
            this.Controls.Add(this.txtPolicyDescription);
            this.Controls.Add(this.txtPorts);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "GPOCreate";
            this.Text = "New Group Policy";
            this.Load += new System.EventHandler(this.GPOCreate_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtPorts;
        private System.Windows.Forms.TextBox txtPolicyName;
        private System.Windows.Forms.TextBox txtPolicyDescription;
        private System.Windows.Forms.Button btnSubmit;
        private System.Windows.Forms.LinkLabel lnklblHelp;
    }
}