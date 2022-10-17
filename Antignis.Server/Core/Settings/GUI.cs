using System;
using System.ComponentModel;
using System.Configuration;
using System.Windows.Forms;

namespace Antignis.Server.Core.Settings
{
    public partial class GUI : Form
    {
        //Thx: https://stackoverflow.com/questions/48040186/edit-contents-of-setting-file-settings-settings-in-datagridview

        public GUI()
        {
            InitializeComponent();
        }

        private void GUI_Load(object sender, EventArgs e)
        {

            propertyGrid1.SelectedObject = Properties.Settings.Default;
            propertyGrid1.BrowsableAttributes = new AttributeCollection(new UserScopedSettingAttribute());
        }

        private void propertyGrid1_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            Properties.Settings.Default.Save();
        }
    }
}
