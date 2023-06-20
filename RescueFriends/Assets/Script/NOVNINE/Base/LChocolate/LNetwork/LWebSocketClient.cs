using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using WebSocketSharp.Net;

namespace LChocolate
{
    public class LWebSocketClient<T> : LBaseObject where T : LNetworkClient_Interface
    {
        private System.String m_strHostNameOrAddress;
        private E_CLIENT_NETWORK_STATUS m_eClientNetworkStatus;
        //private System.Byte m_ucTryConnectCount = 0;

//        private Uri m_kUri = null;
        private T m_pkT = default(T);
        
        public static bool ShowNetworkPopup   = false;

        bool m_bPaused   = true;
        bool tryReconnect   = false;
        bool notReachable   = false;
        bool callClose = false;

        void setStatus(E_CLIENT_NETWORK_STATUS status)
        {
            m_eClientNetworkStatus  = status;
        }
        public E_CLIENT_NETWORK_STATUS getStatus()
        {
            return m_eClientNetworkStatus;
        }
        public LWebSocketClient()
        {
            setStatus(E_CLIENT_NETWORK_STATUS.E_CLIENT_NETWORK_STATUS_NONE);
        }

        public void SetNotReachable()
        {
            notReachable = Application.internetReachability == NetworkReachability.NotReachable;
        }
            
        public void Paused(bool paused)
        {
            m_bPaused = paused; 
        }

        public System.Boolean Create(T kT, System.String hostNameOrAddress)
        {
            if (null == kT || string.IsNullOrEmpty(hostNameOrAddress))
                return false;

            m_pkT = kT;

//            if (m_strHostNameOrAddress != hostNameOrAddress)
//                m_kUri = new Uri(hostNameOrAddress);    

            m_strHostNameOrAddress = hostNameOrAddress;
			
            Connect(m_strHostNameOrAddress);
            return true;
        }
            
        public void Network_Update()
        {}

        public System.Boolean GetPacket(ref ST_Packet packet)
        {
            byte[] kRecvedByteArray = Recv();
            if (null == kRecvedByteArray)
                return false;
            
            if (!LNetwork.MakePacketForWebSocket(ref kRecvedByteArray, ref packet))
                return false;
            
            return true;
        }
            
        public System.Boolean IsValidConnection()
        {
            return true;
        }

        public bool InternetNotReachable()
        {
            return notReachable;
        }

#if UNITY_WEBGL && !UNITY_EDITOR
    private WebSocket m_kWebSocket = null;
	int m_NativeRef = 0;
        
	public IEnumerator Connect()
	{
        m_kWebSocket = new WebSocket(m_kUri);

		m_NativeRef = m_kWebSocket.Call_SocketCreate(m_kUri.AbsoluteUri);

		while (m_kWebSocket.Call_SocketState(m_NativeRef) == 0)
			yield return 0;
        
        if (null == error)
        {
            m_eClientNetworkStatus = E_CLIENT_NETWORK_STATUS.E_CLIENT_NETWORK_STATUS_CONNECTED;
            m_pkT.Network_Connect_Complete();
        }
        else
        {
            m_eClientNetworkStatus = E_CLIENT_NETWORK_STATUS.E_CLIENT_NETWORK_STATUS_CONNECTING_FAIL;
            m_pkT.Network_Connect_Fail();
        }
	}
 
	public void Send(byte[] buffer)
	{
		m_kWebSocket.Call_SocketSend(m_NativeRef, buffer, buffer.Length);
	}

	public byte[] Recv()
	{
		int length = m_kWebSocket.Call_SocketRecvLength(m_NativeRef);
		if (length == 0)
			return null;
		byte[] buffer = new byte[length];
		m_kWebSocket.Call_SocketRecv(m_NativeRef, buffer, length);
		return buffer;
	}

	public void Close()
	{
        m_eClientNetworkStatus = E_CLIENT_NETWORK_STATUS.E_CLIENT_NETWORK_STATUS_DISCONNECTED;

		m_kWebSocket.Call_SocketClose(m_NativeRef);
	}

	public string error
	{
		get
        {
			const int bufsize = 1024;
			byte[] buffer = new byte[bufsize];
			int result = m_kWebSocket.Call_SocketError (m_NativeRef, buffer, bufsize);
			if (result == 0)
				return null;

        	return Encoding.UTF8.GetString (buffer);				
		}
	}
#else
        List<WebSocketSharp.WebSocket> m_kClosedWebSocketPools = new List<WebSocketSharp.WebSocket>();
        WebSocketSharp.WebSocket m_kWebSocket = null;
        Queue<byte[]> m_Messages = new Queue<byte[]>();
        string m_Error = null;

        public int GetWebSocketCount()
        {
            //Debug.Log(string.Format("m_kClosedWebSocketPools.Count:{0}",m_kClosedWebSocketPools.Count));
            return m_kClosedWebSocketPools.Count;
        }
        public void TraceLog()
        {
            Debug.Log(string.Format("TraceLog:\n --> m_eClientNetworkStatus:{0}\n --> m_Error:{1}\n --> internetReachability:{2}\n",m_eClientNetworkStatus,m_Error,Application.internetReachability));
            if (m_kWebSocket != null)
                Debug.Log(string.Format("m_kWebSocket:\n --> IsAlive:{0}\n --> Ping:{1}", m_kWebSocket.IsAlive, m_kWebSocket.Ping()));
            else
                Debug.Log("m_kWebSocket = null");
        }

        void OnMessage(object sender, WebSocketSharp.MessageEventArgs e)
        {
            WebSocketSharp.WebSocket socket = (WebSocketSharp.WebSocket)sender;
            //Debug.Log("--------OnMessage: m_kWebSocket = socket : " + (socket == m_kWebSocket));
            m_Messages.Enqueue(e.RawData);
        }

        void OnOpen(object sender, EventArgs e)
        {
            WebSocketSharp.WebSocket socket = (WebSocketSharp.WebSocket)sender;
            if (socket != null)
            {
                //Debug.Log("--------OnOpen: m_kWebSocket = socket : " + (socket == m_kWebSocket));
                //socket.OnOpen -= OnOpen;

                if (socket == m_kWebSocket)
                {
                    //Debug.Log("--------E_CLIENT_NETWORK_STATUS_CONNECTED:" + m_kWebSocket.GetHashCode());
                    tryReconnect   = false;
                    setStatus(E_CLIENT_NETWORK_STATUS.E_CLIENT_NETWORK_STATUS_CONNECTED);
                    m_pkT.RunOnGameThread(() =>{
                        m_pkT.Network_Connect_Complete();
                    });
                }
            }
        }

        void OnError(object sender, WebSocketSharp.ErrorEventArgs e)
        {
            WebSocketSharp.WebSocket socket = (WebSocketSharp.WebSocket)sender;
            if (socket != null)
            {
                //socket.OnError -= OnError;
                //Debug.Log(":--------m_Error: " + m_Error + "\n m_kWebSocket = socket : " + (socket == m_kWebSocket));
                if (socket == m_kWebSocket)
                {
                  //  Debug.Log("--------m_Error E_CLIENT_NETWORK_STATUS_DISCONNECTED:" + m_kWebSocket.GetHashCode());
                    m_Error = e.Message;
                    setStatus(E_CLIENT_NETWORK_STATUS.E_CLIENT_NETWORK_STATUS_DISCONNECTED);
                }
            }
        }

        void OnClose(object sender, WebSocketSharp.CloseEventArgs e)
        {
            WebSocketSharp.WebSocket socket = (WebSocketSharp.WebSocket)sender;
            if (socket != null)
            {
                //Debug.Log("--------OnClose:" + "e.Reason:" + e.Reason +" : "+ e.Code + ":" + e.WasClean + "\n m_kWebSocket = socket : " + (socket == m_kWebSocket));
                //socket.OnClose -= OnClose;

                if (socket == m_kWebSocket)
                {
                    setStatus(E_CLIENT_NETWORK_STATUS.E_CLIENT_NETWORK_STATUS_CLOSED);
                    callClose = true;
                }
                else
                    m_kClosedWebSocketPools.Add(socket);
            }
        }
            
        WebSocketSharp.WebSocket GetWebSocket(  string absoluteUri )
        {
            string token = absoluteUri + "/";


            if (m_kWebSocket != null && m_kWebSocket.Url.AbsoluteUri == absoluteUri)
                return m_kWebSocket;
            
            WebSocketSharp.WebSocket _socket = null;

            for (int i = 0; i < m_kClosedWebSocketPools.Count; ++i)
            {
                if (token == m_kClosedWebSocketPools[i].Url.AbsoluteUri)
                {
                    _socket = m_kClosedWebSocketPools[i];
                    m_kClosedWebSocketPools.RemoveAt(i);
                    //Debug.Log("m_kClosedWebSocketPools.RemoveAt " + i);
                    break;
                }    
            }

            if (_socket == null)
            {
                _socket = new WebSocketSharp.WebSocket(absoluteUri);
                //Debug.Log("_socket new :" + _socket.GetHashCode());
                _socket.OnMessage += OnMessage;
                _socket.OnOpen += OnOpen;
                _socket.OnError += OnError;
                _socket.OnClose += OnClose;
            }
            //Debug.Log("_socket:" + _socket.GetHashCode());
            return _socket;
        }

        public void Connect( string _url)
        {
            if (m_kWebSocket != null)
            {
                if(callClose == false)
                    Network_CloseAsync();
            }

            if (notReachable)
                return;

            callClose = false;
            setStatus( E_CLIENT_NETWORK_STATUS.E_CLIENT_NETWORK_STATUS_CONNECTING );
            m_kWebSocket = GetWebSocket(_url);
//            Debug.Log("m_kWebSocket.ConnectAsync()----->1");
            m_kWebSocket.ConnectAsync();
 //           Debug.Log("m_kWebSocket.ConnectAsync()----->2");
        }

        public void Send(byte[] buffer)
        {
            if(m_kWebSocket != null)
                m_kWebSocket.Send(buffer);
        }

        public void SendAsync(byte[] buffer, Action<bool> completed)
        {
            if(m_kWebSocket != null)
                m_kWebSocket.SendAsync(buffer,completed);
        }

        public void Network_CloseAsync()
        {
            return;
        }

        public void Network_Close()
        {
            return;
        }

        public byte[] Recv()
        {
            if (m_Messages.Count == 0)
                return null;
            
            return m_Messages.Dequeue();
        }

        public string error
        {
            get
            {
                return m_Error;
            }
        }
#endif
    }
}