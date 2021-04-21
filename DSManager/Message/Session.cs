using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DSManager
{
    class Session
    {
        KService kService_c;
        KService kService_s;
        static Session session_c;
        static Session session_s;
        public Session()//client
        {
            kService_c = new KService();
        }
        public Session(int port)//server
        {
            kService_s = new KService(port);
        }
        public static Session get()//client
        {
            if (session_c == null)
            {
                session_c = new Session();
            }
            return session_c;
        }
        public static KService get(int port)//server
        {
            if (session_s == null)
            {
                session_s = new Session(port);
            }
            return session_s.kService_s;
        }
        public  KChannel GetChannel(IPEndPoint remotserveripEndPoint)
        {
            return kService_c.CreateAConnectChannel(remotserveripEndPoint);
        }

    }
}
