//using UnityEngine;
using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Text;

namespace LChocolate
{
    public enum E_CLIENT_NETWORK_STATUS : byte
    {
        E_CLIENT_NETWORK_STATUS_NONE,

        E_CLIENT_NETWORK_STATUS_CONNECTED,

        E_CLIENT_NETWORK_STATUS_CONNECTING,
        E_CLIENT_NETWORK_STATUS_CONNECTING_FAIL,

        E_CLIENT_NETWORK_STATUS_RECONNECT_FAIL,


        E_CLIENT_NETWORK_STATUS_DISCONNECTED,
        E_CLIENT_NETWORK_STATUS_CLOSED,
    };

    static class NetworkDefine
    {
        public const System.Byte PACKETSIZE_BYTE_LENGTH = 2;
        public const System.Byte PACKET_HEADER_LENGTH = 4;
        public const System.UInt16 MAX_PACKET_SIZE = 16384;
        public const System.UInt16 MAX_PACKETDATA_SIZE = MAX_PACKET_SIZE - PACKET_HEADER_LENGTH;

        public const System.UInt16 MAX_CLIENT_TRY_CONNECT_COUNT = 5;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct ST_PacketHeader
    {
        private System.UInt16 usPacketSize;
        private System.UInt16 usPacketCommand;

        public void Init()
        {
            usPacketSize = 0;
            usPacketCommand = 0;
        }
        public System.UInt16 GetPacketSize()
        {
            return usPacketSize;
        }
        public System.UInt16 GetPacketCommand()
        {
            return usPacketCommand;
        }
        public void Set(System.UInt16 packetCommand, System.UInt16 packetDataSize)
        {
            usPacketCommand = packetCommand;
            usPacketSize = (System.UInt16)(packetDataSize + NetworkDefine.PACKET_HEADER_LENGTH);
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct ST_Packet
    {
        private ST_PacketHeader stPacketHeader;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = NetworkDefine.MAX_PACKETDATA_SIZE)]
        public System.Byte[] cData;

        public ST_Packet(ref ST_PacketHeader packetHeader, ref System.Byte[] data)
        {
            stPacketHeader = packetHeader;
            cData = data;
            // this.Set(ref packetHeader, ref data);
        }
        
		public void Init()
        {
            stPacketHeader.Init();
        }
        
		public void Set(ref ST_PacketHeader packetHeader, ref System.Byte[] data)
        {
            stPacketHeader = packetHeader;
            cData = data;
        }
        
		public void Set(System.UInt16 packetCommand, System.UInt16 packetDataSize, ref System.Byte[] data)
        {
            if ((1 > packetCommand) || (NetworkDefine.MAX_PACKETDATA_SIZE < packetDataSize) || (null == data))
            {
                return;
            }
            stPacketHeader.Set(packetCommand, packetDataSize);
            cData = data;
        }
        
		public System.UInt16 GetPacketSize()
        {
            return stPacketHeader.GetPacketSize();
        }
        
		public System.UInt16 GetPacketCommand()
        {
            return stPacketHeader.GetPacketCommand();
        }
        
		public System.Byte[] GetData()
        {
            return cData;
        }

        public static System.Boolean ConvertPacketToByteArray(ref ST_Packet packet, ref byte[] byteArray)
        {
            System.UInt16 packetSize = packet.GetPacketSize();
            System.IntPtr buffer = Marshal.AllocHGlobal(packetSize); // 비관리 메모리 영역에 구조체 크기만큼의 메모리를 할당한다.
            Marshal.StructureToPtr(packet, buffer, false); // 할당된 구조체 객체의 주소를 구한다.            
            Marshal.Copy(buffer, byteArray, 0, packetSize); // 구조체 객체를 배열에 복사
            Marshal.FreeHGlobal(buffer); // 비관리 메모리 영역에 할당했던 메모리를 해제함

            return true;
        }
        
		public static System.Boolean ConvertByteArrayToPacket(ref byte[] byteArray, ref ST_Packet outPacket)
        {
            /*
            if (BitConverter.IsLittleEndian)
            Array.Reverse(bytes);
            */
            /*
            if (null == outPacket)
                return false;
            */
            IntPtr buffer = Marshal.AllocHGlobal(byteArray.Length); // 배열의 크기만큼 비관리 메모리 영역에 메모리를 할당한다.
            Marshal.Copy(byteArray, 0, buffer, byteArray.Length); // 배열에 저장된 데이터를 위에서 할당한 메모리 영역에 복사한다.
            outPacket = (ST_Packet)Marshal.PtrToStructure(buffer, typeof(ST_Packet)); // 복사된 데이터를 구조체 객체로 변환한다.
            Marshal.FreeHGlobal(buffer); // 비관리 메모리 영역에 할당했던 메모리를 해제함

            if (Marshal.SizeOf(outPacket) != byteArray.Length)// (((PACKET_DATA)obj).TotalBytes != data.Length) // 구조체와 원래의 데이터의 크기 비교
            {
                return false; // 크기가 다르면 null 리턴
            }
            return true; // 구조체 리턴
        }
        /*
        public byte[] Serialize()
        {
            // allocate a byte array for the struct data
            var buffer = new byte[Marshal.SizeOf(typeof(DataPacket))];

            // Allocate a GCHandle and get the array pointer
            var gch = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            var pBuffer = gch.AddrOfPinnedObject();

            // copy data from struct to array and unpin the gc pointer
            Marshal.StructureToPtr(this, pBuffer, false);
            gch.Free();

            return buffer;
        }

        // this method will deserialize a byte array into the struct.
        public void Deserialize(ref byte[] data)
        {
            var gch = GCHandle.Alloc(data, GCHandleType.Pinned);
            this = (DataPacket)Marshal.PtrToStructure(gch.AddrOfPinnedObject(), typeof(DataPacket));
            gch.Free();
        }
        */
    }

    public class LNetwork// : MonoBehaviour
    {
        public static byte[] MakePacketForWebSocket(System.UInt16 packetCommand, System.String kJsonDataString)
        {
            if ((1 > packetCommand) || (null == kJsonDataString) || (1 > kJsonDataString.Length))
            {
                UnityEngine.Debug.Log("LNetwork::MakePacket Error");
                return null;
            }

            byte[] jsonDataBytes = Encoding.UTF8.GetBytes(kJsonDataString);
            System.Int32 packetDataSize = jsonDataBytes.Length + 1;                         // +1 은 c++ 에서 string NULL 때문에 예외처리

            byte[] packetByteArray = new byte[packetDataSize + 2];

            BitConverter.GetBytes(packetCommand).CopyTo(packetByteArray, 0);
            Array.Copy(jsonDataBytes, 0, packetByteArray, 2, packetDataSize - 1);
            packetByteArray[packetDataSize + 2 - 1] = 0;   // c++ 에서 string NULL 임

            return packetByteArray;
        }
		
        public static System.Boolean MakePacketForWebSocket(ref byte[] byteArray, ref ST_Packet outPacket)
        {
            if (null == byteArray)
                return false;

            System.UInt16 packetSize = (System.UInt16)byteArray.Length;
            System.UInt16 packetCommand = System.BitConverter.ToUInt16(byteArray, 0);
            System.UInt16 packetDataSize = (System.UInt16)(packetSize - 2 - 1);        // -1 은 C++ 에서 string NULL 때문인데, C# 에서는 String 끝에 NULL 안씀

            byte[] packetData_ByteArray = new byte[packetDataSize];
            Array.Copy(byteArray, 2, packetData_ByteArray, 0, packetDataSize);

            outPacket.Set(packetCommand, packetDataSize, ref packetData_ByteArray);

            return true;
        }

        public static byte[] MakePacketForLChocolate(System.UInt16 packetCommand, System.String kJsonDataString)
        {
            if ((1 > packetCommand) || (null == kJsonDataString) || (1 > kJsonDataString.Length))
            {
                UnityEngine.Debug.Log("LNetwork::MakePacket Error");
                return null;
            }

            byte[] jsonDataBytes = Encoding.UTF8.GetBytes(kJsonDataString);
            System.Int32 packetDataSize = jsonDataBytes.Length + 1;                         // +1 은 c++ 에서 string NULL 때문에 예외처리
            System.Int32 packetSize = (System.UInt16)(packetDataSize + NetworkDefine.PACKET_HEADER_LENGTH);

            byte[] packetByteArray = new byte[packetDataSize + NetworkDefine.PACKET_HEADER_LENGTH];

            BitConverter.GetBytes(packetSize).CopyTo(packetByteArray, 0);
            BitConverter.GetBytes(packetCommand).CopyTo(packetByteArray, 2);
            Array.Copy(jsonDataBytes, 0, packetByteArray, NetworkDefine.PACKET_HEADER_LENGTH, packetDataSize - 1);
            packetByteArray[packetDataSize + NetworkDefine.PACKET_HEADER_LENGTH - 1] = 0;   // c++ 에서 string NULL 임

            return packetByteArray;
        }
        public static System.Boolean MakePacketForLChocolate(ref byte[] byteArray, ref ST_Packet outPacket)
        {
            if (null == byteArray)
                return false;

            System.UInt16 packetSize = System.BitConverter.ToUInt16(byteArray, 0);
            System.UInt16 packetCommand = System.BitConverter.ToUInt16(byteArray, 2);
            System.UInt16 packetDataSize = (System.UInt16)(packetSize - NetworkDefine.PACKET_HEADER_LENGTH - 1);        // -1 은 C++ 에서 string NULL 때문인데, C# 에서는 String 끝에 NULL 안씀

            byte[] packetData_ByteArray = new byte[packetDataSize];
            Array.Copy(byteArray, NetworkDefine.PACKET_HEADER_LENGTH, packetData_ByteArray, 0, packetDataSize);

            outPacket.Set(packetCommand, packetDataSize, ref packetData_ByteArray);

            return true;
        }
		
		public static LitJson.JsonData GetJsonData(ref LChocolate.ST_Packet packet)
		{
			byte[] pPacketData = packet.GetData();
			System.String jsonString = Encoding.UTF8.GetString(pPacketData);

            return LitJson.JsonMapper.ToObject(jsonString);

			/*
            LitJson.JsonReader reader = new LitJson.JsonReader(jsonString);
            LitJson.JsonData kJsonData = LitJson.JsonMapper.ToObject(reader);

            return kJsonData;
            */
		}
		
        public static byte[] MakePacket<T>(System.UInt16 packetCommand, System.UInt16 packetDataSize, ref T tStructure)
        {
            if ((1 > packetCommand) || (NetworkDefine.MAX_PACKETDATA_SIZE < packetDataSize) || (null == tStructure))
            {
                UnityEngine.Debug.Log("LNetwork::MakePacket Error");
                return null;
            }

            System.Int32 packetSize = (System.UInt16)(packetDataSize + NetworkDefine.PACKET_HEADER_LENGTH);

            System.IntPtr buffer = Marshal.AllocHGlobal(packetDataSize); // 비관리 메모리 영역에 구조체 크기만큼의 메모리를 할당한다.
            byte[] packetByteArray = new byte[packetDataSize + NetworkDefine.PACKET_HEADER_LENGTH];

            BitConverter.GetBytes(packetSize).CopyTo(packetByteArray, 0);
            BitConverter.GetBytes(packetCommand).CopyTo(packetByteArray, 2);

            Marshal.StructureToPtr(tStructure, buffer, false); // 할당된 구조체 객체의 주소를 구한다.            
            Marshal.Copy(buffer, packetByteArray, NetworkDefine.PACKET_HEADER_LENGTH, packetDataSize); // 구조체 객체를 배열에 복사
            Marshal.FreeHGlobal(buffer); // 비관리 메모리 영역에 할당했던 메모리를 해제함

            return packetByteArray;
        }

        public static System.Boolean MakeTStructure<T>(System.Byte[] byteArray, ref T outTStructure)
        {
            if ((null == byteArray) || (null == outTStructure))
                return false;
            /*
            if (BitConverter.IsLittleEndian)
            Array.Reverse(bytes);
            */
            /*
            if (null == outPacket)
                return false;
            */
            IntPtr buffer = Marshal.AllocHGlobal(byteArray.Length); // 배열의 크기만큼 비관리 메모리 영역에 메모리를 할당한다.
            Marshal.Copy(byteArray, 0, buffer, byteArray.Length); // 배열에 저장된 데이터를 위에서 할당한 메모리 영역에 복사한다.
            outTStructure = (T)Marshal.PtrToStructure(buffer, typeof(T)); // 복사된 데이터를 구조체 객체로 변환한다.
            Marshal.FreeHGlobal(buffer); // 비관리 메모리 영역에 할당했던 메모리를 해제함

            /*
            if (Marshal.SizeOf(outTStructure) != byteArray.Length)// (((PACKET_DATA)obj).TotalBytes != data.Length) // 구조체와 원래의 데이터의 크기 비교
            {
                return false; // 크기가 다르면 null 리턴
            }
            */
            return true; // 구조체 리턴
        }
    }
}