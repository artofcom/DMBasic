// 서버 side 작업이 완료되기 전까지 임시 봉인.
//#define LLOG_DEBUGER      // local에 전체 detailed log를 남긴다.
//#define LOCAL_DEBUGER       // remote-web을 사용하지 않고 local에 unity log 처리.

#if UNITY_EDITOR
    #define LOCAL_DEBUGER
#endif

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using JsonFx.Json;
using System.Text;
using NOVNINE;

#pragma warning disable 0414 // value is assigned but its value is never used
#pragma warning disable 0219

namespace NOVNINE.Diagnostics
{
	public static class Debugger
	{
	    static internal readonly string assertPrefix = "[ASSERT] ";
	
	    public static bool EnableLocalLog {
	        get {
	            return TraceListener.enableLocalLog;
	        }
	        set {
	            TraceListener.enableLocalLog = value;
	        }
	    }
	    public static bool EnableRemoteLog {
	        get {
	            return TraceListener.enableRemoteLog;
	        }
	        set {
	            TraceListener.enableRemoteLog = value;
	        }
	    }
	
	    public class Application
	    {
	        //static event UnityEngine.Application.LogCallback logCallback;
	
	        public static void RegisterLogCallback (UnityEngine.Application.LogCallback handler, bool register = true)
	        {
	            if (null == handler) {
					UnityEngine.Debug.LogError("NOVNINE.Diagnostics.Debugger.Application::RegisterLogCallback - invalid handle, register : " + register.ToString());
	                return;
	            }
	
	            if (true == register)
					UnityEngine.Application.logMessageReceived += handler;
	            else
					UnityEngine.Application.logMessageReceived -= handler;
	
				//UnityEngine.Application.logMessageReceived(logCallback);
	        }
	    }
	
	    public static bool Launch(Hashtable param)
	    {
	        TraceListener.Init(param);
	
	        return true;
	    }
	
		public static string BuildStackTrace(int skipLine)
		{
			string stack = UnityEngine.StackTraceUtility.ExtractStackTrace ();
	        
			//skip 2 new lines
			while(skipLine > 0) {
				int idx = stack.IndexOf(System.Environment.NewLine, 0);
				if(idx != -1) {
					//Debug.Log(skipLine+" : "+ stack.Substring(0, idx));
					stack = stack.Substring(idx+1);
				}
				skipLine--;
			}
			//Debug.Log(stack);
			return stack;
		}
	
	    public static void Assert(bool condition)
	    {
	        if (true == condition) return;
			string stackMsg = BuildStackTrace(2);
	        Debug.LogException(new System.Exception(assertPrefix+stackMsg));
							
	//#if !NN_DEPLOY
	//		Platform.SystemAlert("고쳐주세요..ㅠ_ㅠ", stackMsg, "눼눼 알겠쭙니다", null , (ok) => {
	//			UnityEngine.Application.Quit();
	//		});
	//#endif
	
	    }
	
	    public static void Assert(bool condition, string message)
	    {
	        if (true == condition) return;
			string stackMsg = BuildStackTrace(2);
	        Debug.LogException(new System.Exception(assertPrefix+message+System.Environment.NewLine+stackMsg));
	//#if !NN_DEPLOY
	//		Platform.SystemAlert("AssertFail:"+message, stackMsg, "눼눼 알겠쭙니다", null, (ok) => {
	//			UnityEngine.Application.Quit();
	//		});
	//#endif
	    }
	
	    public static void Log(string message)
	    {
	        if(EnableLocalLog) Debug.Log(message);
	    }
	
	    public static void LogWarning(string message)
	    {
	        Debug.LogWarning(message);
	    }
	
	    public static void LogError(string message)
	    {
	        Debug.LogError(message);
	    }
	}
	
	static class TraceListener
	{
	    static readonly string KEY_LAST_EventLog = "debuger_EventLog";

        /*
        #if LIVE_MODE
        static readonly string postURL = "http://tastymagic.ttsgames.com/WebLog/weblog_dumpinsert.php";         // "http://watson.bitmango.com/errlog11.php";
        static readonly string postRemoteURL = "http://tastymagic.ttsgames.com/WebLog/weblog_dumpinsert.php";   // "http://watson.bitmango.com/errlog211.php";
        #elif DEV_TEST
        static readonly string postURL = "http://nov9games.iptime.org:83/WebLog_TEST/weblog_dumpinsert.php";         // "http://watson.bitmango.com/errlog11.php";
        static readonly string postRemoteURL = "http://nov9games.iptime.org:83/WebLog_TEST/weblog_dumpinsert.php";   // "http://watson.bitmango.com/errlog211.php";
        #else
        static readonly string postURL = "http://nov9games.iptime.org:83/WebLog/weblog_dumpinsert.php";         // "http://watson.bitmango.com/errlog11.php";
        static readonly string postRemoteURL = "http://nov9games.iptime.org:83/WebLog/weblog_dumpinsert.php";   // "http://watson.bitmango.com/errlog211.php";
        #endif */
	    
        static readonly string postURL = "";//http://nov9games.iptime.org:83/WebLog/weblog_dumpinsert.php";         // "http://watson.bitmango.com/errlog11.php";
        static readonly string postRemoteURL = "";// "http

	    // external Event Call back
	    public static event System.Action<string> wwwRequestCallback;
	    public static event System.Action<bool, string> wwwResponseCallback;
	
	    public static bool enableLocalLog = false;
	    public static bool enableRemoteLog = false;
        public static bool encodeWithBase64 = true;
	
	    #region inner-class define (EventLog, Filter)
	    class EventLog
	    {
	        public readonly float tick;
	
	        public string condition;
	        public string stacktrace;
	        public string type;
	
	        public EventLog(string condition, string stacktrace, string type)
	        {
	            this.tick = Time.realtimeSinceStartup;
	
	            this.condition = condition;
	            this.stacktrace = stacktrace;
	            this.type = type;
	        }
	
	        public bool Equals(EventLog msg)
	        {
	            if (this.condition != msg.condition) return false;
	            if (this.stacktrace != msg.stacktrace) return false;
	            if (this.type != msg.type) return false;
	
	            return true;
	        }
	
	        public override string ToString()
	        {
	            return string.Format("{0}:{1}\r\n{2}\r\n{3}", this.tick, this.type, this.condition, this.stacktrace);
	        }
	    }
	
	    enum FilterType { Duration, Duplicate};
	    class Filter
	    {
	        public FilterType type = FilterType.Duration;
	
	        public float time = 0f;
	        public int count = 0;
	
	        public Filter(float time, int count, FilterType type)
	        {
	            this.time = time;
	            this.count = count;
	
	            this.type = type;
	        }
	
	        public void Remove(List<EventLog> queueMsg)
	        {
	            if (this.type != FilterType.Duration) return;
	
	            float tick = Time.realtimeSinceStartup;
	
	            int count = 0;
	            for (int i=queueMsg.Count-1; i>=0; i--) {
	                EventLog msg = queueMsg[i];
	                if (this.count == 0) {
	                    if ((tick - msg.tick) <= this.time) continue;
	
	                    queueMsg.RemoveAt(i);
	                } else {
	                    if ((tick - msg.tick) > this.time) continue;
	
	                    count+=1;
	                    if (count <= this.count) continue;
	
	                    queueMsg.RemoveAt(i);
	                }
	            }
	        }
	
	        public bool IsFiltered(List<EventLog> queueMsg, EventLog item)
	        {
	            if (this.count == 0) return false;
	
	            float tick = Time.realtimeSinceStartup;
	            int count = queueMsg.Count((msg)=> {
	                if ((tick - msg.tick) > time) return false;
	                if (this.type == FilterType.Duplicate && false == item.Equals(msg)) return false;
	
	                return true;
	            });
	
	            if (count >= this.count) return true;
	            return false;
	        }
	    }
	    #endregion
	
	    #region Filter Instance
	    static Filter []filters = new Filter[] {
	        new Filter(5f, 2, FilterType.Duplicate),		// 5초이내 2번까지 완전중복 허용 (Duplicate)
	
	        new Filter(5f, 5, FilterType.Duration), 		// 5초이내 EventLog 5번까지 허용 (Duration)
	        new Filter(20f, 10, FilterType.Duration), 	// 20초이내 EventLog 10번까지 허용 (Duration)
	        new Filter(80f, 15, FilterType.Duration), 	// 80초이내 EventLog 15번까지 허용 (Duration)
	        new Filter(320f, 20, FilterType.Duration), // 320초이내 EventLog 20번까지 허용 (Duration)
	        new Filter(320f, 0, FilterType.Duration)	// 320초이상 EventLog 모두 제거.
	    };
	    #endregion
	
	    static List<EventLog> queueMsg = new List<EventLog>();
	
	    static Hashtable defaultTable = null;
	
	    static public void Init(Hashtable param)
	    {

            // note : 원래는 ControlCenter를 통해 profile 정보를 set하고 그 정보를 바탕으로 
            //        관련 flag들이 세팅되나, Nov9 에서는 별도 서버를 사용하므로, ControlCenter를 
            //        사용하지 않는다. 따라서 default로 아래와 같이 처리.
            enableLocalLog      = false;// true;
            enableRemoteLog     = true; // false;
            encodeWithBase64    = true;
            //

	        int ul = NOVNINE.Profile.UserLv; //don't remove - scjoo
	        if (null != defaultTable) return;
	
	        defaultTable = new Hashtable() {
	            #region Table Value..
	            {"startup", System.DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff")},
	            {"session", System.Guid.NewGuid().ToString()}
	            #endregion
	        };
	
	        if(null != param ) 
			{
	            foreach (DictionaryEntry item in param) 
				{
	                defaultTable[item.Key] = item.Value;
	            }
	        }
	
	        // Application.ThreadException += new ThreadExceptionEventHandler(Application_ThreadException);
	        System.AppDomain.CurrentDomain.UnhandledException += OnUnresolvedExceptionHandler;
	        NOVNINE.Diagnostics.Debugger.Application.RegisterLogCallback(OnLogCallbackHandler);
	#if UNITY_IPHONE
	        if(Application.platform == RuntimePlatform.IPhonePlayer)
	            NOVNINE.Diagnostics.Debugger.Application.RegisterLogCallback(OnLogCallbackHandler_iOS);
	#endif
	        RetryLastWaitEventLog();		//이전에 비정상 종료시 보내지 못한 메시지가 있을 경우, 다시 보내기
	    }
	
	    #region Filtering Policy
	    static void FilterPolicyRemove()
	    {
	        foreach (Filter filter in filters) {
	            filter.Remove(queueMsg);
	        }
	    }
	    static bool FilterPolicyAdd(EventLog item)
	    {
	        foreach (Filter filter in filters) {
	            if (true == filter.IsFiltered(queueMsg, item)) return false;
	        }
	
	        //this.queueMsg.Enqueue(item);
	        queueMsg.Add(item);
	        return true;
	    }
	    #endregion
	
	    #region Log Callback Handler
	    static void OnLogCallbackHandler (string condition, string stacktrace, LogType type)
	    {
#if LLOG_DEBUGER
	        System.IO.File.AppendAllText(UnityEngine.Application.persistentDataPath+ "//" + "FullLog" + ".txt", string.Format("{0}\r\n{1}\r\n{2}\r\n", type, condition, stacktrace) + System.Environment.NewLine);
#endif
            // note : remote 든 local 이든 exeption만 처리하자.
            //if (!enableRemoteLog) {
	            if (type == LogType.Log)        return;
	            if (type == LogType.Warning)    return;
	        //}
	
            if (string.IsNullOrEmpty(stacktrace))
            {
                System.Diagnostics.StackTrace _stackTrace = new System.Diagnostics.StackTrace();
                stacktrace = _stackTrace.ToString().Replace("   at ", "");
            }
            
	        if (type == LogType.Exception) 
            {
	            int assertIndex = condition.IndexOf(Debugger.assertPrefix);
	            if (assertIndex>=0) {
	                condition = LogType.Assert.ToString() + ": " + condition.Substring(assertIndex + Debugger.assertPrefix.Length);
	                type = LogType.Assert;
	            }
	        }
	
//            if (_stackTrace.FrameCount > 0)
//            {
//                var stackFrame = _stackTrace.GetFrame(_stackTrace.FrameCount - 1);
//                var lineNumber = stackFrame.GetFileLineNumber();
//
//                stacktrace += string.Format(" lineNumber:{0}", lineNumber);
//            }

	        ProcessCallbackEventLog(condition, stacktrace, type.ToString());
	    }
	
	#if UNITY_IPHONE
	    static void OnLogCallbackHandler_iOS (string condition, string stacktrace, LogType type)
	    {
	        if(enableLocalLog) 
	            NOVNINE.Native.iOSExt.NSLog(type.ToString()+"] "+condition);
	    }
	#endif//UNITY_IPHONE
	
	    static void OnUnresolvedExceptionHandler(object sender, System.UnhandledExceptionEventArgs args)
	    {
	        if(args == null) return;
	        if(args.ExceptionObject == null) return;
	
	        try {
	            System.Type type	= args.ExceptionObject.GetType();
	            if(type == typeof(System.Exception)) 
                {
	                System.Exception e	= (System.Exception)args.ExceptionObject;

	                string condition = e.Message;
//                    string stacktrace = "";

//                    System.Diagnostics.StackTrace _stackTrace = new System.Diagnostics.StackTrace(e, true);
//                    var stackFrames = _stackTrace.GetFrames();
//
//                    foreach (var frame in stackFrames)
//                    {
//                        stacktrace += string.Format("{0} : {1}\n", frame.GetMethod().Name,frame.GetFileLineNumber());
//                    }

                    string stacktrace = e.ToString();   // include name, EventLog, inner exception, stackTrace.

	                if(args.IsTerminating) 
                    {
	                    // LogError..
	                    ProcessCallbackEventLog(condition, stacktrace, "UnresolvedTerminate");
	                }
                    else 
                    {
	                    // LogWarning..
	                    ProcessCallbackEventLog(condition, stacktrace, "UnresolvedException");
	                }
	            }
	        } catch {
	            Debug.Log("Debuger: Failed to resolve exception");
	        }
	    }
	    #endregion
	
	    static void ProcessCallbackEventLog(string condition, string stacktrace, string type)
	    {
	        EventLog msg = new EventLog(condition, stacktrace, type);
	
	        FilterPolicyRemove();
	        if (false == FilterPolicyAdd(msg)) return;
	
	        Hashtable table = defaultTable.Clone() as Hashtable;
	        table.Add("msg_since", msg.tick.ToString("F3"));
	        table.Add("msg_type", msg.type);
	        table.Add("msg_condition", msg.condition);
	        table.Add("adid", NOVNINE.Profile.ADID);
	        table.Add("msg_stacktrace", msg.stacktrace);
	
			//string textData= MiniJSON.Json.Serialize(table);
			
			StringBuilder output = new StringBuilder();
			JsonWriterSettings writerSetting = new JsonWriterSettings();
			writerSetting.PrettyPrint = true;

			using (JsonWriter writer = new JsonWriter(output, writerSetting))
			{
				writer.Write(table);
				TaskManager.StartCoroutine( CoSendEventLog(NOVNINE.Profile.UUID, output.ToString()) );
				//TaskManager.StartCoroutine( CoSendEventLog(NOVNINE.Profile.UUID, textData) );
			}
	    }
	
	    static void RetryLastWaitEventLog()
	    {
	        string EventLog = PlayerPrefs.GetString(KEY_LAST_EventLog);
	        TaskManager.StartCoroutine( CoSendEventLog(NOVNINE.Profile.UUID, EventLog, true) );
	    }
	
	    static IEnumerator CoSendEventLog(string guid, string EventLog, bool retry = false)
	    {
            if(null==EventLog || EventLog=="")
                yield break;

	#if LLOG_DEBUGER
	        System.IO.File.AppendAllText(UnityEngine.Application.persistentDataPath+ "//" + "Debuger.txt", EventLog + System.Environment.NewLine);
	#endif
	
	#if LOCAL_DEBUGER
	        yield return 0;
	
	        if (null != wwwResponseCallback) wwwResponseCallback(true, "success (local debug)");
	
	#if LLOG_DEBUGER
	        System.IO.File.AppendAllText(UnityEngine.Application.persistentDataPath+ "//" + "Response.txt", postURL + System.Environment.NewLine);
	#endif
	        yield break;
	#else
	
            // = test... yield break;

		if (null != wwwRequestCallback) wwwRequestCallback(EventLog);
        bool result = false;

        WWW www = null;
        if (UnityEngine.Application.internetReachability != NetworkReachability.NotReachable)
        {
            string strLog       = EventLog;
            //if(true == encodeWithBase64)
            //   strLog           = NOVNINE.Encryptor.EncodeWithKey(EventLog, "Nove9Games");

            WWWForm form = new WWWForm();
            //form.AddField("JSON", strLog);
            form.AddField("DumpData", strLog);
            form.AddField("Version", Application.version);

            //= Debug.Log("=== error log : " + EventLog);

            if(!enableRemoteLog) {
                www = new WWW(postURL, form);
            } else {
                www = new WWW(postRemoteURL, form);
            }
            yield return www;
            
            result = (true == string.IsNullOrEmpty(www.error)) ? true : false;
            if (null != wwwResponseCallback) 
            {
//            #if DEV_MODE
//            ToastAnalyticsInfo.ERR += string.Format("\n {0} \n",result ? www.text : www.error);
//            #endif
                wwwResponseCallback(result, result ? www.text : www.error);
            }

        } else {
            if (null != wwwResponseCallback) wwwResponseCallback(false, NetworkReachability.NotReachable.ToString());
        }

            // case of success.
#if LLOG_DEBUGER
            if(null != www)
            {
                if (true == result) {
	                System.IO.File.AppendAllText(Application.persistentDataPath+ "//" + "Response.txt", www.text + System.Environment.NewLine);
	            } else {
	                System.IO.File.AppendAllText(Application.persistentDataPath+ "//" + "Error.txt", www.error + System.Environment.NewLine);
	            }
            }
	#endif
	
	#endif
	    }
	}
}

