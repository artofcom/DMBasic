using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using NOVNINE.Diagnostics;

namespace NOVNINE
{
public static class Encryptor
{
	static byte[] rijnKey = Encoding.UTF8.GetBytes("Nov91108Nov91108");
	static byte[] rijnIV = Encoding.UTF8.GetBytes("Nov91108Nov91108");

    public static string Encrypt(string s)
    {
        return Encrypt(s, rijnKey, rijnIV);
    }
    public static string Decrypt(string s)
    {
        return Decrypt(s, rijnKey, rijnIV);
    }

    static string Encrypt(string s, byte[] key, byte[] IV)
    {
		string result;
        RijndaelManaged rijn = new RijndaelManaged();
        rijn.Mode = CipherMode.CBC;
        rijn.Padding = PaddingMode.Zeros;
        using (MemoryStream msEncrypt = new MemoryStream()) {
            using (ICryptoTransform encryptor = rijn.CreateEncryptor(key, IV)) {
                using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write)) {
                    using (StreamWriter swEncrypt = new StreamWriter(csEncrypt)) {
                        swEncrypt.Write(PKCS7_Pad(s));
                    }
                }
            }
            result = Convert.ToBase64String(msEncrypt.ToArray());
        }
        rijn.Clear();
        return result;
    }
		
	const int READ_BUFFER_SIZE = 1024;
    static char[] READ_BUFFER = new char[READ_BUFFER_SIZE];
		
    static string Decrypt(string s, byte[] key, byte[] IV)
    {
        string result = null;
        RijndaelManaged rijn = new RijndaelManaged();
        rijn.Mode = CipherMode.CBC;
        rijn.Padding = PaddingMode.Zeros;
		
        using (MemoryStream msDecrypt = new MemoryStream(Convert.FromBase64String(s))) {
            using (ICryptoTransform decryptor = rijn.CreateDecryptor(key, IV)) {
                using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read)) {
                    using (StreamReader swDecrypt = new StreamReader(csDecrypt)) {
						List<char> readbuf = new List<char>();
						int byteCount = 0;
						do {
							byteCount = swDecrypt.ReadBlock(READ_BUFFER, 0, READ_BUFFER_SIZE);
							if(byteCount == READ_BUFFER_SIZE)
								readbuf.AddRange(READ_BUFFER);
							else
								readbuf.AddRange(READ_BUFFER.Take(byteCount).ToArray());
						} while( byteCount == 1024 );
						result = PKCS7_UnPad(readbuf.ToArray());
                    }
                }
            }
        }
        rijn.Clear();
        return result;
    }
			
	static char[] PKCS7_Pad(string val){
		List<char> outval = new List<char>(val);
		int padSize = 16 - (val.Length%16);
		char[] appendVal = new char[padSize];	
		for (int i = 0; i < padSize; i++) {
			appendVal[i] = (char)padSize;
		}
		outval.AddRange(appendVal);
		return outval.ToArray();
	}

	static string PKCS7_UnPad(char[] val){
		Debugger.Assert(val.Length % 16 == 0);	
		List<char> outval = new List<char>(val);	
		
		int padSize = Convert.ToInt32(outval[outval.Count-1]);	
		return new string(outval.GetRange(0, outval.Count - padSize).ToArray());
	}	

    public static string EncodeWithKey(string text, string key)
    {
        byte[] result = new byte[text.Length];
        //var result = new StringBuilder();
        for (int c = 0; c < text.Length; c++)
            result[c] = (byte)((uint)text[c] ^ (uint)key[c % key.Length]);
        //result.Append((char)((uint)text[c] ^ (uint)key[c % key.Length]));
        return System.Convert.ToBase64String(result, 0, result.Length);
    }
}
}

