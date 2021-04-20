//#define LOCALFILE
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSManager
{
    class Logger
    {
        public static void log(string msg)
        {
#if LOCALFILE
            window_file_log.Log(msg);
#else
            Console.WriteLine(msg);
#endif
        }
    }
}
