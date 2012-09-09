using System;
using System.IO;

namespace hobd{

public class Logger
{
    public static bool DUMP = false;
    public static bool TRACE = false;
    public static bool INFO = false;
    public static bool WARN = false;
    public static bool ERROR = true;

    public const string ERROR_TAG = "ERROR";
    public const string INFO_TAG = "INFO ";
    public const string TRACE_TAG = "TRACE";
    public const string DUMP_TAG = "DUMP ";

    static StreamWriter fs;
    
    public static void Init(string path)
    {        
        try{
            fs = new StreamWriter(new FileStream( path, FileMode.Append));
        }catch(Exception){
        }
    }
    
    public static void error(String comp, String msg)
    {
        if (ERROR) error(comp, msg, null);
    }

    public static void error(String comp, String msg, Exception e)
    {
        if (ERROR) log(ERROR_TAG, comp, msg, e);
    }

    public static void info(String comp, String msg)
    {
        if (INFO) log(INFO_TAG, comp, msg, null);
    }

    public static void info(String comp, String msg, Exception e)
    {
        if (INFO) log(INFO_TAG, comp, msg, e);
    }

    public static void trace(String comp, String msg)
    {
        if (TRACE) log(TRACE_TAG, comp, msg, null);
    }

    public static void trace(String msg)
    {
        if (TRACE) log(TRACE_TAG, "", msg, null);
    }
    
    public static void dump(String comp, String msg)
    {
        if (DUMP) log(DUMP_TAG, comp, msg, null);
    }


    public static void log(string level, string comp, string msg, Exception e)
    {
        if (comp == null) comp = "";
        if (msg == null) msg = "";
        
        var nowms = DateTimeMs.NowMs;
        var ts = nowms.ToShortDateString().PadLeft(10) + " " + nowms.ToLongTimeString().PadLeft(8) + "." + nowms.Millisecond.ToString().PadLeft(3, '0');

        msg = "["+level+"] "+ ts + "["+comp+"]  " + msg;
        try{
            if (e != null)
            {
                msg +=  "\n" + e.GetType().ToString() + ": " + e.Message +"\n"+ e.StackTrace;
            }
            if (e != null && e.GetBaseException() != null && e.GetBaseException() != e)
            {
                e = e.GetBaseException();
                msg +=  "BaseException:\n" + e.GetType().ToString() + ": " + e.Message +"\n"+ e.StackTrace;
            }
        }catch(Exception){}
        try{
#if !__ANDROID__
            System.Console.WriteLine(msg);
#endif
#if __ANDROID__
            Android.Util.Log.Error("HOBD", msg);
#endif
            if (fs != null)
            {
                fs.Write(msg+"\n");
                fs.Flush();    
            }
        }catch(Exception){}
    }

    
    public static void SetLevel(string logLevel)
    {
        if (logLevel == "DUMP") {
            DUMP = TRACE = INFO = WARN = ERROR = true;
            return;
        }
        if (logLevel == "TRACE") {
            DUMP = false;
            TRACE = INFO = WARN = ERROR = true;
            return;
        }
        if (logLevel == "INFO") {
            DUMP = false;
            TRACE = false;
            INFO = WARN = ERROR = true;
            return;
        }
        if (logLevel == "WARN") {
            DUMP = TRACE = INFO = false;
            WARN = ERROR = true;
            return;
        }
        if (logLevel == "ERROR") {
            DUMP = TRACE = INFO = WARN = false;
            ERROR = true;
            return;
        }
        SetLevel("WARN");
    }
}


}