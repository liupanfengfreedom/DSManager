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
        Dictionary<int,dsinfor> dss;
        Launchserver LS;
        static DSManager dsm=null;
        public DSManager() {
            dss = new Dictionary<int, dsinfor>();
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
        public dsinfor LaunchADS()
        {
            dsinfor ds = LS.CreateADSInstance();
            if (ds == null)
            {

            }
            else {
                dss.Add(ds.port,ds);
                //Console.WriteLine("d.ProcessName: " + ds.ProcessName);
                //Console.WriteLine("d.Id: " + ds.Id);
                ds.process.Disposed += (object sender, EventArgs e) =>
                {
                    //Console.Write("Disposed");
                    window_file_log.Log("Disposed" + ds.process.Id);
                };
                ds.process.Exited += (object sender, EventArgs e) =>
                {
                    //Console.Write("exit");
                    window_file_log.Log("exit" + ds.process.Id);
                };
            }
            window_file_log.Log("create" + ds.process.Id);
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
                if (d.Value != null && !d.Value.process.HasExited)
                {
                    KillProcessAndChildren(d.Value.process.Id);
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
