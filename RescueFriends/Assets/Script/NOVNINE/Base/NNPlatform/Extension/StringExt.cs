using UnityEngine;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace NOVNINE
{
public static class StringExt
{
    public static string Base64Encode(this string src, System.Text.Encoding enc)
    {
        byte[] arr = enc.GetBytes(src);
        return Convert.ToBase64String(arr);
    }

    public static string Base64Decode(this string src, System.Text.Encoding enc)
    {
        byte[] arr = Convert.FromBase64String(src);
        return enc.GetString(arr);
    }

    //From (http://stackoverflow.com/questions/6309379/how-to-check-for-a-valid-base-64-encoded-string-in-c-sharp)
    public static bool IsBase64String(string s)
    {
        s = s.Trim();
        return (s.Length % 4 == 0) && Regex.IsMatch(s, @"^[a-zA-Z0-9\+/]*={0,3}$", RegexOptions.None);
    }

    public static Dictionary<string,string> parseQueryString(this string src)
    {
        string[] tok = src.Split('&');
        Dictionary<string,string> dic = new Dictionary<string,string>();
        for(int i=0; i<tok.Length; i+=2)
            dic[tok[i]] = tok[i+1];
        return dic;
    }

    public static bool Like(this string str, string wildcard)
    {
        return new Regex(
                   "^" + Regex.Escape(wildcard).Replace(@"\*", ".*").Replace(@"\?", ".") + "$",
                   RegexOptions.IgnoreCase | RegexOptions.Singleline
               ).IsMatch(str);
    }

    /*
            static char DirSep = Path.DirectorySeparatorChar;
            public static string PathHead(this string path)
            {
                if (path.StartsWith("" + DirSep + DirSep))
                    return path.Substring(0, 2) + path.Substring(2).Split(DirSep)[0] + DirSep + path.Substring(2).Split(DirSep)[1];

                return path.Split(DirSep)[0];
            }

            public static string PathTail(this string path)
            {
                if (!path.Contains(DirSep))
                    return path;

                return path.Substring(1 + PathHead(path).Length);
            }
    */
}
}

