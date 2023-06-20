using UnityEngine;
using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using NOVNINE.Diagnostics;

namespace NOVNINE
{
	public static class Nov9Request
	{
		public static WWW GET(string url, System.Action<string> callback = null)
	    {
			Debugger.Log("Nov9Request.Get "+url);
	        WWW www = new WWW (url);
			if (callback == null) return null;	
	        TaskRunner.Instance.Run(WaitForRequest (www, callback));
	        return www;
	    }
	
		public static WWW GET_Plain(string url,System.Action<string> callback = null)
	    {
			Debugger.Log("Nov9Request.GET_Plain "+url);
	        WWW www = new WWW (url);
	       if(callback ==null) return null; 
	        TaskRunner.Instance.Run(WaitForRequestLite(www,callback));
	        return www;
	    }
	    
	   public static IEnumerator WaitForRequestLite(WWW www,System.Action<string> callback)
	   {
	        yield return www;
	
	        if(www.error==null) {
	            if(callback!=null)
	                callback(www.text);
	        }else{
				Debugger.LogError(www.error);
	            if(callback!=null)
	                callback("");
	        }
	   }
	    private static IEnumerator WaitForRequest(WWW www, System.Action<string> callback)
	    {
	        yield return www;
	        string decrypted = null;
	        if (www.error == null) {
	            try {
	                decrypted = Encryptor.Decrypt(www.text);
					Debugger.Log("Nov9Request.decrypted : "+decrypted);
	            } catch(System.Exception e) {
					Debugger.LogError("Nov9Request.decrypt Fail.. text : "+ www.text +", error : "+e.ToString());
	            }
	            if (callback != null) {
	                if(string.IsNullOrEmpty(decrypted)) {
	                    callback("");
	                } else {
	                    callback(decrypted);
	                }
	            }
	        } else {
				Debugger.LogError(www.error);
				if (callback != null)
					callback("");
			}
	    }	
	}
}
