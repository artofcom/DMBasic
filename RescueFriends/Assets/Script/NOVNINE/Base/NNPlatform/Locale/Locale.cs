using UnityEngine;
using System.Collections.Generic;
using System.IO;
using ReadWriteCsv;

public enum LocaleType {
    EN,
	SE,
	DK,
	NO,
	DE,
    MAX_LOCALE
}

public static class Locale
{
    private static LocaleType _current = LocaleType.MAX_LOCALE;
    public static LocaleType current
    {
        get {
            if(_current == LocaleType.MAX_LOCALE) {
                LocaleType ltype = LocaleType.EN;
#if UNITY_EDITOR
                ltype = (LocaleType)PlayerPrefs.GetInt("locale");
#else
                switch(Application.systemLanguage) {
	                case SystemLanguage.Swedish:
	                    ltype = LocaleType.SE;
	                    break;
	                case SystemLanguage.Danish:
	                    ltype = LocaleType.DK;
	                    break;
					case SystemLanguage.Norwegian:
	                    ltype = LocaleType.NO;
	                    break;
	           		case SystemLanguage.German:
	                    ltype = LocaleType.DE;
	                    break;
                }
#endif

                _current = ltype;
                //if(tk2dSystem.CurrentPlatform != _current)
                //    tk2dSystem.CurrentPlatform = _current.ToString();
                //Debug.Log("set current locale to :"+_current.ToString());
            }
            return _current;
        }
    }

    public static string name
    {
        get {
            return current.ToString();
        }
    }

    private static Dictionary<string, string> stringTable;

    static Locale()
    {
        stringTable = new Dictionary<string, string>();
        //todo list
        //LoadStringTable("platformCommonText");
        //LoadStringTable("StringTable");
    }

	public static void ReloadLocale() {
		#if UNITY_EDITOR
		Reset ();
		stringTable = new Dictionary<string, string>();
        //todo
//		LoadStringTable("platformCommonText");
//		LoadStringTable("StringTable");
		#endif
	}

    private static bool LoadStringTable(string resName)
    {
        TextAsset txasset = Resources.Load(resName, typeof(TextAsset)) as TextAsset;
        if(txasset == null) {
            Debug.LogWarning("stringTable ["+resName+"] not found");
            return false;
        }

        using (CsvFileReader reader = new CsvFileReader(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(txasset.text))))
        {
            string localeName = Locale.name;
            CsvRow row = new CsvRow();

            reader.ReadRow(row);
            List<string> tableHeader = new List<string>(row);

            while( reader.ReadRow(row) )
                if(row[0] == localeName)
                    break;

            int i = 1;
            for(i=1; i<row.Count; ++i) 
            {
                stringTable.Add((string)tableHeader[i], row[i]);
            }

            if (tableHeader.Count != row.Count)
                Debug.LogError("Miss match Locale String Table : " + tableHeader[i]);
        }
        return true;
    }

    public static string GetString(this string key)
    {
        if(stringTable == null) {
            Debug.LogError("StringTable is null");
            return "<StringTable not found>";
        }
        if(key == null)
            return null;
        if(stringTable.ContainsKey(key)) return stringTable[key];
        else return key;
    }

#if UNITY_EDITOR
    public static void Reset()
    {
        _current = LocaleType.MAX_LOCALE;
    }
#endif

}

