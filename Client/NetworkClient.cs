// 
// Client.cs
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
namespace Client
{
	using System;
	using System.Net;
	using System.Net.Sockets;
	using System.Threading;
	
	public class NetworkClient
	{
		private Thread _thread;
		private TcpClient _client;
		
		public NetworkClient ()
		{
			_client.BeginConnect("localhost", 52244, r => 
            {
				_client.EndConnect(r);
				
				_thread = new Thread(ConnectionThread);
				_thread.Start();
				
				
			}, null);
		}
		
		private void ConnectionThread()
		{
		}
		
	}
}

