namespace LChocolate
{
    public class LString
    {

        public static System.Boolean ConvertStringToByteArray(ref System.String inString, out System.Byte[] outByteArray)
        {
            outByteArray = null;

            if (null == inString || 1 > inString.Length)
                return false;

            outByteArray = System.Text.Encoding.UTF8.GetBytes(inString);
            return true;
        }
        public static System.Boolean ConvertByteArrayToString(ref System.Byte[] inByteArray, out System.String outString)
        {
            outString = null;

            if (null == inByteArray)
                return false;

            outString = System.Text.Encoding.UTF8.GetString(inByteArray);
            return true;
        }
    }
}