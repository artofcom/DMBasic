using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[AddComponentMenu("2D Toolkit/Sprite/tk2dLocaleSprite")]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
[ExecuteInEditMode]
public class tk2dLocaleSprite : tk2dSprite
{
/*
    public override void Build()
    {
        if(currentLocaleCol != null) {
            Collection = currentLocaleCol;
            renderer.sharedMaterial = null;
            UpdateMaterial();
        }
        base.Build();
    }

    public tk2dSpriteCollectionData currentLocaleCol
    {
        get {
            if(_currentLocaleCol == null) {
                GameObject go = Resources.Load("locale"+Locale.name+"_Data/data") as GameObject;
                if(go == null) {
                    go = Resources.Load("localeEN_Data/data") as GameObject;
                    Debug.LogWarning("Locale SpriteCollection not Found for "+Locale.name);
                }
                if(go == null) {
                    Debug.LogError("Default(EN) Locale SpriteCollection not Found");
                    return null;
                }
                _currentLocaleCol = go.GetComponent<tk2dSpriteCollectionData>();
            }
            return _currentLocaleCol;
        }
    }

    private static tk2dSpriteCollectionData _currentLocaleCol;
    */
}

