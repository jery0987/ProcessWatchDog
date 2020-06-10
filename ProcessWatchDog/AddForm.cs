using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ProcessWatchDog
{
    public partial class AddForm : Form
    {
        public bool multi = false;

        public AddForm()
        {
            InitializeComponent();
            DialogResult = DialogResult.Cancel;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            reloadProcess();
        }

        private void reloadProcess()
        {
            listBox1.Items.Clear();
            Process[] processlist = Process.GetProcesses();
            Array.Sort(processlist, delegate (Process p1, Process p2)
            {
                return p1.ProcessName.CompareTo(p2.ProcessName);
            });
            foreach (Process process in processlist)
            {
                listBox1.Items.Add(process.ProcessName);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Title = "選擇一個程式";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                textBox2.Text = dialog.FileName;

            }
        }

        private void listBox1_DoubleClick(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem != null)
            {
                textBox1.Text = listBox1.SelectedItem.ToString();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            bool hasError = false;
            int delayTime;
            int noResponseTime;
            if (textBox1.Text == "" && !multi)
            {
                toolTip1.Show("請輸入監看處理程序名稱或雙擊左方列表加入", textBox1, 2000);
                hasError = true;
            }
            if (textBox2.Text == "" && !multi)
            {
                toolTip2.Show("請選擇欲開啟的程式", textBox2, 2000);
                hasError = true;
            }
            if (!int.TryParse(textBox3.Text, out delayTime))
            {
                toolTip3.Show("請輸入正確的數字", textBox3, 2000);
                hasError = true;
            }
            else
            {
                if (delayTime < 0)
                {
                    toolTip3.Show("請輸入正整數", textBox3, 2000);
                    hasError = true;
                }
            }
            if (!int.TryParse(textBox4.Text, out noResponseTime))
            {
                toolTip4.Show("請輸入正確的數字", textBox4, 2000);
                hasError = true;
            }
            else
            {
                if (noResponseTime < 0)
                {
                    toolTip4.Show("請輸入正整數", textBox4, 2000);
                    hasError = true;
                }
            }
            if (!hasError)
            {
                DialogResult = DialogResult.OK;
            }
        }
    }
}
