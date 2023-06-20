/**
* @file PPrefsSetting.cs
* @brief
* @author amugana (amugana@bitmango.com)
* @version 1.0
* @date 2012-08-15
*/

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.SocialPlatforms.Impl;
using System.IO;
using System.Reflection;
using System.Linq;
using NOVNINE;
//using NOVNINE.Store;

public static class PlatformInfoBuild
{
#if USE_PlatformUI
	[MenuItem ("NOVNINE/Build Platform Atlas", false, 2101)]
    public static void BuildPlatformGameAtlas()
    {
	//string dataPath = NOVNINE.Module.MODULE_PREFAB;//"Assets/NOVNINE/Resources/BitMangoPlatform.prefab";
        string atlasPath = "Assets/Resources/platformGameAtlas.prefab";
        string matPath = "Assets/Resources/platformGameAtlas.mat";
        //string texPath = "Assets/Resources/PlatformConfig/platformGameAtlas.png";
        //get PlatformInfo
        PlatformContext info = ModuleManager.LoadModuleHolder().GetComponent<PlatformContext>();

        //build GameAtlas

        Material mat = AssetDatabase.LoadAssetAtPath(matPath, typeof(Material)) as Material;
        if (mat == null) {
            Shader shader = Shader.Find("Unlit/Transparent Colored");
            mat = new Material(shader);

            AssetDatabase.CreateAsset(mat, matPath);
            AssetDatabase.Refresh();

            mat = AssetDatabase.LoadAssetAtPath(matPath, typeof(Material)) as Material;
        }

        GameObject go = AssetDatabase.LoadAssetAtPath(atlasPath, typeof(GameObject)) as GameObject;
        Object prefab = (go != null) ? go : PrefabUtility.CreateEmptyPrefab(atlasPath);

        // Create a new game object for the atlas
        go = new GameObject("platformGameAtlas");
        go.AddComponent<UIAtlas>().spriteMaterial = mat;

        // Update the prefab
        PrefabUtility.ReplacePrefab(go, prefab);
        Object.DestroyImmediate(go);

        // Select the atlas
        go = AssetDatabase.LoadAssetAtPath(atlasPath, typeof(GameObject)) as GameObject;
        UIAtlas atlas = go.GetComponent<UIAtlas>();

        List<string> ids = new List<string>();
        List<Texture> textures = new List<Texture>();
        //RenameTex(info.appIcon, "appIcon");

        if(info.appIcon == null) {
            EditorUtility.DisplayDialog("Error", "appIcon is Not Set on BitMangoPlatform.prefab", "ok");
            return;
        }

        ids.Add("appIcon");
        textures.Add(info.appIcon);

#if USE_UncleBill
        UncleBillContext ubCntx = info.GetComponent<UncleBillContext>();
        /*
        ids.Add("currencyIcon");
        if(ubCntx.currencyIcon == null)
            textures.Add(info.appIcon);
        else
            textures.Add(ubCntx.currencyIcon);
        */

        foreach(InventoryItem si in ubCntx.inventoryItems) {
            ids.Add(si.id);
            //if(si.icon == null)
            textures.Add(info.appIcon);
            //else
            //    textures.Add(si.icon);
        }
        foreach(ShopItem si in ubCntx.shopItems) {
            ids.Add(si.id);
            if(si.icon == null)
                textures.Add(info.appIcon);
            else
                textures.Add(si.icon);
        }
#endif

        /*
        textures.ForEach(tex=>{
        	UIAtlasMaker.AddOrUpdate(atlas, tex);
        });
        */
        List<Texture> uniqueTextures = textures.Distinct().ToList();
        //Debug.Log(textures.Count);
        //Debug.Log(uniqueTextures.Count);
        List<UIAtlasMaker.SpriteEntry> sprites = UIAtlasMaker.CreateSprites(uniqueTextures);
        UIAtlasMaker.ExtractSprites(atlas, sprites);
        UIAtlasMaker.UpdateAtlas(atlas, sprites);

        //Rename Sprites
        Dictionary<string, string> texIDmap = new Dictionary<string, string>();

        List<UISpriteData> sprList = new List<UISpriteData>();

        for(int i=0; i<textures.Count; ++i) {
            Debug.Log(i+" get tex : "+ids[i]);
            Debug.Log(i+" get tex : "+textures[i].name);
            UISpriteData spr = atlas.GetSprite(textures[i].name);
            if(spr == null) {
                Debug.Log("not found: adding "+ids[i]);
                if(texIDmap.ContainsKey(textures[i].name)) {
                    spr = atlas.GetSprite(texIDmap[textures[i].name]);
                    UISpriteData newSprite = new UISpriteData();
                    newSprite.CopyFrom(spr);
                    newSprite.name = ids[i];
                    sprList.Add(newSprite);
                } else {
                    Debug.LogError("Sprite Not found for ID:"+ids[i]+"  Tex:"+textures[i].name);
                }
            } else {
                Debug.Log("found: adding "+ids[i]);
                spr.name = ids[i];
                texIDmap[textures[i].name] = ids[i];
                sprList.Add(spr);
            }
        }
        atlas.spriteList = sprList;
        atlas.MarkAsDirty();

        EditorUtility.SetDirty(info);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
#endif

    /*
        [MenuItem ("NOVNINE/Build Platform Prefab", false, 2102)]
        static void BuildPlatformPrefab()
        {
            //Activator.BuildActivatorPrefab("Assets/Resources/platformConfig/Nov9Platform.prefab");
            NOVNINE.Module.BuildContextPrefab();
        }
        public static PlatformInfo LoadPlatformInfo()
        {
            GameObject go = AssetDatabase.LoadAssetAtPath("Assets/Resources/platformConfig/Nov9Platform.prefab", typeof(GameObject)) as GameObject;
            return go.GetComponent<PlatformInfo>();
        }

        public static void SavePlatformInfo(PlatformInfo info)
        {
            EditorUtility.SetDirty(info);
            AssetDatabase.SaveAssets();
        }
    */
}

