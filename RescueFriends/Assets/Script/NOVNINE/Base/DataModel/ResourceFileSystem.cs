/**
* @file ResourceFileSystem.cs
* @brief
* @author amugana (amugana@bitmango.com)
* @version 1.0
* @date 2013-03-15
*/

using UnityEngine;
using System.IO;
using System.Text;
using JsonFx.Json;
using System.Security.Cryptography;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class ResourceFileSystem : IJsonFileSystem
{
    public static string subPath;
	

    private static string _saltForKey;

    private static byte[] _keys;
    private static byte[] _iv;
    private static int keySize = 256;
    private static int blockSize = 128;
    private static int _hashLen = 32;


	public ResourceFileSystem()
	{
		subPath = Root.GetPostfix();    // PlayerPrefs.GetString("DataSet");

        // 8 바이트로 하고, 변경해서 쓸것
        byte[] saltBytes = new byte[] { 25, 36, 77, 51, 43, 14, 75, 93 };

        // 길이 상관 없고, 키를 만들기 위한 용도로 씀
        string randomSeedForKey = "5b6fcb4aaa0a42acae649eba45a506ec";

        // 길이 상관 없고, aes에 쓸 key 와 iv 를 만들 용도
        string randomSeedForValue = "2e327725789841b5bb5c706d6b2ad897";

        {
            Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(randomSeedForKey, saltBytes, 1000);
            _saltForKey = System.Convert.ToBase64String(key.GetBytes(blockSize / 8));
        }

        {
            Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(randomSeedForValue, saltBytes, 1000);
            _keys = key.GetBytes(keySize / 8);
            _iv = key.GetBytes(blockSize / 8);
        }
	}
	
	public string LoadContext(string key)
    {
        //if(PlayerPrefs.HasKey(key))
            //Debug.Log("ResourceFileSystem.LoadContext "+key+" : " +PlayerPrefs.GetString(key));

        //return 
        return PlayerPrefs.GetString(key);
    }

    public void SaveContext(string key, string content)
    {
		if(string.IsNullOrEmpty(content))
			return;
		
        //SetSecurityValue(key,content)
        PlayerPrefs.SetString(key, content);
    }

    public string Read(string path)
    {
#if !UNITY_WEBPLAYER
        //Debug.Log("ResourceFileSystem.Read : "+ Application.dataPath+"/Data/text/"+subPath+"/"+path+".txt");
        //return File.ReadAllText(Application.dataPath+"/Data/text/"+subPath+"/"+path+".txt");//, content);
        TextAsset txtAsset      = Resources.Load("data/"+subPath+"/"+path) as TextAsset;
        return txtAsset.text;
#else
        Debug.LogError("WEB Player doesn't supported");
        return null;
#endif
    }

    public void Write(string path, string content)
    {
#if UNITY_EDITOR
        //Debug.Log("ResourceFileSystem.Write: "+path);

        string dir = Path.GetDirectoryName(Application.dataPath+"/Data/text/"+subPath+"/"+path+".txt");
        Directory.CreateDirectory(dir);
#if !UNITY_WEBPLAYER
        File.WriteAllText(Application.dataPath+"/Data/text/"+subPath+"/"+path+".txt", content);
#else
        Debug.LogError("WEB Player doesn't supported");
#endif

#else
        Debug.LogError("ResourceFileSystem.Write is not implemented while writing "+path);
#endif

    }

    string MakeHash(string original)
    {
        using (MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider())
        {
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(original);
            byte[] hashBytes = md5.ComputeHash(bytes);

            string hashToString = "";
            for (int i = 0; i < hashBytes.Length; ++i)
                hashToString += hashBytes[i].ToString("x2");

            return hashToString;
        }
    }

    byte[] Encrypt(byte[] bytesToBeEncrypted)
    {
        using (RijndaelManaged aes = new RijndaelManaged())
        {
            aes.KeySize = keySize;
            aes.BlockSize = blockSize;

            aes.Key = _keys;
            aes.IV = _iv;

            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using (ICryptoTransform ct = aes.CreateEncryptor())
            {
                return ct.TransformFinalBlock(bytesToBeEncrypted, 0, bytesToBeEncrypted.Length);
            }
        }
    }

    byte[] Decrypt(byte[] bytesToBeDecrypted)
    {
        using (RijndaelManaged aes = new RijndaelManaged())
        {
            aes.KeySize = keySize;
            aes.BlockSize = blockSize;

            aes.Key = _keys;
            aes.IV = _iv;

            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using (ICryptoTransform ct = aes.CreateDecryptor())
            {
                return ct.TransformFinalBlock(bytesToBeDecrypted, 0, bytesToBeDecrypted.Length);
            }
        }
    }

    string Encrypt(string input)
    {
        byte[] bytesToBeEncrypted = Encoding.UTF8.GetBytes(input);
        byte[] bytesEncrypted = Encrypt(bytesToBeEncrypted);

        return System.Convert.ToBase64String(bytesEncrypted);
    }

    string Decrypt(string input)
    {
        byte[] bytesToBeDecrypted = System.Convert.FromBase64String(input);
        byte[] bytesDecrypted = Decrypt(bytesToBeDecrypted);

        return Encoding.UTF8.GetString(bytesDecrypted);
    }

    private void SetSecurityValue(string key, string value)
    {
        string hideKey = MakeHash(key + _saltForKey);
        string encryptValue = Encrypt(value + MakeHash(value));

        PlayerPrefs.SetString(hideKey, encryptValue);
    }

    private string GetSecurityValue(string key)
    {
        string hideKey = MakeHash(key + _saltForKey);

        string encryptValue = PlayerPrefs.GetString(hideKey);
        if (true == string.IsNullOrEmpty(encryptValue))
            return string.Empty;

        string valueAndHash = Decrypt(encryptValue);
        if (_hashLen > valueAndHash.Length)
            return string.Empty;

        string savedValue = valueAndHash.Substring(0, valueAndHash.Length - _hashLen);
        string savedHash = valueAndHash.Substring(valueAndHash.Length - _hashLen);

        if (MakeHash(savedValue) != savedHash)
            return string.Empty;

        return savedValue;
    }

    public void DeleteKey(string key)
    {
        PlayerPrefs.DeleteKey(MakeHash(key + _saltForKey));
    }
}

