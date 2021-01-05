using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ProcessWatchDog
{
    public partial class Form1 : Form
    {
        public static List<WatchItem> itemlist;
        public static List<LogItem> loglist;
        private bool isStartup = false;
        NotifyIcon notifyIcon1 = new NotifyIcon();
        System.Threading.Timer CheckTimer;

        public Form1(string[] args)
        {
            InitializeComponent();
            initData();
            reloadList();
            notifyIcon1.Text = "程式看門狗";
            notifyIcon1.Icon = new Icon(Application.StartupPath + "\\icon.ico");
            notifyIcon1.MouseDoubleClick += new MouseEventHandler(notifyIcon1_MouseDoubleClick);
            CheckTimer = new System.Threading.Timer(_ => CheckTimer_Tick(), null, Properties.Settings.Default.checkTime * 1000, Properties.Settings.Default.checkTime * 1000);
            if (args.Length > 0 && Properties.Settings.Default.minify)
            {
                if (args[0] == "startup")
                {
                    isStartup = true;
                }
            }
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Show();
            WindowState = FormWindowState.Normal;
            notifyIcon1.Visible = false;
        }

        private void initData()
        {
            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "data.dat"))
            {
                using (Stream input = File.OpenRead(AppDomain.CurrentDomain.BaseDirectory + "data.dat"))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    itemlist = (List<WatchItem>)formatter.Deserialize(input);
                    foreach (WatchItem item in itemlist)
                    {
                        item.init();
                    }
                }
            }
            else
            {
                itemlist = new List<WatchItem>();
            }
            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "log.dat"))
            {
                using (Stream input = File.OpenRead(AppDomain.CurrentDomain.BaseDirectory + "log.dat"))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    loglist = (List<LogItem>)formatter.Deserialize(input);
                }
            }
            else
            {
                loglist = new List<LogItem>();
            }
        }

        private void reloadList()
        {
            listView1.Items.Clear();
            listView1.BeginUpdate();
            foreach (WatchItem item in itemlist)
            {
                string[] row = { item.watchProcessName, item.openFileName, item.openDelayTime.ToString(), item.noResponseCheckTime.ToString(), (item.status ? "啟動" : "停止") };
                ListViewItem lvi = new ListViewItem(row);
                listView1.Items.Add(lvi);
            }
            listView1.EndUpdate();
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            AddForm addForm = new AddForm();
            DialogResult result = addForm.ShowDialog();
            if (result == DialogResult.OK)
            {
                WatchItem item = new WatchItem()
                {
                    watchProcessName = addForm.textBox1.Text,
                    openFileName = addForm.textBox2.Text,
                    openDelayTime = int.Parse(addForm.textBox3.Text),
                    noResponseCheckTime = int.Parse(addForm.textBox4.Text),
                    status = false
                };
                itemlist.Add(item);
                reloadList();
            }
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView1.SelectedItems)
            {
                itemlist[item.Index].status = true;
                itemlist[item.Index].init();
            }
            reloadList();
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView1.SelectedItems)
            {
                itemlist[item.Index].status = false;
            }
            reloadList();
        }

        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show("確定要停止所有監控?", "", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                foreach (WatchItem item in itemlist)
                {
                    item.status = false;
                }
                reloadList();
            }
        }

        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 1)
            {
                AddForm addForm = new AddForm();
                addForm.textBox1.Text = itemlist[listView1.SelectedItems[0].Index].watchProcessName;
                addForm.textBox2.Text = itemlist[listView1.SelectedItems[0].Index].openFileName;
                addForm.textBox3.Text = itemlist[listView1.SelectedItems[0].Index].openDelayTime.ToString();
                addForm.textBox4.Text = itemlist[listView1.SelectedItems[0].Index].noResponseCheckTime.ToString();
                DialogResult result = addForm.ShowDialog();
                if (result == DialogResult.OK)
                {
                    itemlist[listView1.SelectedItems[0].Index].watchProcessName = addForm.textBox1.Text;
                    itemlist[listView1.SelectedItems[0].Index].openFileName = addForm.textBox2.Text;
                    itemlist[listView1.SelectedItems[0].Index].openDelayTime = int.Parse(addForm.textBox3.Text);
                    itemlist[listView1.SelectedItems[0].Index].noResponseCheckTime = int.Parse(addForm.textBox4.Text);
                }
                reloadList();
            }
            else if (listView1.SelectedItems.Count > 1)
            {
                AddForm addForm = new AddForm();
                addForm.multi = true;
                bool delayTimeEqual = true;
                bool noResponseTimeEqual = true;
                foreach (ListViewItem item in listView1.SelectedItems)
                {
                    if (itemlist[item.Index].openDelayTime != itemlist[listView1.SelectedItems[0].Index].openDelayTime)
                    {
                        delayTimeEqual = false;
                    }
                    if (itemlist[item.Index].noResponseCheckTime != itemlist[listView1.SelectedItems[0].Index].noResponseCheckTime)
                    {
                        noResponseTimeEqual = false;
                    }
                }
                addForm.button1.Enabled = false;
                addForm.button2.Enabled = false;
                addForm.textBox1.Enabled = false;
                addForm.textBox2.Enabled = false;
                if (delayTimeEqual)
                {
                    addForm.textBox3.Text = itemlist[listView1.SelectedItems[0].Index].openDelayTime.ToString();
                }
                if (noResponseTimeEqual)
                {
                    addForm.textBox4.Text = itemlist[listView1.SelectedItems[0].Index].noResponseCheckTime.ToString();
                }
                DialogResult result = addForm.ShowDialog();
                if (result == DialogResult.OK)
                {
                    foreach (ListViewItem item in listView1.SelectedItems)
                    {
                        itemlist[item.Index].openDelayTime = int.Parse(addForm.textBox3.Text);
                        itemlist[item.Index].noResponseCheckTime = int.Parse(addForm.textBox4.Text);
                    }
                }
                reloadList();
            }
        }

        private void toolStripButton7_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                DialogResult dialogResult = MessageBox.Show("確定要刪除" + listView1.SelectedItems.Count.ToString() + "筆資料?", "", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes)
                {
                    List<WatchItem> removelist = new List<WatchItem>();
                    foreach (ListViewItem item in listView1.SelectedItems)
                    {
                        removelist.Add(itemlist[item.Index]);
                    }
                    foreach (WatchItem item in removelist)
                    {
                        itemlist.Remove(item);
                    }
                    reloadList();
                }
            }
            else
            {
                MessageBox.Show("請選擇要刪除的項目");
            }
        }

        private void toolStripButton8_Click(object sender, EventArgs e)
        {
            LogForm logForm = new LogForm();
            logForm.listView1.Items.Clear();
            logForm.listView1.BeginUpdate();
            foreach (LogItem item in loglist)
            {
                string[] row = { item.time.ToString("yyyy-MM-dd HH:mm:ss"), item.processname, item.message };
                ListViewItem lvi = new ListViewItem(row);
                logForm.listView1.Items.Add(lvi);
            }
            listView1.EndUpdate();
            logForm.Show();
        }

        private void toolStripButton6_Click(object sender, EventArgs e)
        {
            SettingForm settingForm = new SettingForm();
            DialogResult result = settingForm.ShowDialog();
            if (result == DialogResult.OK)
            {
                CheckTimer.Change(Properties.Settings.Default.checkTime * 1000, Properties.Settings.Default.checkTime * 1000);
            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            using (Stream output = File.Create(AppDomain.CurrentDomain.BaseDirectory + "data.dat"))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(output, itemlist);
                output.Close();
            }

            using (Stream output = File.Create(AppDomain.CurrentDomain.BaseDirectory + "log.dat"))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(output, loglist);
                output.Close();
            }
        }

        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.minify)
            {
                if (WindowState == FormWindowState.Minimized)
                {
                    Hide();
                    notifyIcon1.Visible = true;
                }
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show("是否要關閉程式看門狗", "", MessageBoxButtons.YesNo);
            if (dialogResult != DialogResult.Yes)
            {
                e.Cancel = true;
                return;
            }
        }

        private void CheckTimer_Tick()
        {
            foreach (WatchItem item in itemlist)
            {
                if (item.status && !item.checkWatchIsOnProcess())
                {
                    item.inOnProcess = true;
                    Process[] processlist = Process.GetProcessesByName(item.watchProcessName);
                    if (processlist.Length <= 0)
                    {
                        LogItem logitem = new LogItem()
                        {
                            time = DateTime.Now,
                            processname = item.watchProcessName,
                            message = "檢測到處理程序關閉"
                        };
                        loglist.Insert(0, logitem);
                        item.delayStart();
                        item.inOnProcess = false;
                    }
                    else
                    {
                        if (item.noResponseCheckTime > 0)
                        {
                            bool isResponding = processlist[0].MainWindowHandle == IntPtr.Zero ? IsResponding(processlist[0]) : processlist[0].Responding;
                            if (!isResponding)
                            {
                                item.isOnCheckNoResponse = true;
                                LogItem logitem = new LogItem()
                                {
                                    time = DateTime.Now,
                                    processname = item.watchProcessName,
                                    message = "檢測到處理程序無回應"
                                };
                                loglist.Insert(0, logitem);
                                new Thread(() =>
                                    {
                                        Thread.Sleep(item.noResponseCheckTime * 1000);
                                        bool newIsResponding = processlist[0].MainWindowHandle == IntPtr.Zero ? IsResponding(processlist[0]) : processlist[0].Responding;
                                        if (!newIsResponding)
                                        {
                                            LogItem logitem2 = new LogItem()
                                            {
                                                time = DateTime.Now,
                                                processname = item.watchProcessName,
                                                message = "再次檢測處理程序依然無回應，強制關閉處理程序"
                                            };
                                            loglist.Insert(0, logitem2);
                                            new Thread(() =>
                                                {
                                                    try
                                                    {
                                                        processlist[0].Kill();
                                                        LogItem logitem3 = new LogItem()
                                                        {
                                                            time = DateTime.Now,
                                                            processname = item.watchProcessName,
                                                            message = "成功關閉處理程序"
                                                        };
                                                        loglist.Insert(0, logitem3);
                                                    }
                                                    catch (Exception)
                                                    {
                                                        LogItem logitem3 = new LogItem()
                                                        {
                                                            time = DateTime.Now,
                                                            processname = item.watchProcessName,
                                                            message = "無法關閉處理程序"
                                                        };
                                                        loglist.Insert(0, logitem3);
                                                    }
                                                    item.isOnCheckNoResponse = false;
                                                    item.inOnProcess = false;
                                                }
                                            ).Start();
                                        }
                                        else
                                        {
                                            LogItem logitem2 = new LogItem()
                                            {
                                                time = DateTime.Now,
                                                processname = item.watchProcessName,
                                                message = "再次檢測處理程序處理程序重新取得回應"
                                            };
                                            loglist.Insert(0, logitem2);
                                            item.isOnCheckNoResponse = false;
                                            item.inOnProcess = false;
                                        }
                                    }
                                ).Start();
                            }
                            else
                            {
                                item.inOnProcess = false;
                            }
                        }
                        else
                        {
                            item.inOnProcess = false;
                        }
                    }
                }
            }
            try
            {
                using (Stream output = File.Create(AppDomain.CurrentDomain.BaseDirectory + "data.dat"))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    formatter.Serialize(output, itemlist);
                    output.Close();
                }
            }
            catch (Exception){}

            try
            {
                using (Stream output = File.Create(AppDomain.CurrentDomain.BaseDirectory + "log.dat"))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    formatter.Serialize(output, loglist);
                    output.Close();
                }
            }
            catch (Exception){}
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern IntPtr SendMessageTimeout(
            HandleRef hWnd,
            int msg,
            IntPtr wParam,
            IntPtr lParam,
            int flags,
            int timeout,
            out IntPtr pdwResult);

        const int SMTO_ABORTIFHUNG = 2;

        bool IsResponding(Process process)
        {
            HandleRef handleRef = new HandleRef(process, process.MainWindowHandle);

            int timeout = 2000;
            IntPtr lpdwResult;

            IntPtr lResult = SendMessageTimeout(
                handleRef,
                0,
                IntPtr.Zero,
                IntPtr.Zero,
                SMTO_ABORTIFHUNG,
                timeout,
                out lpdwResult);

            return lResult != IntPtr.Zero;
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            if (isStartup)
            {
                WindowState = FormWindowState.Minimized;
                Hide();
                notifyIcon1.Visible = true;
            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/jery0987/");
        }
    }
}
