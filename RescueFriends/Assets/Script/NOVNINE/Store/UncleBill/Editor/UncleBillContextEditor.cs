using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;

[CustomEditor(typeof(UncleBillContext))]
public class UncleBillContextEditor : Editor
{
    int a = 0, b = 0;

    public override void OnInspectorGUI()
    {
		
        var cntx = (UncleBillContext)target;
        base.OnInspectorGUI();

		return;
		
        a = EditorGUILayout.IntField("indexA", a);
        b = EditorGUILayout.IntField("indexB", b);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Swap.Inventory")) 
        {
            if (a>=0 && a <cntx.inventoryItems.Length && b>=0 && b<cntx.inventoryItems.Length)
            {
                NOVNINE.InventoryItem temp = cntx.inventoryItems[a];
                cntx.inventoryItems[a] = cntx.inventoryItems[b];
                cntx.inventoryItems[b] = temp;
                GUI.changed = true;
            }
        }

        if (GUILayout.Button("Swap.Shop")) 
        {
            if (a>=0 && a <cntx.shopItems.Count && b>=0 && b<cntx.shopItems.Count) 
            {
                NOVNINE.ShopItem temp = cntx.shopItems[a];
                cntx.shopItems[a] = cntx.shopItems[b];
                cntx.shopItems[b] = temp;
                GUI.changed = true;
            }
        }

        GUILayout.EndHorizontal();
    }
}
