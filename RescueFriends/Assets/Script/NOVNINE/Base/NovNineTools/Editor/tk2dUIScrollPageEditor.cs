using UnityEngine;
using UnityEditor;
using System;
using System.Collections;

[CustomEditor(typeof(tk2dUIScrollPage))]
public class tk2dUIScrollPageEditor : Editor {
    bool collapsed;
    int customPageCount;

    public override void OnInspectorGUI () {
		tk2dUIScrollPage scrollPage = (tk2dUIScrollPage)target;

        LayoutObject<tk2dCamera>("Page Camera", ref scrollPage.pageCamera);
        LayoutFloat("Movement Sensitivity", ref scrollPage.movementSensitivity);
        LayoutFloat("Acceleration Sensitivity", ref scrollPage.accelerationSensitivity);
        LayoutFloat("Acceleration Threshold Time", ref scrollPage.accelerationThresholdTime);

 	    scrollPage.pageIconAlign = (PAGE_ICON_ALIGN)EditorGUILayout.EnumPopup("Page Icon Align", scrollPage.pageIconAlign);
        
        if (scrollPage.pageIconAlign != PAGE_ICON_ALIGN.DISABLE) {
            EditorGUI.indentLevel++;
            LayoutObject<GameObject>("Active Page Icon", ref scrollPage.activePageIcon);
            LayoutObject<GameObject>("Deactive Page Icon", ref scrollPage.deactivePageIcon);
            LayoutFloat("Icon Offset", ref scrollPage.iconOffset);
            LayoutFloat("Icon Margin", ref scrollPage.iconMargin);
            EditorGUI.indentLevel--;
        }

        LayoutBool("PageLength is Screen", ref scrollPage.pageLengthIsScreen);

        EditorGUI.indentLevel++;
        if (scrollPage.pageLengthIsScreen == false) LayoutFloat("Page Gap", ref scrollPage.pageGap);
        EditorGUI.indentLevel--;

        LayoutBool("Design in Editor", ref scrollPage.designInEditor);

        EditorGUI.indentLevel++;
        if (scrollPage.designInEditor) {
            LayoutBool("Auto Deactive Page", ref scrollPage.autoDeactivePage);

            collapsed = EditorGUILayout.Foldout(collapsed, "Custom Pages");

            if (collapsed) {
                EditorGUILayout.BeginVertical();

                if (scrollPage.customPages == null) {
                    scrollPage.customPages = new GameObject[0];
                }

                customPageCount = LayoutInt("Page Count", scrollPage.customPages == null ? 0 : scrollPage.customPages.Length);

                if (customPageCount != scrollPage.customPages.Length) {
                    int copyCount = Mathf.Min(scrollPage.customPages.Length, customPageCount);
                    GameObject[] pages = new GameObject[customPageCount];
                    Array.Copy(scrollPage.customPages, 0, pages, 0, copyCount);
                    scrollPage.customPages = pages;
                }

                for (int i = 0; i < scrollPage.customPages.Length; i++) {
                    GameObject page = scrollPage.customPages[i];
                    LayoutObject<GameObject>("Page "+i, ref page);
                    scrollPage.customPages[i] = page;
                }

                EditorGUILayout.EndVertical();
            }
        } else {
 	        LayoutInt("Prepare Page Count", ref scrollPage.preparePageCount);
            LayoutObject<GameObject>("Page Prefab", ref scrollPage.pagePrefab);
            LayoutBool("Use Level Package", ref scrollPage.useLevelPackage);

            if (scrollPage.useLevelPackage) {
                EditorGUI.indentLevel++;
                LayoutObject<GameObject>("Level Prefab", ref scrollPage.levelPrefab);
                LayoutVector2("Level Margin", ref scrollPage.packageMargin);
                LayoutVector2("Level Offset", ref scrollPage.packageOffset);

                EditorGUILayout.LabelField("Level Package Size");
                EditorGUI.indentLevel++;
                EditorGUILayout.BeginHorizontal();
 	            LayoutInt("Width", ref scrollPage.packageWidth);
 	            LayoutInt("Height", ref scrollPage.packageHeight);
                EditorGUILayout.EndHorizontal();

                if (scrollPage.levelPrefab != null) {
                    LayoutVector2("Level Prefab Size", ref scrollPage.levelSize);

                    if (GUILayout.Button("Calculate Level Prefab Size")) {
                        GameObject level = Instantiate(scrollPage.levelPrefab) as GameObject;
                        Bounds bounds = tk2dUIItemBoundsHelper.GetRendererBoundsInChildren(level.transform, level.transform);
                        scrollPage.levelSize = new Vector2(bounds.size.x, bounds.size.y);
                        DestroyImmediate(level);
                    }
                }

                EditorGUI.indentLevel--;
                EditorGUI.indentLevel--;
            }
        }
        EditorGUI.indentLevel--;

        LayoutObject<GameObject>("Event Listener", ref scrollPage.eventListener);

        if (GUI.changed) EditorUtility.SetDirty(scrollPage);
    }
    
    // Sortcut Methods

    int LayoutInt (string title, int _value) {
        return EditorGUILayout.IntField(title, _value);
    }

    void LayoutInt (string title, ref int _value) {
        _value = EditorGUILayout.IntField(title, _value);
    }

    void LayoutBool (string title, ref bool _value) {
        _value = EditorGUILayout.ToggleLeft(title, _value);
    }
    
    float LayoutFloat (string title, float _value) {
        return EditorGUILayout.FloatField(title, _value);
    }

    void LayoutFloat (string title, ref float _value) {
        _value = EditorGUILayout.FloatField(title, _value);
    }

    void LayoutString (string title, ref string _value) {
        _value = EditorGUILayout.TextField(title, _value);
    }

    void LayoutVector2 (string title, ref Vector2 _value) {
        _value = EditorGUILayout.Vector2Field(title, _value);
    }

    void LayoutObject<T> (string title, ref T obj) where T : UnityEngine.Object {
        obj = EditorGUILayout.ObjectField(title, obj, typeof(T), true) as T;
    }
}
