using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ProcessWatchDog
{
    public partial class SettingForm : Form
    {
        RegistryKey rkApp = Registry.CurrentUser.CreateSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run");

        public SettingForm()
        {
            InitializeComponent();
            DialogResult = DialogResult.Cancel;
            textBox1.Text = Properties.Settings.Default.checkTime.ToString();
            checkBox1.Checked = Properties.Settings.Default.startupOpen;
            checkBox2.Checked = Properties.Settings.Default.minify;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int checkTime;
            if (!int.TryParse(textBox1.Text, out checkTime))
            {
                toolTip1.Show("請輸入正確的數字", textBox1, 2000);
                return;
            }
            else
            {
                if (checkTime < 0)
                {
                    toolTip1.Show("請輸入正整數", textBox1, 2000);
                    return;
                }
            }
            Properties.Settings.Default.checkTime = checkTime;
            Properties.Settings.Default.startupOpen = checkBox1.Checked;
            Properties.Settings.Default.minify = checkBox2.Checked;
            if (checkBox1.Checked)
            {
                rkApp.SetValue("AlanProcessWatchDog", "\"" + Application.ExecutablePath.ToString() + "\"");
            }
            else
            {
                rkApp.DeleteValue("AlanProcessWatchDog", false);
            }
            Properties.Settings.Default.Save();
            DialogResult = DialogResult.OK;
        }
    }
}
