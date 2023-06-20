using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using NOVNINE.Diagnostics;

namespace NOVNINE
{

	public class Module
    {
	    #region static
	    private static GameObject _holder = null;
	    public static readonly string ModuleCntx = "ModuleContext";

	    public static GameObject Holder
	    {
	        get {
	            Debugger.Assert(_holder != null);
	            //if(_holder == null)
	            //    Init();
	            return _holder;
	        }
	    }

	    public static void Init()
	    {
	        if(_holder == null) 
			{
	            GameObject prefab = Resources.Load(ModuleCntx) as GameObject;
	            if(prefab == null) 
				{
	                Debug.LogError("No "+ModuleCntx+".prefab found in Resources folder!");
	                return;
	            }
	            
				_holder = Object.Instantiate(prefab, Vector3.zero, Quaternion.identity) as GameObject;

				_holder.name = "__NOVNINEPlatform__";
	            //_holder.hideFlags = HideFlags.HideAndDontSave;
	            Object.DontDestroyOnLoad(_holder);
	        } 
			else
			{
	            Debug.LogWarning("Multiple Module cctor() called");
	        }
	    }
	
	    #endregion
	}

}

