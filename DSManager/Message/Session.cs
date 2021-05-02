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
        static readonly Dictionary<int,Session> session_s=new Dictionary<int, Session>();
        public Session()//client
        {
            kService_c = new KService();
        }
        public Session(int port)//server
        {
            kService_s = new KService(port);
        }
        public static Session getnew()//client
        {
            return new Session();
        }
        public static KService createorget(int port)//server
        {
            Session session;
            if (session_s.ContainsKey(port))
            {
                session =  session_s[port];
            }
            else
            {
                session = new Session(port);
                session_s.Add(port, session);
            }
            return session.kService_s;
        }
        public  KChannel GetChannel(IPEndPoint remotserveripEndPoint)
        {
            return kService_c.CreateAConnectChannel(remotserveripEndPoint);
        }

    }
}
