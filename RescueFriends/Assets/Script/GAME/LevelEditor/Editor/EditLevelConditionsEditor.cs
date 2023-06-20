using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using JsonFx.Json;
using NOVNINE;

[CustomEditor (typeof(EditLevelConditions))]
public class EditLevelConditionsEditor : Editor 
{
    /*
	[MenuItem("NOVNINE/JMK/Mode - Edit", true)]
	static bool IsLevelEditorMode() {
		return !EditorPrefs.GetBool("JMK_EditorMode");
	}
	
	[MenuItem("NOVNINE/JMK/Mode - Edit")]
	static void LevelEditorMode() {
		EditorPrefs.SetBool("JMK_EditorMode", true);
	}

	[MenuItem("NOVNINE/JMK/Mode - Play", true)]
	static bool IsPlayMode() {
		return EditorPrefs.GetBool("JMK_EditorMode");
	}

	[MenuItem("NOVNINE/JMK/Mode - Play")]
	static void PlayMode() {
		EditorPrefs.SetBool("JMK_EditorMode", false);
	}*/

	private EditLevelConditions Target {
		get { return (EditLevelConditions)target; }
	}
	
    void OnEnable() 
	{
		if(Target != null)
			Target.WantRepaint += this.Repaint;
	}

	void OnDisable() 
	{
		if(Target != null)
			Target.WantRepaint -= this.Repaint;
	}

    public override void OnInspectorGUI() 
	{
		if(Target == null)
			return;
		
        base.OnInspectorGUI();

        EditLevelConditions conditions = target as EditLevelConditions;

        int newLvID = EditorGUILayout.IntField("Level ID (Global)", conditions.levelIndex+1);
        if(newLvID != conditions.levelIndex+1) 
		{
            conditions.levelIndex = newLvID - 1;              // id to index...
            conditions.LoadLevel(conditions.levelIndex);
        }
		
        if (GUILayout.Button("Load Level")) 
		{
            //var dataSet = PlayerPrefs.GetString("DataSet");
            //Root.SetPostfix("A");

            var path = EditorUtility.OpenFilePanel("Pick to edit", Application.dataPath+"/Data/text/"+Root.GetPostfix()+"/split", "txt");
            if(string.IsNullOrEmpty(path)) return;
            var fn = System.IO.Path.GetFileNameWithoutExtension(path);
            int idx = 0;
            if(System.Int32.TryParse(fn, out idx)) 
                conditions.LoadLevel(idx);
            else
                EditorUtility.DisplayDialog( "로드실패", "레벨파일이름은 반드시 숫자여야합니다.", "Ok");
        }
		
        if (GUILayout.Button("Save Level")) 
		{
            //var dataSet = PlayerPrefs.GetString("DataSet");
            //Root.SetPostfix(dataSet);

            conditions.SaveLevel(conditions.levelIndex);
        }
		
        if (GUILayout.Button("Save Level As..")) 
		{
            //var dataSet = PlayerPrefs.GetString("DataSet");
            //Root.SetPostfix(dataSet);

            var path = EditorUtility.SaveFilePanel("Where to save", "Assets/Data/text/"+Root.GetPostfix()+"/split", conditions.levelIndex.ToString()+".txt", "txt");
            if(string.IsNullOrEmpty(path)) return;
            var fn = System.IO.Path.GetFileNameWithoutExtension(path);
            int idx = 0;
            if(System.Int32.TryParse(fn, out idx)) 
                conditions.SaveLevel(idx-1);
            else
                EditorUtility.DisplayDialog( "저장실패", "레벨파일이름은 반드시 숫자여야합니다.", "Ok");
        }

		if (GUILayout.Button("Split Data To Levels")) 
			LevelEditorSceneHandler.Instance.SplitThemeToLevels();
        
        if (GUILayout.Button("Merge Levels To Data")) 
			LevelEditorSceneHandler.Instance.MergeLevelsToTheme();
		
        if (GUILayout.Button("Normalize All Levels"))
			LevelEditorSceneHandler.Instance.NormalizeAllLevels();
		
		if (GUILayout.Button("Export Levels Infomation")) 
		    ExportLevelsInfo();
        
        /*
        if (GUILayout.Button("Migration Levels")) {
            string[] packs = new string[] { "A", "B" };

            foreach (string pack in packs) {
                string path = Application.dataPath+"/Data/text/"+pack+"/";
                ConvertTreasure(path);
            }
        }
        */
	}

    void ExportLevelsInfo()
	{
        /* -
        string path = Application.dataPath+"/Data/text/"+PlayerPrefs.GetString("DataSet")+"/";

        if (Directory.Exists(path) == false) {
            UnityEditor.EditorUtility.DisplayDialog("에러", 
                string.Format("{0} 경로를 찾을 수 없습니다.", path), "Ok");
            return;
        }

        StreamReader streamReader = new StreamReader(path+"/Data.Root.txt");
        string data = streamReader.ReadToEnd();
        streamReader.Close();

        Root.SetPostfix(PlayerPrefs.GetString("DataSet"));
        Data.Root root = Root.Load();
        //root.resetCurrentLevel(WorldSceneHandler.ID_MAX_LEVEL);
        root.InitializeRoot(100, "0");

        string infos = "";
        int index = 1;

        foreach (Data.Level _lv in root.levels)
		{
			infos += string.Format("{0},{1}\n", index, GetLevelInfo(_lv));
			index++;
        }

        string outDir = Application.dataPath+"/../LevelInfo/";
        string outPath = outDir+System.DateTime.Now.ToString("yyyyMMdd_HHmmss")+".csv";

        if (Directory.Exists(outDir) == false) Directory.CreateDirectory(outDir);

        StreamWriter streamWriter = new StreamWriter(outPath);
        streamWriter.Write(infos);
        streamWriter.Close();
        AssetDatabase.Refresh();
        */
    }

    string GetLevelInfo(Data.Level level)
	{
        string info = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8}",
            GetLevelType(level), GetGoal(level), level.isTimerGame ? level.givenTime : level.allowedMoves, 
            level.scoreToReach[0], level.scoreToReach[1], level.scoreToReach[2], 
            GetBlockerInfo(level), GetFeatureInfo(level), GetColorCount(level));
        return info;
    }

    string GetLevelType(Data.Level level) 
	{
        if (level.isTimerGame)
		    return "TIMER";
        else if (level.isClearShadedGame)
            return "ICE";
        else if (level.isClearChocolateGame)
            return "SNOWMAN";
        else if (level.isGetTypesGame)
            return "JEWEL";
        else if (level.isTreasureGame)
            return "INGREDIENT";
		else if (level.isSpecialJewelGame)
            return "SPECIAL_JEWEL";
        else if (level.isPenguinGame)
            return "PENGUIN";
        else if (level.isYetiGame)
            return "YETI";
        else
            return "MOVE";
    }

    string GetGoal (Data.Level level) 
	{
        if (level.isTimerGame)
            return "";
        else if (level.isClearShadedGame)
            return GetShadedInfo(level);
        else if (level.isClearChocolateGame)
            return GetSnowmanInfo(level);
        else if (level.isGetTypesGame)
            return GetJewelInfo(level);
        else if (level.isTreasureGame)
            return GetTreasureInfo(level);
        else if (level.isSpecialJewelGame)
            return GetSpecialJewelInfo(level);
        else if (level.isPenguinGame)
            return GetPenguinInfo(level);
		else if (level.isYetiGame)
            return GetYetiInfo(level);
        else
            return "";
    }

    string GetShadedInfo (Data.Level level)
	{
        int s = 0;
        int d = 0;

        for (int i = 0; i < level.shaded.Length; i++)
		{
            if (level.shaded[i] == 0)
                s++;
            else if (level.shaded[i] == 1)
                d++;
        }

        return string.Format("single:{0};double:{1}", s, d);
    }

    string GetSnowmanInfo (Data.Level level)
	{
        int count = 0;
        
        for (int i = 0; i < level.panels.Length; i++) 
		{
            if (level.panels[i] == 4) count++;
        }

        return string.Format("snowman:{0}", count);
    }

    string GetJewelInfo (Data.Level level) 
	{
        string info = "";
        
        for (int i = 0; i < level.numToGet.Length; i++) 
		{
            if (level.numToGet[i] <= 0) continue;

            if (string.IsNullOrEmpty(info) == false)
                info += ";";

            switch (i) 
			{
                case 0 :
                    info += "red:";
                    break;
                case 1 :
                    info += "yellow:";
                    break;
                case 2 :
                    info += "green:";
                    break;
                case 3 :
                    info += "blue:";
                    break;
                case 4 :
                    info += "purple:";
                    break;
                case 5 :
                    info += "orange:";
                    break;
            }

            info += level.numToGet[i];
        }

        return info;
    }

    string GetTreasureInfo (Data.Level level) {
        return string.Format("countPotion1:{0};countPotion2:{1};countPotion3:{2}", level.countPotion1, level.countPotion2, level.countPotion3);
    }

    string GetSpecialJewelInfo (Data.Level level) {
        string info = "";
        
        for (int i = 0; i < level.specialJewels.Length; i++) {
            if (level.specialJewels[i] <= 0) continue;

            if (string.IsNullOrEmpty(info) == false) {
                info += ";";
            }

            switch (i) {
                case 0 :
                    info += "line:";
                    break;
                case 1 :
                    info += "line+line:";
                    break;
                case 2 :
                    info += "line+bomb:";
                    break;
                case 3 :
                    info += "line+rainbow:";
                    break;
                case 4 :
                    info += "bomb:";
                    break;
                case 5 :
                    info += "bomb+bomb:";
                    break;
                case 6 :
                    info += "bomb+rainbow:";
                    break;
                case 7 :
                    info += "rainbow:";
                    break;
                case 8 :
                    info += "rainbow+rainbow:";
                    break;
            }

            info += level.specialJewels[i];
        }
        
        return info;
    }

    string GetPenguinInfo (Data.Level level) {
        return string.Format("penguin:{0}", level.numberOfPenguin);
    }

    string GetYetiInfo (Data.Level level) {
        return string.Format("yeti:{0}", level.yetiHealth);
    }

    string GetBlockerInfo (Data.Level level) {
        Dictionary<string,int> blockerDict = new Dictionary<string,int>();

        for (int i = 0; i < level.panels.Length; i++) {
            string key = "";

            switch (level.panels[i]) {
                case 2 :
                    key = "Frost"+level.strengths[i];
                    break;
                case 4 :
                    key = "Snow"+level.strengths[i];
                    break;
                case 7 :
                    key = "Snowman"+level.strengths[i];
                    break;
                case 8 :
                    key = "Cage"+level.strengths[i];
                    break;
                case 18 :
                case 19 :
                case 20 :
                case 21 :
                    key = "Cake";
                    break;
            }

            if (string.IsNullOrEmpty(key)) continue;

            if (blockerDict.ContainsKey(key)) {
                blockerDict[key]++;
            } else {
                blockerDict.Add(key, 1);
            }
        }

        if (blockerDict.ContainsKey("Cake")) {
            blockerDict["Cake"] = blockerDict["Cake"] / 4;
        }

        for (int i = 0; i < level.pieces.Length; i++) {
            string key = "";

            switch (level.pieces[i]) {
                case 7 :
                    key = "Time Bomb";
                    break;
                case 8 :
                    key = "Spiral Snow";
                    break;
                case 9 :
                    key = "Penguin";
                    break;
            }

            if (string.IsNullOrEmpty(key)) continue;

            if (blockerDict.ContainsKey(key)) {
                blockerDict[key]++;
            } else {
                blockerDict.Add(key, 1);
            }
        }

        string info = "";

        foreach (string key in blockerDict.Keys) {
            if (string.IsNullOrEmpty(info) == false) {
                info += ";";
            }

            info += string.Format("{0}:{1}", key, blockerDict[key]);
        }

        return info;
    }

    string GetFeatureInfo (Data.Level level) {
        Dictionary<string,int> featureDict = new Dictionary<string,int>();

        for (int i = 0; i < level.panels.Length; i++) {
            string key = "";

            switch (level.panels[i]) {
                case 9 :
                    key = "Snowman Creator";
                    break;
                case 11 :
                    key = "Time Bomb Creator";
                    break;
                case 12 :
                    key = "Spiral Snow Creator";
                    break;
                case 13 :
                    key = "Ingredient Creator";
                    break;
                case 14 :
                    key = "Spiral Snow + Time Bomb Creator";
                    break;
                case 15 :
                    key = "Ingredient + Time Bomb Creator";
                    break;
                case 16 :
                    key = "Ingredient + Spiral Snow Creator";
                    break;
                case 22 :
                    key = "Conveyor";
                    break;
            }

            if (string.IsNullOrEmpty(key)) continue;

            if (featureDict.ContainsKey(key)) {
                featureDict[key]++;
            } else {
                featureDict.Add(key, 1);
            }
        }

        for (int i = 0; i < level.pieces.Length; i++) {
            string key = "";

            switch (level.pieces[i]) {
                case 1 :
                    key = "Horizontal";
                    break;
                case 2 :
                    key = "Vertical";
                    break;
                case 3 :
                    key = "Bomb";
                    break;
                case 4 :
                    key = "Rainbow";
                    break;
                case 10 :
                    key = "Mystery";
                    break;
                case 11 :
                    key = "Chameleon";
                    break;
                case 13 :
                    key = "Ghost";
                    break;
                case 14 :
                    key = "Frog";
                    break;
            }

            if (string.IsNullOrEmpty(key)) continue;

            if (featureDict.ContainsKey(key)) {
                featureDict[key]++;
            } else {
                featureDict.Add(key, 1);
            }
        }

        string info = "";

        foreach (string key in featureDict.Keys) {
            if (string.IsNullOrEmpty(info) == false) {
                info += ";";
            }

            info += string.Format("{0}:{1}", key, featureDict[key]);
        }

        return info;
    }

    int GetColorCount (Data.Level level) {
        if (level.normalProbability == null) return (int)LEItem.COLOR.NORMAL_COUNT;

        int count = 0;

        foreach (int p in level.normalProbability) {
            if (p > 0) count++;
        }

        return count;
    }

//    void ConvertTreasure (string path) {
//        var dataSet = PlayerPrefs.GetString("DataSet");
//        Root.SetPostfix(dataSet);
//        var rootData = Root.Load();
//
//        foreach(var thm in rootData.themes) {
//            string filePath = path + "lv_"+thm.id+ ".txt";
//            if (File.Exists(filePath) == false) continue;
//
//            StreamReader streamReader = new StreamReader(filePath);
//            string data = streamReader.ReadToEnd();
//            streamReader.Close();
//            List<Data.Level> levels = new List<Data.Level>(JsonReader.Deserialize<Data.Level[]>(data));
//
//            Debug.Log(">>> " + thm.id + " <<<");
//
//            foreach (Data.Level l in levels) {
//                if (l.chanceToSpawn < 7) {
//                    l.movePerTreasure = 7;
//                } else if (l.chanceToSpawn < 12) {
//                    l.movePerTreasure = 5;
//                } else if (l.chanceToSpawn < 31) {
//                    l.movePerTreasure = 4;
//                } else if (l.chanceToSpawn < 41) {
//                    l.movePerTreasure = 3;
//                } else if (l.chanceToSpawn < 71) {
//                    l.movePerTreasure = 2;
//                } else {
//                    l.movePerTreasure = 1;
//                }
//            }
//
//            string themeData = JsonWriter.Serialize(levels);
//            StreamWriter streamWriter = new StreamWriter(filePath);
//            streamWriter.Write(themeData);
//            streamWriter.Close();
//        }
//
//        AssetDatabase.Refresh();
//    }
}
