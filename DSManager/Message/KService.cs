using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace DSManager
{
	public static class KcpProtocalType
	{
		public const uint SYN = 1;
		public const uint ACK = 2;
		public const uint FIN = 3;
		public const uint PING = 4;
	}
	public delegate void OnAcceptAKchannel(ref KChannel channel);
public	class KService
    {
		public OnAcceptAKchannel onAcceptAKchannel;
		public uint TimeNow { get; set; }
		private uint IdGenerater = 1000;
		public readonly ConcurrentDictionary<long, KChannel> idChannels = new ConcurrentDictionary<long, KChannel>();
		public readonly ConcurrentDictionary<long, KChannel> requestChannels = new ConcurrentDictionary<long, KChannel>();
		private UdpClient socket;
		public KService(int port)//for server
		{
			this.socket = new UdpClient(new IPEndPoint(IPAddress.Any, port));
			this.StartRecv();
			Update();
		}

		public KService()//for client
		{
			this.socket = new UdpClient(new IPEndPoint(IPAddress.Any, 0));
			this.StartRecv();
			Update();
		}
		public async void StartRecv()
		{
			while (true)
			{
				if (this.socket == null)
				{
					return;
				}

				UdpReceiveResult udpReceiveResult;
				try
				{
					udpReceiveResult = await this.socket.ReceiveAsync();
				}
				catch (Exception e)
				{
					continue;
				}

				try
				{
					int messageLength = udpReceiveResult.Buffer.Length;

					// 长度小于4，不是正常的消息
					if (messageLength < 4)
					{
						continue;
					}

					// accept
					uint conn = BitConverter.ToUInt32(udpReceiveResult.Buffer, 0);
					// conn从1000开始，如果为1，2，3则是特殊包
					switch (conn)
					{
						case KcpProtocalType.SYN:
							// 长度!=8，不是accpet消息
							if (messageLength != 8)
							{
								break;
							}
							this.HandleAccept(udpReceiveResult);
							break;
						case KcpProtocalType.ACK:
							// 长度!=12，不是connect消息
							if (messageLength != 12)
							{
								break;
							}
							this.HandleConnect(udpReceiveResult);
							break;
						case KcpProtocalType.FIN:
							// 长度!=12，不是DisConnect消息
							if (messageLength != 12)
							{
								break;
							}
							this.HandleDisConnect(udpReceiveResult);
							break;
						case KcpProtocalType.PING://client ping server
							// 长度!=12，不是PING消息
							if (messageLength != 8)
							{
								break;
							}
							this.HandlePing(udpReceiveResult);
							break;
						default:
							this.HandleRecv(udpReceiveResult, conn);
							break;
					}
				}
				catch (Exception e)
				{
					continue;
				}
			}
		}
		private void HandleAccept(UdpReceiveResult udpReceiveResult)//server do this
		{
			uint requestConn = BitConverter.ToUInt32(udpReceiveResult.Buffer, 4);
			if (requestChannels.ContainsKey(requestConn))
			{
				if (requestChannels[requestConn].isConnected)
				{
					KChannel outkc;
					requestChannels.TryRemove(requestConn, out outkc);
					return;
				}
			}
			else
			{ 
				uint newid;
				do {
					newid = this.IdGenerater++;
				}
				while (idChannels.ContainsKey(newid));
				KChannel channel = new KChannel(newid, requestConn,this.socket, udpReceiveResult.RemoteEndPoint,this);
				requestChannels.TryAdd(channel.requestConn, channel);
				idChannels.TryAdd(channel.Id, channel);
				onAcceptAKchannel.Invoke(ref channel);
			}
			requestChannels[requestConn].HandleAccept();
		}
		private void HandleConnect(UdpReceiveResult udpReceiveResult)//client do this
		{
			uint id = BitConverter.ToUInt32(udpReceiveResult.Buffer, 4);
			uint requestConn = BitConverter.ToUInt32(udpReceiveResult.Buffer, 8);

			if (idChannels.ContainsKey(requestConn))
			{
				idChannels[requestConn].HandleConnnect(id);
				idChannels.TryAdd(id, idChannels[requestConn]);
				KChannel outkc;
				idChannels.TryRemove(requestConn, out outkc);
			}
			else
			{ 
			}
		}
		public KChannel CreateAConnectChannel(IPEndPoint remotserveripEndPoint)//client do this
		{
			uint id = (uint)RandomHelper.RandomNumber(100, int.MaxValue);
			//id = IdGenerater++;
			KChannel channel = new KChannel(id, this.socket, remotserveripEndPoint);
			idChannels.TryAdd(channel.requestConn, channel);
			return channel;
		}
		private void HandleDisConnect(UdpReceiveResult udpReceiveResult)
		{
			//uint requestConn = BitConverter.ToUInt32(udpReceiveResult.Buffer, 8);

			//KChannel kChannel;
			//if (!this.idChannels.TryGetValue(requestConn, out kChannel))
			//{
			//	return;
			//}
			//// 处理chanel
			//this.idChannels.Remove(requestConn);
		}
		private void HandlePing(UdpReceiveResult udpReceiveResult)
		{
			uint id = BitConverter.ToUInt32(udpReceiveResult.Buffer,4);
            KChannel kChannel;
            if (!this.idChannels.TryGetValue(id, out kChannel))
            {
                return;
            }
			kChannel.HandlePing();
		}
		private void HandleRecv(UdpReceiveResult udpReceiveResult, uint conn)
		{
			KChannel kChannel;
			if (!this.idChannels.TryGetValue(conn, out kChannel))
			{
				return;
			}
			// 处理chanel
			kChannel.HandleRecv(udpReceiveResult);
		}
        public void Update()
        {
            Task.Run(async () =>
            {
                try
                {
                    while (true)
                    {
                        await Task.Delay(1);
                        this.TimeNow = (uint)TimeHelper.ClientNowSeconds();
                        for (int i = 0; i < idChannels.Values.Count; i++)
                        {
							if (idChannels.Values.ToArray()[i].ispendingdestory)
							{
								KChannel outkc;
								idChannels.TryRemove(idChannels.Values.ToArray()[i].Id, out outkc);
							}
							else
							{ 
							     idChannels.Values.ToArray()[i].Update(TimeNow);
							}
						}
                    }
                }
                catch (Exception e)
                {
					Logger.log(e.ToString());
				}
			});
        }
    }
}
