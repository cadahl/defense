// 
// Server.cs
//  
// Author:
//       carl <>
// 
// Copyright (c) 2010 carl
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
namespace Defense
{
	using System;
	using System.Collections.Generic;
	using System.Net;
	using System.Net.Sockets;
	using System.Threading;
	using Lidgren.Network;

	public class NetworkServer
	{
		Lidgren.Network.NetServer _srv;
		private TcpListener _listener;
		private Queue<Client.Sim.ObjectStatePacket> _packets = new Queue<Client.Sim.ObjectStatePacket>();
		
		public NetworkServer ()
		{
			var cfg = new NetConfiguration("aav.defense");
			cfg.Address = IPAddress.Any;
			cfg.Port = 52244;
			cfg.AnswerDiscoveryRequests = true;
			_srv = new NetServer(cfg);

			_srv.Start();
		}

		public void Update()
		{
			
			
		}	
		
		private void AcceptConnection(IAsyncResult r)
		{
			TcpListener listener = (TcpListener)r.AsyncState;
			TcpClient client = listener.EndAcceptTcpClient(r);

			_thread = new Thread( () =>
            {
				NetworkStream ns = client.GetStream();
				
				ns.rea
				
			});
			
			listener.BeginAcceptTcpClient(AcceptConnection, listener);
		}
	}
}

