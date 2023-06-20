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
            System.Runtime.InteropServices.Marshal.StructureToPtr(inTStructure, buffer, false);               // �Ҵ�� ����ü ��ü�� �ּҸ� ���Ѵ�.
            System.Runtime.InteropServices.Marshal.Copy(buffer, outByteArray, 0, tStructureSize);           // ����ü ��ü�� �迭�� ����
            System.Runtime.InteropServices.Marshal.FreeHGlobal(buffer);                                     // ����� �޸� ������ �Ҵ��ߴ� �޸𸮸� ������

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
            
            System.IntPtr buffer = System.Runtime.InteropServices.Marshal.AllocHGlobal(inByteArray.Length); // �迭�� ũ�⸸ŭ ����� �޸� ������ �޸𸮸� �Ҵ��Ѵ�.
            System.Runtime.InteropServices.Marshal.Copy(inByteArray, 0, buffer, inByteArray.Length); // �迭�� ����� �����͸� ������ �Ҵ��� �޸� ������ �����Ѵ�.
            outTStructure = (T)System.Runtime.InteropServices.Marshal.PtrToStructure(buffer, typeof(T)); // ����� �����͸� ����ü ��ü�� ��ȯ�Ѵ�.
            System.Runtime.InteropServices.Marshal.FreeHGlobal(buffer); // ����� �޸� ������ �Ҵ��ߴ� �޸𸮸� ������

            if (System.Runtime.InteropServices.Marshal.SizeOf(outTStructure) != inByteArray.Length)// (((PACKET_DATA)obj).TotalBytes != data.Length) // ����ü�� ������ �������� ũ�� ��
            {
                return false; // ũ�Ⱑ �ٸ��� null ����
            }
            return true; // ����ü ����
        }

    }
}