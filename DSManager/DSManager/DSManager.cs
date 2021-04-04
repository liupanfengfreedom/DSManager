using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Management;
namespace DSManager
{

    class DSManager
    {
        List<Process> dss;
        Launchserver LS;
        static DSManager dsm=null;
        public DSManager() {
            dss = new List<Process>();
            LS = new Launchserver();
        }
         ~DSManager() {
            cleardss();
        }
        public static DSManager GetSingleton()
        {
            if (dsm == null)
            {
                dsm = new DSManager();
            }
            return dsm;
        }
        public Process LaunchADS()
        {
            Process ds = LS.CreateADSInstance();
            if (ds == null)
            {

            }
            else {
                dss.Add(ds);
                Console.WriteLine("d.ProcessName: " + ds.ProcessName);
                Console.WriteLine("d.Id: " + ds.Id);
                ds.Disposed += (object sender, EventArgs e) =>
                {
                    Console.Write("Disposed");
                    window_file_log.Log("Disposed" + ds.Id);
                };
                ds.Exited += (object sender, EventArgs e) =>
                {
                    Console.Write("exit");
                    window_file_log.Log("exit" + ds.Id);
                };
            }
            window_file_log.Log("create" + ds.Id);
            return ds;
        }
        public void killds(Process ps)
        {
            if (ps != null && !ps.HasExited)
            {
                KillProcessAndChildren(ps.Id);
            }
        }
        public void cleardss()
        {
            foreach (var d in dss)
            {
                if (d != null && !d.HasExited)
                {
                    KillProcessAndChildren(d.Id);
                }
            }
            dss.Clear();
        }
        private  void KillProcessAndChildren(int pid)
        {
            // Cannot close 'system idle process'.
            if (pid == 0)
            {
                return;
            }
            ManagementObjectSearcher searcher = new ManagementObjectSearcher
                    ("Select * From Win32_Process Where ParentProcessID=" + pid);
            ManagementObjectCollection moc = searcher.Get();
            foreach (ManagementObject mo in moc)
            {
                KillProcessAndChildren(Convert.ToInt32(mo["ProcessID"]));
            }
            try
            {
                Process proc = Process.GetProcessById(pid);
                proc.Kill();
            }
            catch (ArgumentException)
            {
                // Process already exited.
            }
        }
    }

}
