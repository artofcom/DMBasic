using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NOVNINE;

/// <summary> ##################################
/// 
/// NOTICE :
/// This script is conditions set to win/end the current game.
/// 
/// </summary> ##################################

public class EditLevelConditions : MonoBehaviour {
	[HideInInspector] public int levelIndex;
	[HideInInspector] public int levelLength;

	public System.Action<int> OnLevelLoad;
	public System.Action<int> OnSaveLevel;


#if UNITY_EDITOR
	
	public delegate void RepaintAction();
	public event RepaintAction WantRepaint;
	
	public void Repaint()
	{
		if (WantRepaint != null)
			WantRepaint();
	}
	
#endif

	public void LoadLevel(int index)
	{
		if (OnLevelLoad != null)
			OnLevelLoad(index);
	}

	public void SaveLevel(int index)
	{
		if (OnSaveLevel != null)
			OnSaveLevel(index);
	}

}
