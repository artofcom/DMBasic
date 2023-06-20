using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace NOVNINE
{
	public static class WWWNov9
	{
	    public static string GetParametar(Dictionary<string,string> dic)
	    {
	        string parameter = "";
	
	        foreach( var param in dic ) {
	            parameter = parameter + param.Key + "=" + param.Value + "&";
	        }
	
	        string encodingUrl = WWW.EscapeURL(Encryptor.Encrypt( parameter.Substring(0,parameter.Length-1) ));
		
			//Debug.Log(parameter.Substring(0,parameter.Length-1));
	        return  encodingUrl;
	    }
	}
}
