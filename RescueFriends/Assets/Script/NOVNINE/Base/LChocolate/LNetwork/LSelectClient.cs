using UnityEngine;
//using System.Collections;

using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System;

namespace LChocolate
{
    public class LSelectClient<T> : LBaseObject where T : LNetworkClient_Interface
    {
        const System.UInt32 MAX_CLIENT_RECVBUFFER_SIZE = NetworkDefine.MAX_PACKET_SIZE * 5;
        const System.UInt32 MAX_CLIENT_SENDBUFFER_SIZE = NetworkDefine.MAX_PACKET_SIZE * 5;

        private System.Byte[] m_stRecvBuffer = null;
        private System.Byte[] m_stTempRecvBuffer = null;
        private System.UInt32 m_uiRecvBufferAccumulateSize = 0;

        private System.Byte[] m_stSendBuffer = null;
        private System.Byte[] m_stTempSendBuffer = null;
        private System.UInt32 m_uiSendBufferAccumulateSize = 0;
        private System.UInt32 m_uiSendBuffer_CurrentSendPosition = 0;

        private Socket m_kSocket = null;

        private System.String m_strHostNameOrAddress;
        private System.UInt16 m_usPort;

        private System.Boolean m_bRecvPacketEncrypted = false;
        private System.Boolean m_bSendPacketEncryption = false;


        private E_CLIENT_NETWORK_STATUS m_eClientNetworkStatus;
        
        private System.Byte m_ucTryConnectCount = 0;


        private System.Boolean m_bDisposed = false;

        private System.Boolean m_bRecvStatus = false;
        private System.Boolean m_bSendStatus = false;

        private T m_pkT = default(T);
        
        /*
        // 굳이 싱글톤일 필요 없지???
        private static LSelectClient<T> pkLSelectClient = null;
        public static LSelectClient<T> GetInstance()
        {
            if (null == pkLSelectClient)
            {
                pkLSelectClient = new LSelectClient<T>();
            }
            return pkLSelectClient;
        }
        */

        private System.Object m_kLockObject = new System.Object();

        public LSelectClient()
        {
            m_stRecvBuffer = new byte[MAX_CLIENT_RECVBUFFER_SIZE];
            m_uiRecvBufferAccumulateSize = 0;
            m_stTempRecvBuffer = null;

            m_stSendBuffer = new byte[MAX_CLIENT_SENDBUFFER_SIZE];
            m_uiSendBufferAccumulateSize = 0;
            m_uiSendBuffer_CurrentSendPosition = 0;
            m_stTempSendBuffer = null;

            m_kSocket = null;

            m_strHostNameOrAddress = "";
            m_usPort = 0;

            m_bRecvPacketEncrypted = false;
            m_bSendPacketEncryption = false;

            m_eClientNetworkStatus = E_CLIENT_NETWORK_STATUS.E_CLIENT_NETWORK_STATUS_NONE;

            m_ucTryConnectCount = 0;

            m_bDisposed = false;

            m_bRecvStatus = false;
            m_bSendStatus = false;

            m_pkT = default(T);
        }

        public void Network_Close()
        {
            if ((null != m_kSocket) && m_kSocket.Connected)
            {
                m_uiRecvBufferAccumulateSize = 0;
                m_stTempRecvBuffer = null;

                m_uiSendBufferAccumulateSize = 0;
                m_uiSendBuffer_CurrentSendPosition = 0;
                m_stTempSendBuffer = null;

                if (m_kSocket.Connected)
                {
                    m_kSocket.Shutdown(System.Net.Sockets.SocketShutdown.Both);
                    //m_kSocket.Disconnect(false);    // 이 함수 unity 에서 지원안됨. 사용 불가
                    //m_kSocket.Dispose();  // 이 함수 unity 에서 지원안됨. 사용 불가
                    m_kSocket.Close();

                    m_kSocket = null;
                }
                
                m_ucTryConnectCount = 0;

                m_bRecvStatus = false;
                m_bSendStatus = false;


                m_eClientNetworkStatus = E_CLIENT_NETWORK_STATUS.E_CLIENT_NETWORK_STATUS_DISCONNECTED;
            }
        }

        public override void Dispose(System.Boolean disposing)
        {
            if (!m_bDisposed)
            {
                m_stRecvBuffer = null;
                m_stTempRecvBuffer = null;

                m_stSendBuffer = null;
                m_stTempSendBuffer = null;

                if ((null != m_kSocket) && m_kSocket.Connected)
                {
                    m_kSocket.Shutdown(System.Net.Sockets.SocketShutdown.Both);
                    //m_kSocket.Disconnect(false);    // 이 함수 unity 에서 지원안됨. 사용 불가
                    //m_kSocket.Dispose();  // 이 함수 unity 에서 지원안됨. 사용 불가
                    m_kSocket.Close();
                    m_eClientNetworkStatus = E_CLIENT_NETWORK_STATUS.E_CLIENT_NETWORK_STATUS_DISCONNECTED;
                }

                if (disposing)
                {
                    // Release managed resources.
                }
                // Release unmanaged resources.
                // Set large fields to null.
                // Call Dispose on your base class.
                m_bDisposed = true;
            }
            base.Dispose(disposing);
        }

        public System.Boolean IsValidConnection()
        {
            //return System.Convert.ToInt32(E_CLIENT_NETWORK_STATUS.E_CLIENT_NETWORK_STATUS_CONNECTED) == m_eClientNetworStatus;
            return E_CLIENT_NETWORK_STATUS.E_CLIENT_NETWORK_STATUS_CONNECTED == m_eClientNetworkStatus;
        }

        public System.Boolean Create(T kT, System.String hostNameOrAddress, System.UInt16 port, System.Boolean recvPacketEncrypted, System.Boolean sendPacketEncryption)
        {
            if ( (null == kT) || (1 > hostNameOrAddress.Length) || (1 > port))
                return false;

            Network_Close();

            m_pkT = kT;
            
            m_strHostNameOrAddress = hostNameOrAddress;
            m_usPort = port;

            m_bRecvPacketEncrypted = recvPacketEncrypted;
            m_bSendPacketEncryption = sendPacketEncryption;

            return TryConnect();
        }

        //public void Netowrk_Update(System.Single deltaTime)
        public void Network_Update()
        {
            switch (m_eClientNetworkStatus)
            {
                case E_CLIENT_NETWORK_STATUS.E_CLIENT_NETWORK_STATUS_CONNECTING:
                    break;
                case E_CLIENT_NETWORK_STATUS.E_CLIENT_NETWORK_STATUS_CONNECTING_FAIL:
                    {
                        if (NetworkDefine.MAX_CLIENT_TRY_CONNECT_COUNT > m_ucTryConnectCount)
                        {
                            ++m_ucTryConnectCount;
                            TryConnect();
                        }
                        else
                        {
                            m_eClientNetworkStatus = E_CLIENT_NETWORK_STATUS.E_CLIENT_NETWORK_STATUS_RECONNECT_FAIL;
                            m_ucTryConnectCount = 0;

                            m_pkT.Network_Connect_Fail();
                        }
                    }
                    break;
                case E_CLIENT_NETWORK_STATUS.E_CLIENT_NETWORK_STATUS_CONNECTED:
                    {
                        SendPacketDatas(0, m_uiSendBufferAccumulateSize);
                        RecvPacket();
                    }
                    break;

                case E_CLIENT_NETWORK_STATUS.E_CLIENT_NETWORK_STATUS_RECONNECT_FAIL:
                    break;

                case E_CLIENT_NETWORK_STATUS.E_CLIENT_NETWORK_STATUS_DISCONNECTED:
                    break;
            }
        }


        public System.Boolean TryConnect()
        {
            UnityEngine.Debug.Log("LSelectClient::TryConnect start");

            try
            {
                IPHostEntry kIPHostEntry = Dns.GetHostEntry(m_strHostNameOrAddress);
                for (int i = 0; kIPHostEntry.AddressList.Length > i; ++i)
                {   
                    IPAddress kIPAddress = kIPHostEntry.AddressList[i];

                    if (AddressFamily.InterNetworkV6 == kIPAddress.AddressFamily)
                        continue;

                    IPEndPoint kIPEndPoint = new IPEndPoint(kIPAddress, m_usPort);
                    Socket kSocket = new Socket(kIPAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                    // kSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.IPv6Only, 0);
                    try
                    {
                        SocketAsyncEventArgs kSocketAsyncEventArgs = new SocketAsyncEventArgs();
                        kSocketAsyncEventArgs.RemoteEndPoint = kIPEndPoint;
                        kSocketAsyncEventArgs.UserToken = kSocket;
                        kSocketAsyncEventArgs.Completed += new System.EventHandler<SocketAsyncEventArgs>(Connect_Completed);
                        
                        if (kSocket.ConnectAsync(kSocketAsyncEventArgs))
                        {
                            //UnityEngine.Debug.Log("TryConnect, kSocket.ConnectAsync Connecting");
                            m_eClientNetworkStatus = E_CLIENT_NETWORK_STATUS.E_CLIENT_NETWORK_STATUS_CONNECTING;
                        }
                        else
                        {
                            //UnityEngine.Debug.Log("TryConnect, kSocket.ConnectAsync Connected");
                            Connect_Completed(kSocket, null);
                            //m_eClientNetworkStatus = E_CLIENT_NETWORK_STATUS.E_CLIENT_NETWORK_STATUS_CONNECTED;
                        }

                        //return true;
                    }
                    catch (SocketException socketException)
                    {
                        //UnityEngine.Debug.Log("TryConnect, SocketException error");
                        //UnityEngine.Debug.Log(socketException);

                        if (null != kSocket)
                            kSocket.Close();

                        System.Console.WriteLine(socketException);
                        /*
                        if (i == resolvedServer.AddressList.Length - 1)
                            Console.WriteLine(
                                "Failed to connect to the server.");
                        */
                    }
                }
            }
            catch (SocketException kSocketException)
            {
                kSocketException.ToString();
            }

            return true;
        }

        private void Connect_Completed(object kObject, SocketAsyncEventArgs e)
        {
            UnityEngine.Debug.Log("LSelectClient::Connect_Completed called");
            
            // e.ConnectSocket 유니티에서 앞에 변수만 사용하면 먹통되는데.. ㅡ.ㅡ
            if ((null != e) || (SocketError.Success == e.SocketError))
            {
                m_kSocket = (Socket)kObject;
                m_eClientNetworkStatus = E_CLIENT_NETWORK_STATUS.E_CLIENT_NETWORK_STATUS_CONNECTED;
                m_ucTryConnectCount = 0;

                m_bRecvStatus = false;
                m_bSendStatus = false;

                SendPacketDatas(0, m_uiSendBufferAccumulateSize);

                m_pkT.Network_Connect_Complete();
            }
            else
            {
                m_kSocket = null;
                m_eClientNetworkStatus = E_CLIENT_NETWORK_STATUS.E_CLIENT_NETWORK_STATUS_CONNECTING_FAIL;
            }
        }

        /*
        private static void Connect_Completed(object kObject, SocketAsyncEventArgs e)
        {
            UnityEngine.Debug.Log("Connect_Completed called");
            if (SocketError.Success == e.SocketError)
            {
                if (null != e.ConnectSocket)
                {
                    LSelectClient<T> kSelectClient = (LSelectClient<T>)kObject;
                    kSelectClient.Connect_Complete1(true, e.ConnectSocket);
                }
                else
                {
                    LSelectClient<T> kSelectClient = (LSelectClient<T>)kObject;
                    kSelectClient.Connect_Complete1(false, null);
                }
            }
            else
            {
                if (null != e.ConnectSocket)
                {
                    e.ConnectSocket.Close();
                }
                
                LSelectClient<T> kSelectClient = (LSelectClient<T>)kObject;
                kSelectClient.Connect_Complete1(false, null);
            }
        }
        */

        public void Connect_Complete1(System.Boolean isConnectSuccess, Socket connectedSocket)
        {
            if (isConnectSuccess)
            {
                m_kSocket = connectedSocket;
                m_eClientNetworkStatus = E_CLIENT_NETWORK_STATUS.E_CLIENT_NETWORK_STATUS_CONNECTED;
                m_ucTryConnectCount = 0;

                m_bRecvStatus = false;
                m_bSendStatus = false;

                SendPacketDatas(0, m_uiSendBufferAccumulateSize);

                m_pkT.Network_Connect_Complete();
            }
            else
            {
                m_kSocket = null;
                m_eClientNetworkStatus = E_CLIENT_NETWORK_STATUS.E_CLIENT_NETWORK_STATUS_CONNECTING_FAIL;
            }
        }
        
        private void RecvPacket()
        {
            if (m_kSocket != null)
            {
                if (m_kSocket.Connected)
                {
                    if (null == m_stTempRecvBuffer)
                    {
                        m_stTempRecvBuffer = new byte[LChocolate.NetworkDefine.MAX_PACKET_SIZE];
                    }

                    if (!m_bRecvStatus)
                    {
                        m_bRecvStatus = true;

                        SocketAsyncEventArgs kSocketAsyncEventArgs = new SocketAsyncEventArgs();
                        kSocketAsyncEventArgs.SetBuffer(m_stTempRecvBuffer, 0, LChocolate.NetworkDefine.MAX_PACKET_SIZE);
                        kSocketAsyncEventArgs.Completed += new System.EventHandler<SocketAsyncEventArgs>(Receive_Completed);
                        
                        m_kSocket.ReceiveAsync(kSocketAsyncEventArgs);
                    }
                }
            }
        }

        private void Receive_Completed(object kObject, SocketAsyncEventArgs e)
        {
            if (m_eClientNetworkStatus == E_CLIENT_NETWORK_STATUS.E_CLIENT_NETWORK_STATUS_CONNECTED)
            {
                if (m_kSocket.Connected)
                {
                    if (0 < e.BytesTransferred)
                    {
                        lock (m_kLockObject)
                        {
                            e.Buffer.CopyTo(m_stRecvBuffer, m_uiRecvBufferAccumulateSize);
                            m_uiRecvBufferAccumulateSize += (System.UInt32)e.BytesTransferred;
                        }
                    }
                    else
                    {
                        m_kSocket.Shutdown(System.Net.Sockets.SocketShutdown.Both);
                        //m_kSocket.Disconnect(false);  // 이 함수 unity 에서 지원안됨. 사용 불가
                        m_kSocket.Close();
                        //m_kSocket.Dispose();  // m_kSocket.Dispose(); 이것도 쓰면 안됨... ㅡ.ㅡ
                    }
                }
            }

            m_bRecvStatus = false;
        }

        /*
        public void Network_SendPacket(ref ST_Packet packet)
        {
            System.UInt16 packetSize = packet.GetPacketSize();
            if (0 < packetSize)
            {
                if (null == m_stTempSendBuffer)
                {
                    m_stTempSendBuffer = new byte[LChocolate.NetworkDefine.MAX_PACKET_SIZE];
                }

                if (ST_Packet.ConvertPacketToByteArray(ref packet, ref m_stTempSendBuffer))
                {
                    if (m_bSendPacketEncryption)
                    {
                        LSecurity.LSecuirty_EncryptDecrypt(ref m_stTempSendBuffer, NetworkDefine.PACKETSIZE_BYTE_LENGTH, (System.UInt16)(packetSize - NetworkDefine.PACKETSIZE_BYTE_LENGTH), null, 0);
                    }

                    System.Buffer.BlockCopy(m_stTempSendBuffer, 0, m_stSendBuffer, (System.Int32)m_uiSendBufferAccumulateSize, packetSize);
                    
                    Debug.Log("Network_SendPacket, packet.GetPacketCommand() = " + packet.GetPacketCommand() + " packetSize = " + packetSize);

                    m_uiSendBufferAccumulateSize += packetSize;

                    SendPacketDatas();
                }
            }
        }
        */
        public void Network_SendPacket(ref System.Byte[] packetByteArray)
        {
            if (null != packetByteArray)
            {
                if (null == m_stTempSendBuffer)
                {
                    m_stTempSendBuffer = new byte[LChocolate.NetworkDefine.MAX_PACKET_SIZE];
                }

                System.UInt16 packetSize = System.BitConverter.ToUInt16(packetByteArray, 0);
                if (0 < packetSize)
                {
                    if (m_bSendPacketEncryption)
                    {
                        LSecurity.LSecuirty_EncryptDecrypt(ref packetByteArray, NetworkDefine.PACKETSIZE_BYTE_LENGTH, (System.UInt16)(packetSize - NetworkDefine.PACKETSIZE_BYTE_LENGTH), null, 0);
                    }

                    System.Buffer.BlockCopy(packetByteArray, 0, m_stSendBuffer, (System.Int32)(m_uiSendBuffer_CurrentSendPosition + m_uiSendBufferAccumulateSize), packetSize);

                    m_uiSendBufferAccumulateSize += packetSize;

                    SendPacketDatas(m_uiSendBuffer_CurrentSendPosition + m_uiSendBufferAccumulateSize - packetSize, packetSize);
                }
            }
        }
        private void SendPacketDatas(System.UInt32 offset, System.UInt32 sendSize)
        {
            if ((0 < m_uiSendBufferAccumulateSize) && (!m_bSendStatus))
            {
                try
                {
                    System.Int32 sentSize = 0;
                    while (sentSize < m_uiSendBufferAccumulateSize)
                    {
                        sentSize += m_kSocket.Send(m_stSendBuffer, sentSize, (System.Int32)(m_uiSendBufferAccumulateSize - sentSize), SocketFlags.None);
                    }

                    m_uiSendBufferAccumulateSize = 0;
                }
                catch (SocketException e)
                {
                    Console.WriteLine("{0} Error code: {1}.", e.Message, e.ErrorCode);
                }

                /*
                m_bSendStatus = true;

                SocketAsyncEventArgs kSocketAsyncEventArgs = new SocketAsyncEventArgs();
                kSocketAsyncEventArgs.SetBuffer(m_stSendBuffer, (System.Int32)offset, (System.Int32)sendSize);
                kSocketAsyncEventArgs.Completed += new System.EventHandler<SocketAsyncEventArgs>(Send_Completed);
                m_kSocket.SendAsync(kSocketAsyncEventArgs);
                */
            }
        }
        private void Send_Completed(object kObject, SocketAsyncEventArgs e)
        {
            m_bSendStatus = false;
            if (0 < e.BytesTransferred)
            {
                if (m_uiSendBufferAccumulateSize == e.BytesTransferred)
                {
                    System.Array.Clear(m_stSendBuffer, 0, m_stSendBuffer.Length);
                    m_uiSendBufferAccumulateSize = 0;
                    m_uiSendBuffer_CurrentSendPosition = 0;
                }
                else
                {
                    Debug.Log("-----------------------------------Send 111----------------------------------------------");
                    m_uiSendBufferAccumulateSize = (System.UInt32)(m_uiSendBufferAccumulateSize - e.BytesTransferred);
                    m_uiSendBuffer_CurrentSendPosition = (System.UInt32)(m_uiSendBuffer_CurrentSendPosition + e.BytesTransferred);
                    Debug.Log("-----------------------------------Send 222----------------------------------------------");
                }
            }
        }

        public System.Boolean GetPacket(ref ST_Packet packet)
        {
            if (NetworkDefine.PACKET_HEADER_LENGTH > m_uiRecvBufferAccumulateSize)
                return false;

            lock (m_kLockObject)
            {
                System.UInt16 packetSize = System.BitConverter.ToUInt16(m_stRecvBuffer, 0);
                if (packetSize <= m_uiRecvBufferAccumulateSize)
                {
                    if (m_bRecvPacketEncrypted)
                    {
                        LSecurity.LSecuirty_EncryptDecrypt(ref m_stRecvBuffer, NetworkDefine.PACKETSIZE_BYTE_LENGTH, (System.UInt16)(packetSize - NetworkDefine.PACKETSIZE_BYTE_LENGTH), null, 0);
                    }

                    if (!LNetwork.MakePacketForLChocolate(ref m_stRecvBuffer, ref packet))
                    {
                        UnityEngine.Debug.Log("LSelectClient::GetPacket !LNetwork.MakePacket");
                        return false;
                    }
                    //ST_Packet.ConvertByteArrayToPacket(ref m_stRecvBuffer, ref packet);

                    if (m_uiRecvBufferAccumulateSize == packetSize)
                    {
                        System.Array.Clear(m_stRecvBuffer, 0, m_stRecvBuffer.Length);
                        m_uiRecvBufferAccumulateSize = 0;
                    }
                    else
                    {
                        System.Array oldBuffer = m_stRecvBuffer;
                        System.Buffer.BlockCopy(oldBuffer, packetSize, m_stRecvBuffer, 0, (System.Int32)(m_uiRecvBufferAccumulateSize - packetSize));
                        m_uiRecvBufferAccumulateSize -= packetSize;
                    }
                }
            }
            
            return true;
        }
    }
}