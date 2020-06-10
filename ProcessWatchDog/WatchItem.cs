using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace ProcessWatchDog
{
    [Serializable]
    public class WatchItem
    {
        public string watchProcessName;
        public string openFileName;
        public int openDelayTime;
        public int noResponseCheckTime;
        public bool status;

        private bool isOnDelayStart = false;
        public bool isOnCheckNoResponse = false;
        public bool inOnProcess = false;

        public bool checkWatchIsOnProcess()
        {
            return isOnDelayStart || isOnCheckNoResponse || inOnProcess;
        }

        public void init()
        {
            isOnDelayStart = false;
            isOnCheckNoResponse = false;
            inOnProcess = false;
        }

        public void delayStart()
        {
            isOnDelayStart = true;
            new Thread(() =>
                {
                    if (openDelayTime > 0)
                    {
                        Thread.Sleep(openDelayTime * 1000);
                    }
                    if (Process.GetProcessesByName(watchProcessName).Length <= 0)
                    {
                        ProcessStartInfo info = new ProcessStartInfo();
                        info.WorkingDirectory = Path.GetDirectoryName(openFileName);
                        info.FileName = Path.GetFileName(openFileName);
                        try
                        {
                            Process.Start(info);
                            LogItem logitem = new LogItem()
                            {
                                time = DateTime.Now,
                                processname = watchProcessName,
                                message = "程式啟動成功，路徑:" + openFileName
                            };
                            Form1.loglist.Insert(0, logitem);
                        }
                        catch (Exception)
                        {
                            LogItem logitem = new LogItem()
                            {
                                time = DateTime.Now,
                                processname = watchProcessName,
                                message = "程式啟動失敗，路徑:" + openFileName
                            };
                            Form1.loglist.Insert(0, logitem);
                        }
                    }
                    isOnDelayStart = false;
                    inOnProcess = false;
                }
            ).Start();
        }
    }
}
