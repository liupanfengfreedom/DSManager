using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace DSManager
{
    class LaunchApp
    {
       public void startprocess(string path)
        {
            Process myProcess = new Process();
            {
                myProcess.StartInfo.UseShellExecute = false;
                myProcess.StartInfo.FileName = path; //apppath;
                //myProcess.StartInfo.Arguments = "";
                myProcess.StartInfo.CreateNoWindow = false;
                myProcess.Start();
            }
        }
        public void printv1(string s)
        {
            Console.WriteLine(s);
            startprocess(s);
        }
    }
}
