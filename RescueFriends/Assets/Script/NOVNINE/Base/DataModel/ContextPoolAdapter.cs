using UnityEngine;
using System.Reflection;
using NOVNINE.Diagnostics;

public static class ContextPoolAdapter 
{
    public static void LoadAllContext() {
		
#if USE_DLLDATACLASS
        ContextPool.LoadAllContext();
#else
        System.Type cpType = System.Type.GetType("ContextPool");
        Debugger.Assert(cpType != null);
        MethodInfo method = cpType.GetMethod("LoadAllContext");
        Debugger.Assert(method != null);
        method.Invoke(null, null);
#endif
    }
    public static void SaveAllContext() {
//#if USE_DLLDATACLASS
//        ContextPool.SaveAllContext();
//#else
        System.Type cpType = System.Type.GetType("ContextPool");
        Debugger.Assert(cpType != null);
        MethodInfo method = cpType.GetMethod("SaveAllContext");
        Debugger.Assert(method != null);
        method.Invoke(null, null);
//#endif
    }
    public static void Reset() {
		
//#if USE_DLLDATACLASS
//        ContextPool.Reset();
//#else
        System.Type cpType = System.Type.GetType("ContextPool");
        Debugger.Assert(cpType != null);
        MethodInfo method = cpType.GetMethod("Reset");
        Debugger.Assert(method != null);
        method.Invoke(null, null);
//#endif
    }
}
