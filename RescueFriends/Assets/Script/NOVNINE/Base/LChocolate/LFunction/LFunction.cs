namespace LChocolate
{
    public static class LFunction
    {
        public static System.Boolean IS_VALID_STRING(ref System.String kString)
        {
            return ((null != kString) && (0 < kString.Length));
        }
        public static T DeepCopy<T>(T tObject)
        {
            System.Runtime.Serialization.Formatters.Binary.BinaryFormatter kBinaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            using (System.IO.MemoryStream kMemoryStream = new System.IO.MemoryStream())
            {
                kBinaryFormatter.Serialize(kMemoryStream, tObject);
                kMemoryStream.Position = 0;
                T newObject = (T)kBinaryFormatter.Deserialize(kMemoryStream);

                return newObject;
            }
        }

        public static System.Boolean ConvertTStructureToByteArray<T>(ref T inTStructure, ref byte[] outByteArray)
        {
            if ((null == inTStructure) || (null == outByteArray))
                return false;
            // typeof(T);
            System.Int32 tStructureSize = System.Runtime.InteropServices.Marshal.SizeOf(inTStructure);
            System.IntPtr buffer = System.Runtime.InteropServices.Marshal.AllocHGlobal(tStructureSize);
            System.Runtime.InteropServices.Marshal.StructureToPtr(inTStructure, buffer, false);               // 할당된 구조체 객체의 주소를 구한다.
            System.Runtime.InteropServices.Marshal.Copy(buffer, outByteArray, 0, tStructureSize);           // 구조체 객체를 배열에 복사
            System.Runtime.InteropServices.Marshal.FreeHGlobal(buffer);                                     // 비관리 메모리 영역에 할당했던 메모리를 해제함

            return true;
        }

        public static System.Boolean ConvertByteArrayToTStructure<T>(ref byte[] inByteArray, ref T outTStructure)
        {
            if ((null == inByteArray) || (null == outTStructure))
                return false;
            /*
            if (BitConverter.IsLittleEndian)
            Array.Reverse(bytes);
            */
            
            System.IntPtr buffer = System.Runtime.InteropServices.Marshal.AllocHGlobal(inByteArray.Length); // 배열의 크기만큼 비관리 메모리 영역에 메모리를 할당한다.
            System.Runtime.InteropServices.Marshal.Copy(inByteArray, 0, buffer, inByteArray.Length); // 배열에 저장된 데이터를 위에서 할당한 메모리 영역에 복사한다.
            outTStructure = (T)System.Runtime.InteropServices.Marshal.PtrToStructure(buffer, typeof(T)); // 복사된 데이터를 구조체 객체로 변환한다.
            System.Runtime.InteropServices.Marshal.FreeHGlobal(buffer); // 비관리 메모리 영역에 할당했던 메모리를 해제함

            if (System.Runtime.InteropServices.Marshal.SizeOf(outTStructure) != inByteArray.Length)// (((PACKET_DATA)obj).TotalBytes != data.Length) // 구조체와 원래의 데이터의 크기 비교
            {
                return false; // 크기가 다르면 null 리턴
            }
            return true; // 구조체 리턴
        }

    }
}