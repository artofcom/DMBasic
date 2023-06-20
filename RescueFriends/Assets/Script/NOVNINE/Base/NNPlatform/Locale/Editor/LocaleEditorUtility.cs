using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;

using SprColProxy = tk2dEditor.SpriteCollectionEditor.SpriteCollectionProxy;
using ReadWriteCsv;
//using NOVNINE.Store;

public static class LocaleEditorUtility
{
    struct SpriteTransform {
        public tk2dSpriteCollection srcCol;
        public int srcId;

        public tk2dSpriteCollection dstCol;
        public int dstId;
    }
    public static string CutLocale(this string str)
    {
        int ridx = str.LastIndexOf('_');
        return str.Substring(0, ridx);
    }
    [MenuItem("NOVNINE/LocaleSystem/Build", false, 3001)]
    public static void BuildLocaleSpriteCollection()
    {
        EditorUtility.DisplayProgressBar("Building Locale Spritecollections", "Preparing...", 0);

        List<tk2dSpriteCollection> sprColList = GetAllSpriteCollections();
        tk2dSpriteCollection[] localeCollections = GetLocaleSpriteCollections(ref sprColList);
        List<SpriteTransform> movedSprites = new List<SpriteTransform>();
        Dictionary<string, int> defSpriteID = new Dictionary<string, int>();

        // Check all textures that has locale postfix
        for(int i=0; i<(int)LocaleType.MAX_LOCALE; ++i) {
            string localeID = ((LocaleType)i).ToString();
            EditorUtility.DisplayProgressBar("Building Locale Spritecollections", "Building  "+localeID, i*0.15f);
            Debug.Log("Building "+localeID+"...");

            //Find Asset Named "*_[LocaleID].png"
            List<Texture2D> localeTexList = GetLocaleTextures(localeID);
            if(localeTexList.Count == 0) continue;

            //Build new Spritecollection if no collection exist
            if(localeCollections[i] == null && localeTexList.Count > 0) {
                if(!Directory.Exists(Application.dataPath+"/Resources"))
                    Directory.CreateDirectory(Application.dataPath+"/Resources");

                //code from tk2dSpriteCollectionEditor.cs 29-44
                string path = "Assets/Resources/locale"+localeID+".prefab";
                CreateEmptySpriteCollection(path);
                localeCollections[i] = AssetDatabase.LoadAssetAtPath(path, typeof(tk2dSpriteCollection)) as tk2dSpriteCollection;
            }

            SprColProxy proxy = new SprColProxy(localeCollections[i]);
            HashSet<tk2dSpriteCollection> commitList = new HashSet<tk2dSpriteCollection>();

            //Find out tk2dSpriteCollection contains our locale Texture
            foreach(Texture2D tex in localeTexList) {
                Debug.Log("searching "+tex.name);
                bool collectionFound = false;
                foreach(tk2dSpriteCollection sprCol in sprColList) {
                    if(localeCollections.Contains(sprCol)) continue;
                    int orgIndex = System.Array.IndexOf(sprCol.DoNotUse__TextureRefs, tex);
                    if(orgIndex != -1) {
                        //Move to locale SpriteCollection
                        Debug.Log("Moving tex "+tex.name+" from "+sprCol.name+" to "+localeCollections[i].name);
                        int slot = 0;
                        if(i==0) { //first locale
                            slot = GetSlotForTex(proxy, tex);//proxy.FindOrCreateEmptySpriteSlot();
                            defSpriteID[tex.name.CutLocale()] = slot;
                        } else {
                            if(defSpriteID.ContainsKey(tex.name.CutLocale()))
                                slot = defSpriteID[tex.name.CutLocale()];
                            else {
                                Debug.LogError("Unknown SpriteID Found for "+tex.name+".... Skipping");
                                continue;
                            }
                        }

                        SprColProxy srcProxy = new SprColProxy(sprCol);
                        AddTextureToSpriteCollection(tex, srcProxy.textureParams[orgIndex], proxy, slot);
                        RemoveTextureFromSpriteCollection(srcProxy, orgIndex);
                        srcProxy.CopyToTarget();

                        //commitList.Add(sprCol);
                        collectionFound = true;

                        SpriteTransform tr;
                        tr.srcCol = sprCol;
                        tr.srcId = orgIndex;
                        tr.dstCol = localeCollections[i];
                        tr.dstId = slot;
                        movedSprites.Add(tr);
                        break;
                    }
                }

                if(!collectionFound) {
                    Debug.Log("Adding tex "+tex.name+" to "+localeCollections[i].name);
                    int slot = 0;
                    if(i==0) {
                        slot = GetSlotForTex(proxy, tex);//proxy.FindOrCreateEmptySpriteSlot();
                        defSpriteID[tex.name.CutLocale()] = slot;
                        AddTextureToSpriteCollection(tex, null, proxy, slot);
                    } else {
                        if(defSpriteID.ContainsKey(tex.name.CutLocale()))
                            slot = defSpriteID[tex.name.CutLocale()];
                        else {
                            Debug.LogWarning(localeID +" : Translation Texture Not Found For "+tex.name);
                            slot = GetSlotForTex(proxy, tex);//proxy.FindOrCreateEmptySpriteSlot();
                            //break;
                        }
                        AddTextureToSpriteCollection(tex, localeCollections[0].textureParams[slot], proxy, slot);
                    }
                }
            }
            proxy.CopyToTarget();
            commitList.Add(localeCollections[i]);

            //commit all spriteCollections
            foreach(tk2dSpriteCollection col in commitList) {
                tk2dSpriteCollectionBuilder.Rebuild(col);
            }
        }

        AssetDatabase.SaveAssets();

        float prgress=0.7f;

        object[] obj = GameObject.FindObjectsOfType(typeof (tk2dSprite));
        foreach (object o in obj) {
            EditorUtility.DisplayProgressBar("Building Locale Spritecollections", "Converting tk2dSprite to tk2dLocaleSprite", prgress+=0.05f);
            tk2dSprite g = (tk2dSprite) o;
            //Debug.Log("For Tex : "+g.name);

            foreach(SpriteTransform tr in movedSprites) {
                if(tr.srcCol.spriteCollection == g.Collection && tr.srcId == g.spriteId) {
                    Debug.Log("SwitchCollectionAndSprite from "+tr.srcCol.name+ ":"+tr.srcId+"  to "+tr.dstCol.name+" :"+tr.dstId);
                    g.SetSprite(tr.dstCol.spriteCollection, tr.dstId);
                    //g.renderer.sharedMaterial = null;
                }
            }

            if(g.Collection != null && g.Collection.spriteCollectionName.StartsWith("locale")) {
                tk2dLocaleSprite locSpr = g.gameObject.AddComponent<tk2dLocaleSprite>();
                foreach (FieldInfo f in g.GetType().GetFields())
                    f.SetValue(locSpr, f.GetValue(g));
                locSpr.Build();
                locSpr.SetSprite(g.Collection, g.spriteId);
                Object.DestroyImmediate(g);
            }
        }

        EditorUtility.ClearProgressBar();
    }

    [MenuItem("NOVNINE/LocaleSystem/ImportFromCSV", false, 3002)]
    public static void ImportFromCSV()
    {
        string path = EditorUtility.OpenFilePanel("Import Platform String from CSV...", "", "csv");
        if(path.Length != 0) {
            string text = System.IO.File.ReadAllText(path);
            LoadPlatformInfoFromCSV(text);
        }
    }
	
	[MenuItem("NOVNINE/LocaleSystem/ImportFromCSVToXML", false, 3002)]
	public static void ImportFromCSVToXML()
	{
		string path = EditorUtility.OpenFilePanel("Import Platform String from CSV...", "", "csv");
		if(path.Length != 0) 
		{
			string text = System.IO.File.ReadAllText(path);
			XmlDocument xml = LoadPlatformInfoFromCSVToXML(text);

			string fullpath = path.Substring(0, path.Length - 3) + "xml";
			xml.Save(fullpath);
		}
	}

    static CsvRow AddLocaleString(string id, LocaleString str)
    {
        CsvRow row = new CsvRow();
        row.Add(id);
        row.Add(str.EN);
		row.Add(str.SE);
		row.Add(str.DK);
		row.Add(str.NO);
		row.Add(str.DE);
        return row;
    }

    [MenuItem("NOVNINE/LocaleSystem/ExportToCSV", false, 3003)]
    public static void ExportToCSV()
    {
        string path = EditorUtility.SaveFilePanel("Export Platform String to CSV...", "", "platformGameText.csv", "csv");
        if(path.Length != 0) {
            using (CsvFileWriter writer = new CsvFileWriter(path)) {
                CsvRow header = new CsvRow();
                header.Add("platformInfo");
                for(int i=0; i<((int)LocaleType.MAX_LOCALE); ++i)
                    header.Add(((LocaleType)i).ToString());
                writer.WriteRow(header);

//                PlatformContext info = ModuleManager.LoadModuleHolder().GetComponent<PlatformContext>();
//                writer.WriteRow(AddLocaleString("appName", info.appName));

//#if USE_UncleBill
//                //UncleBillContext ubCntx = info.GetComponent<UncleBillContext>();
//                //writer.WriteRow(AddLocaleString("currency", ubCntx.currency));
//                /*
//                foreach(NOVNINE.InventoryItem item in ubCntx.inventoryItems)
//                {
//                    writer.WriteRow(AddLocaleString(item.id+".inven.name", item.name));
//                    writer.WriteRow(AddLocaleString(item.id+".inven.description", item.description));
//                }
//                foreach(NOVNINE.ShopItem item in ubCntx.shopItems)
//                {
//                    writer.WriteRow(AddLocaleString(item.id+".shop.name", item.name));
//                    writer.WriteRow(AddLocaleString(item.id+".shop.description", item.description));
//                }
//                */
//#endif
            }
        }
    }

	static XmlDocument LoadPlatformInfoFromCSVToXML(string text)
	{
		Debug.Log("LoadPlatformInfoFromCSV");
		//        PlatformContext info = ModuleManager.LoadModuleHolder().GetComponent<PlatformContext>();
		Dictionary<int, List<string>> dict = new Dictionary<int, List<string>>();

		using (CsvFileReader reader = new CsvFileReader(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(text))))
		{
			CsvRow header = new CsvRow();
			reader.ReadRow(header);
			XmlDocument xml = new XmlDocument();
			XmlNode docNode = xml.CreateXmlDeclaration("1.0", "UTF-8", null);
			xml.AppendChild(docNode);
			XmlNode rootNode = xml.CreateElement("root");
			
			CsvRow row = new CsvRow();
			
			while(reader.ReadRow(row))
			{
				XmlNode temp = xml.CreateElement("item");
				
				for(int i = 0; i < header.Count; ++i)
				{
					XmlAttribute rootAttrib = xml.CreateAttribute(header[i]);
					rootAttrib.Value = row[i];
					temp.Attributes.Append(rootAttrib);
				}

				rootNode.AppendChild(temp);
			}
			
			xml.AppendChild(rootNode);
			return xml;
		}
		
		return null;
	}
	
    static void LoadPlatformInfoFromCSV(string text)
    {
        Debug.Log("LoadPlatformInfoFromCSV");
//        PlatformContext info = ModuleManager.LoadModuleHolder().GetComponent<PlatformContext>();
        Dictionary<string, LocaleString> dict = new Dictionary<string, LocaleString>();

        using (CsvFileReader reader = new CsvFileReader(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(text)))) {
            CsvRow header = new CsvRow();
            reader.ReadRow(header);

            CsvRow row = new CsvRow();
            while( reader.ReadRow(row) ) {
                string id = row[0];
                try {
                    dict.Add(id, new LocaleString());
                } catch {
                    EditorUtility.DisplayDialog("CSV Import Fail", "SameKey \""+id+"\" Already Exists", "Ok");
                    throw;
                }

                for(int i=1; i<row.Count; ++i)
                    dict[id].Set(header[i], row[i]);
            }

//            info.appName = dict["appName"];

//#if USE_UncleBill
//            //UncleBillContext ubCntx = info.GetComponent<UncleBillContext>();
//            //ubCntx.currency = dict["currency"];
//            /*
//            foreach(NOVNINE.InventoryItem item in ubCntx.inventoryItems)
//            {
//                if(!dict.ContainsKey(item.id+".inven.name"))
//                {
//                    Debug.LogError("Key Not Found : "+item.id+".inven.name");
//                    continue;
//                }
//                item.name = dict[item.id+".inven.name"];
//                item.description = dict[item.id+".inven.description"];
//            }
//
//            foreach(NOVNINE.ShopItem item in ubCntx.shopItems)
//            {
//                if(!dict.ContainsKey(item.id+".shop.name"))
//                {
//                    Debug.LogError("Key Not Found : "+item.id+".shop.name");
//                    continue;
//                }
//                item.name = dict[item.id+".shop.name"];
//                item.description = dict[item.id+".shop.description"];
//            }
//            */
//#endif
        }
//        ModuleManager.SaveModuleHolder(info.gameObject);
    }

    static void SaveStringTableFromCSV(string text)
    {
        Debug.Log("Saving StringTable.txt");
        string path = Application.dataPath+"/Resources/StringTable.txt";
        System.IO.File.WriteAllText(path, text);
    }

    [MenuItem("NOVNINE/LocaleSystem/ImportFromGoogleDrive", false, 3004)]
    public static void ImportFromGoogleDrive()
    {
//        PlatformContext info = ModuleManager.LoadModuleHolder().GetComponent<PlatformContext>();
//        if(string.IsNullOrEmpty(info.GoogleDriveKey)) {
//            EditorUtility.DisplayDialog("GoogleDrive Key Fail", "GoogleDriveKey is not Set on PlatformInfo", "Ok");
//            return;
//        }
//        string URI = "https://docs.google.com/spreadsheet/pub?key="+info.GoogleDriveKey+"&output=csv&single=true";
//
//        float prog = 0;
//
//        for(int i=0; i<20; ++i) {
//            WWW www = new WWW(URI+"&gid="+i);
//
//            while(!www.isDone);
//            if(EditorUtility.DisplayCancelableProgressBar(
//                        "Downloading GoogleDrive Locale Sheets..."+i,
//                        (prog*100).ToString()+"%",
//                        prog)) {
//
//                EditorUtility.ClearProgressBar();
//                return;
//            }
//
//            if(string.IsNullOrEmpty(www.error)) {
//                string text = www.text;
//                if(text.StartsWith("message")) {
//                    SaveStringTableFromCSV(text);
//                } else if(text.StartsWith("platformInfo")) {
//                    LoadPlatformInfoFromCSV(text);
//                }
//            }
//            prog+=0.05f;
//        }
//
//        EditorUtility.ClearProgressBar();
    }

    static List<tk2dSpriteCollection> GetAllSpriteCollections()
    {
        //Get All SpriteCollections
        if(tk2dEditorUtility.GetExistingIndex() == null) {
            tk2dEditorUtility.RebuildIndex();
        }
        List<tk2dSpriteCollection> sprColList = new List<tk2dSpriteCollection>();
        foreach(tk2dSpriteCollectionIndex spcolIdx in
                tk2dEditorUtility.GetExistingIndex().GetSpriteCollectionIndex()) {
            tk2dSpriteCollection col;
            string assetPath = AssetDatabase.GUIDToAssetPath(spcolIdx.spriteCollectionGUID);
            col = AssetDatabase.LoadAssetAtPath(assetPath, typeof(tk2dSpriteCollection)) as tk2dSpriteCollection;
            sprColList.Add(col);
        }
        return sprColList;
    }

    static tk2dSpriteCollection[] GetLocaleSpriteCollections(ref List<tk2dSpriteCollection> sprColList)
    {
        // Get pre-exsting Locale SpriteCollections
        tk2dSpriteCollection[] localeCollections = new tk2dSpriteCollection[(int)LocaleType.MAX_LOCALE];
        for(int i=0; i<(int)LocaleType.MAX_LOCALE; ++i) {
            string localeID = ((LocaleType)i).ToString();
            localeCollections[i] = sprColList.Find(c => c.name == "locale"+localeID);
            if(localeCollections[i] != null)
                Debug.Log("found Locale "+localeID);
        }
        return localeCollections;
    }

    static List<Texture2D> GetLocaleTextures(string localeID)
    {
        //Find Asset Named "*_[LocaleID].png"
        List<Texture2D> localeTexList = new List<Texture2D>();
        string[] files = Directory.GetFiles(Application.dataPath,
                                            "*_"+localeID.ToLower()+".png", SearchOption.AllDirectories);
        foreach (string file in files) {
            string reducedPath = file.Substring(Application.dataPath.Length - 6);
            Texture2D tex = AssetDatabase.LoadAssetAtPath(reducedPath, typeof(Texture2D)) as Texture2D;
            localeTexList.Add(tex);
        }
        return localeTexList;
    }

    static Object CreateEmptySpriteCollection(string path)
    {
        Debug.Log("Creating New Locale SpriteCollection in "+path+"  "+Application.dataPath);
        GameObject go = new GameObject();
        go.AddComponent<tk2dSpriteCollection>();
        go.SetActive(false);
        Object p = PrefabUtility.CreateEmptyPrefab(path);
        PrefabUtility.ReplacePrefab(go, p, ReplacePrefabOptions.ConnectToPrefab);
        GameObject.DestroyImmediate(go);
        return p;
    }

    static bool AddTextureToSpriteCollection(Texture2D tex, tk2dSpriteCollectionDefinition def, SprColProxy dst, int slot)
    {
        if(tex.name.CutLocale() != dst.FindUniqueTextureName(tex.name.CutLocale()))
            return false;//already has one

        if(slot == -1)
            slot = GetSlotForTex(dst, tex);//dst.FindOrCreateEmptySpriteSlot();

        while(dst.spriteSheets.Count <= slot) {
            dst.spriteSheets.Add(new tk2dSpriteSheetSource());
            dst.textureParams.Add(new tk2dSpriteCollectionDefinition());
        }

        if(def != null)
            dst.textureParams[slot].CopyFrom( def );
        else
            dst.textureParams[slot] = new tk2dSpriteCollectionDefinition();

        dst.textureParams[slot].name = tex.name.CutLocale();

        var spriteSheet = new tk2dSpriteSheetSource();
        spriteSheet.active = true;
        spriteSheet.version = tk2dSpriteSheetSource.CURRENT_VERSION;
        spriteSheet.texture = tex;
        dst.spriteSheets[slot] = spriteSheet;
        return true;
    }

    static void RemoveTextureFromSpriteCollection(SprColProxy col, int slot)
    {
        col.spriteSheets[slot] = new tk2dSpriteSheetSource();
        col.textureParams[slot] = new tk2dSpriteCollectionDefinition();
        col.Trim();
    }

    static int GetSlotForTex(SprColProxy col, Texture2D tex)
    {
        int idx = col.spriteSheets.FindIndex(x=>x.texture == tex);
        //int idx = System.Array.IndexOf(col.spriteSheets.ToArray(), tex);
        if(idx >= 0)
            return idx;
        else
            return col.FindOrCreateEmptySpriteSlot();
    }

    [MenuItem("NOVNINE/LocaleSystem/SetLocale/EN", false, 3101)]
    static void SetLocaleEN()
    {
        PlayerPrefs.SetInt("locale", (int)LocaleType.EN);

        Locale.Reset();
        string newPlatform = "EN";
        tk2dPreferences prefs = tk2dPreferences.inst;
        prefs.platform = newPlatform;
        UnityEditor.EditorPrefs.SetString("tk2d_platform", newPlatform);
        tk2dSystem.CurrentPlatform = prefs.platform; // mirror to where it matters
        tk2dSystemUtility.PlatformChanged(); // tell the editor things have changed
        tk2dEditorUtility.UnloadUnusedAssets();
    }

	[MenuItem("NOVNINE/LocaleSystem/SetLocale/SE", false, 3102)]
	static void SetLocaleSE()
    {
		PlayerPrefs.SetInt("locale", (int)LocaleType.SE);

        Locale.Reset();
		string newPlatform = "SE";
        tk2dPreferences prefs = tk2dPreferences.inst;
        prefs.platform = newPlatform;
        UnityEditor.EditorPrefs.SetString("tk2d_platform", newPlatform);
        tk2dSystem.CurrentPlatform = prefs.platform; // mirror to where it matters
        tk2dSystemUtility.PlatformChanged(); // tell the editor things have changed
        tk2dEditorUtility.UnloadUnusedAssets();
    }

	[MenuItem("NOVNINE/LocaleSystem/SetLocale/DK", false, 3103)]
	static void SetLocaleDK()
    {
		PlayerPrefs.SetInt("locale", (int)LocaleType.DK);

        Locale.Reset();
		string newPlatform = "DK";
        tk2dPreferences prefs = tk2dPreferences.inst;
        prefs.platform = newPlatform;
        UnityEditor.EditorPrefs.SetString("tk2d_platform", newPlatform);
        tk2dSystem.CurrentPlatform = prefs.platform; // mirror to where it matters
        tk2dSystemUtility.PlatformChanged(); // tell the editor things have changed
        tk2dEditorUtility.UnloadUnusedAssets();
    }

	[MenuItem("NOVNINE/LocaleSystem/SetLocale/NO", false, 3104)]
	static void SetLocaleNO()
    {
		PlayerPrefs.SetInt("locale", (int)LocaleType.NO);

        Locale.Reset();
		string newPlatform = "NO";
        tk2dPreferences prefs = tk2dPreferences.inst;
        prefs.platform = newPlatform;
        UnityEditor.EditorPrefs.SetString("tk2d_platform", newPlatform);
        tk2dSystem.CurrentPlatform = prefs.platform; // mirror to where it matters
        tk2dSystemUtility.PlatformChanged(); // tell the editor things have changed
        tk2dEditorUtility.UnloadUnusedAssets();
    }

	[MenuItem("NOVNINE/LocaleSystem/SetLocale/DE", false, 3105)]
	static void SetLocaleDE()
    {
		PlayerPrefs.SetInt("locale", (int)LocaleType.DE);

        Locale.Reset();
		string newPlatform = "DE";
        tk2dPreferences prefs = tk2dPreferences.inst;
        prefs.platform = newPlatform;
        UnityEditor.EditorPrefs.SetString("tk2d_platform", newPlatform);
        tk2dSystem.CurrentPlatform = prefs.platform; // mirror to where it matters
        tk2dSystemUtility.PlatformChanged(); // tell the editor things have changed
        tk2dEditorUtility.UnloadUnusedAssets();
    }
}

