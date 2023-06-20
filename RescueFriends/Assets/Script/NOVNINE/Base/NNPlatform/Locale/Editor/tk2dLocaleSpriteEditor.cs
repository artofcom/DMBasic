using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[CustomEditor(typeof(tk2dLocaleSprite))]
class tk2dLocaleSpriteEditor : tk2dSpriteEditor
{
    [MenuItem("GameObject/Create Other/tk2d/LocaleSprite", false, 12905)]
    static void DoCreateSpriteObject()
    {
        tk2dSpriteCollectionData sprColl = null;
        if (sprColl == null) {
            // try to inherit from other Sprites in scene
            tk2dSprite spr = GameObject.FindObjectOfType(typeof(tk2dSprite)) as tk2dSprite;
            if (spr) {
                sprColl = spr.Collection;
            }
        }

        if (sprColl == null) {
            tk2dSpriteCollectionIndex[] spriteCollections = tk2dEditorUtility.GetOrCreateIndex().GetSpriteCollectionIndex();
            foreach (var v in spriteCollections) {
                GameObject scgo = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(v.spriteCollectionDataGUID), typeof(GameObject)) as GameObject;
                var sc = scgo.GetComponent<tk2dSpriteCollectionData>();
                if (sc != null && sc.spriteDefinitions != null && sc.spriteDefinitions.Length > 0) {
                    sprColl = sc;
                    break;
                }
            }

            if (sprColl == null) {
                EditorUtility.DisplayDialog("Create Sprite", "Unable to create sprite as no SpriteCollections have been found.", "Ok");
                return;
            }
        }

        GameObject go = tk2dEditorUtility.CreateGameObjectInScene("Sprite");
        tk2dSprite sprite = go.AddComponent<tk2dLocaleSprite>();
        sprite.Collection = sprColl;
        sprite.GetComponent<Renderer>().material = sprColl.FirstValidDefinition.material;
        sprite.Build();
    }

}

