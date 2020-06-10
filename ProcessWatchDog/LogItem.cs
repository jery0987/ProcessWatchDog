using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessWatchDog
{
    [Serializable]
    public class LogItem
    {
        public DateTime time;
        public string processname;
        public string message;
    }
}
