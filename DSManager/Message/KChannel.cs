using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Net.Sockets.Kcp;
using System.Text;
using System.Threading.Tasks;

namespace DSManager
{
  public delegate void OnUserLevelReceivedCompleted(ref byte[] buffer);
  public  class KChannel
    {
        const int PINGPERIOD = 1;//60*60;//s
        public OnUserLevelReceivedCompleted onUserLevelReceivedCompleted;
        public Action ondisconnect;
        Timerhandler th;
        public uint Id { get; set; }
        public uint requestConn { get; set; }
        private UdpClient socket;//local socket
        public bool isConnected { get; private set; }
        private IPEndPoint remoteEndPoint;
		private Kcp kcp;
		private KService kService;
        private readonly byte[] cacheBytes = new byte[ushort.MaxValue];
		private uint lastpingtime { get; set; }
        public KChannel(uint conn, uint requestConn, UdpClient socket, IPEndPoint remoteEndPoint, KService kService)//server do this
        {
            this.Id = conn;
            this.requestConn = requestConn;
            this.kService = kService;
            this.socket = socket;
            this.remoteEndPoint = remoteEndPoint;
            kcphandle handle = new kcphandle();
            handle.Out = buffer => {
                this.socket.Send(buffer.ToArray(),buffer.Length, this.remoteEndPoint);
            };
            kcp = new Kcp(this.Id, handle);
            kcp.NoDelay(1, 10, 2, 1);//fast
            kcp.WndSize(64, 64);
            kcp.SetMtu(512);
            this.isConnected = true;

            lastpingtime = (uint)TimeHelper.ClientNowSeconds();
            th = new Timerhandler((string s) =>
            {
                if ((uint)TimeHelper.ClientNowSeconds() - lastpingtime > PINGPERIOD * 9)
                {
                    th.kill = true;
                    disconnect();
                }
                //Console.WriteLine("check ping");

            }, "", PINGPERIOD * 1000 *10, true);//unit of timer is ms
            Global.GetComponent<Timer>().Add(th);
        }
        public KChannel(uint requestConn, UdpClient socket, IPEndPoint remoteEndPoint)//client do this
        {
            this.requestConn = requestConn;
            this.socket = socket;
            this.remoteEndPoint = remoteEndPoint;
            th = new Timerhandler((string s) => {
                cacheBytes.WriteTo(0, KcpProtocalType.SYN);
			    cacheBytes.WriteTo(4, this.requestConn);
                //Log.Debug($"client connect: {this.Conn}");
                this.socket.Send(cacheBytes,8, this.remoteEndPoint);
            }, "", 200, true);
            Global.GetComponent<Timer>().Add(th);
        }
        ~KChannel()
        {
            //Console.WriteLine("~KChannel()");
            window_file_log.Log("~KChannel()");
        }
        public void HandleAccept()//server do this
        {
            cacheBytes.WriteTo(0, KcpProtocalType.ACK);
            cacheBytes.WriteTo(4, this.Id);
            cacheBytes.WriteTo(8, this.requestConn);
            this.socket.Send(cacheBytes, 12, remoteEndPoint);
        }
        public void HandleConnnect(uint id)//client do this
        {
            if (this.isConnected)
            {
                return;
            }
            this.Id = id;
            kcphandle handle = new kcphandle();
            handle.Out = buffer => {
                this.socket.Send(buffer.ToArray(), buffer.Length, this.remoteEndPoint);
            };
            kcp = new Kcp(this.Id, handle);
            kcp.NoDelay(1, 10, 2, 1);//fast
            kcp.WndSize(64, 64);
            kcp.SetMtu(512);
            this.isConnected = true;
            th.kill = true;
            th = new Timerhandler((string s) =>
            {
                cacheBytes.WriteTo(0, KcpProtocalType.PING);
                cacheBytes.WriteTo(4, this.Id);
                //Log.Debug($"client connect: {this.Conn}");
                this.socket.Send(cacheBytes, 8, this.remoteEndPoint);
                //Console.WriteLine("ping");
                window_file_log.Log("ping");
            }, "", PINGPERIOD * 1000, true);
            Global.GetComponent<Timer>().Add(th);
        }
        public void HandlePing()
        {
            lastpingtime = (uint)TimeHelper.ClientNowSeconds();
            //Console.WriteLine("ping");

        }
        public void HandleRecv(UdpReceiveResult urr)
        {
            if (kService != null)
            {
                if (remoteEndPoint.Port != urr.RemoteEndPoint.Port || remoteEndPoint.Address.ToString() != urr.RemoteEndPoint.Address.ToString())//here is in case wifi toorfrom 4g 
                {
                    remoteEndPoint = urr.RemoteEndPoint;
                }
            }
            this.kcp.Input(urr.Buffer);

        }

        public void Update(uint timeNow)
        {
            if (!this.isConnected)
            {
                return;
            }
            //this.kcp.Check(DateTime.UtcNow);
            this.kcp.Update(DateTime.UtcNow);
            int len;
            while ((len = kcp.PeekSize()) > 0)
            {
                var buffer = new byte[len];
                if (kcp.Recv(buffer) >= 0)
                {
                    onUserLevelReceivedCompleted.Invoke(ref buffer);
                }
            }
        }
        public void Send(ref byte[] buffer)
        {
            if (this.isConnected)
            {
                var res = this.kcp.Send(buffer);//send by handle.out
                if (res != 0)
                {
                    //Console.WriteLine($"kcp send error");
                }
            }
        }
        void disconnect()
        {
            kService.idChannels.Remove(Id);
            KChannel ch;
            kService.requestChannels.TryGetValue(requestConn, out ch);
            if (ch == this)
            { 
                kService.requestChannels.Remove(requestConn);
                //Console.WriteLine("kService.requestChannels.Count :" + kService.requestChannels.Count);
                window_file_log.Log("kService.requestChannels.Count :" + kService.requestChannels.Count);
            }
            //Console.WriteLine("kService.idChannels.Count :" + kService.idChannels.Count);
            window_file_log.Log("kService.requestChannels.Count :" + kService.requestChannels.Count);
            ondisconnect.Invoke();
        }
    }
    internal class kcphandle : IKcpCallback
    {
        public Action<Memory<byte>> Out;
        public void Output(IMemoryOwner<byte> buffer, int avalidLength)
        {
            Out(buffer.Memory.Slice(0, avalidLength));
        }
    }
}
