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
       //public void startprocess(string path)
       // {
       //     Process myProcess = new Process();
       //     {
       //         myProcess.StartInfo.UseShellExecute = false;
       //         myProcess.StartInfo.FileName = path; //apppath;
       //         //myProcess.StartInfo.Arguments = "";
       //         myProcess.StartInfo.CreateNoWindow = false;
       //         myProcess.Start();
       //     }
       // }
       // public void startthread(Func<string,bool> c,int delaytime, string parastr)
       // {
       //     Task.Run(async () =>
       //     {
       //         try
       //         {
       //             while (true)
       //             {
       //                 bool b = c.Invoke(parastr);
       //                 if (b)
       //                 {
       //                     break;
       //                 }
       //                 await Task.Delay(delaytime);
       //             }
       //         }
       //         catch (Exception e)
       //         {
       //             Logger.log(e.ToString());
       //         }
       //     });
       // }
        public void funct( Func<string,bool> c,string parastr)
        {
            c.Invoke(parastr);
        }
        public void printv1(string s)
        {
            Console.WriteLine(s);
           // startprocess(s);
        }
    }
}
