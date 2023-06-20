using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections;
using UnityEngine;
using System.Runtime.InteropServices;

public class WebSocket
{
	private Uri m_kUri;

    public WebSocket(Uri kUri)
    {
        m_kUri = kUri;

        string protocol = kUri.Scheme;
        if (!protocol.Equals("ws") && !protocol.Equals("wss"))
            throw new ArgumentException("Unsupported protocol: " + protocol);
    }

    /*
    public System.Boolean Create(System.String hostNameOrAddress)
    {
        m_kUri = new Uri(hostNameOrAddress);
        string protocol = m_kUri.Scheme;
        if (!protocol.Equals("ws") && !protocol.Equals("wss"))
        {
            //throw new ArgumentException("Unsupported protocol: " + protocol);
            return false;
        }

        return true;
    }

    public void SendString(string str)
	{
		Send(Encoding.UTF8.GetBytes (str));
	}

	public string RecvString()
	{
		byte[] retval = Recv();
		if (retval == null)
			return null;
		return Encoding.UTF8.GetString (retval);
	}
    */
#if UNITY_WEBGL && !UNITY_EDITOR
	[DllImport("__Internal")]
	private static extern int SocketCreate (string url);

	[DllImport("__Internal")]
	private static extern int SocketState (int socketInstance);

	[DllImport("__Internal")]
	private static extern void SocketSend (int socketInstance, byte[] ptr, int length);

	[DllImport("__Internal")]
	private static extern void SocketRecv (int socketInstance, byte[] ptr, int length);

	[DllImport("__Internal")]
	private static extern int SocketRecvLength (int socketInstance);

	[DllImport("__Internal")]
	private static extern void SocketClose (int socketInstance);

	[DllImport("__Internal")]
	private static extern int SocketError (int socketInstance, byte[] ptr, int length);
    
    public int Call_SocketCreate(string url)
    {
        return SocketCreate (url);
    }
    public int Call_SocketState(int socketInstance)
    {
        return SocketState(socketInstance);
    }
    public void Call_SocketSend(int socketInstance, byte[] ptr, int length)
    {
        SocketSend(socketInstance, ptr, length);
    }
    public void Call_SocketRecv(int socketInstance, byte[] ptr, int length)
    {
        SocketRecv(socketInstance, ptr, length);
    }
    public int Call_SocketRecvLength (int socketInstance)
    {
        return SocketRecvLength(socketInstance);
    }
    public void Call_SocketClose(int socketInstance)
    {
        SocketClose(socketInstance);
    }
    public int Call_SocketError(int socketInstance, byte[] ptr, int length)
    {
        return SocketError( socketInstance, ptr, length);
    }
#else
#endif 
}