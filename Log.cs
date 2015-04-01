using UnityEngine;
using System.Collections;

namespace dpull
{
    public class Log : MonoBehaviour 
    {
        private System.IO.StreamWriter LogWriter;
        
        void Start () 
        {
            CleanOldLog();
            Application.RegisterLogCallback(HandleLog);
        }
        
        void OnDestory()
        {
            Application.RegisterLogCallback(null);
        }

        string GetLogDirectory()
        {
            var path = System.IO.Path.Combine(Application.persistentDataPath, "logs");
            if (!System.IO.Directory.Exists(path))
                System.IO.Directory.CreateDirectory(path);
            return path;
        }

        void CleanOldLog()
        {
            var dir = GetLogDirectory();
            var files = System.IO.Directory.GetFiles(dir, "*.log", System.IO.SearchOption.AllDirectories);
            var now = System.DateTime.Now;
            var expirationTime = new System.TimeSpan(24 * 3, 0, 0);
            
            foreach (var file in files)
            {
                var fileInfo = new System.IO.FileInfo(file);
                var subTime = now - fileInfo.LastWriteTime;
                if (subTime < expirationTime)
                    continue;

                try
                {
                    System.IO.File.Delete(file);
                }
                catch
                {
                }
            }
        }

        void HandleLog(string logString, string stackTrace, LogType type)
        {
            if (type == LogType.Log && !logString.StartsWith("Lua:"))
                return;
            
            if (LogWriter == null)
            {
                lock(this)
                {
                    if (LogWriter == null)
                    {
                        try
                        {
                            var logfile = GetLogDirectory();
                            logfile = System.IO.Path.Combine(logfile, string.Format("{0}.log", System.DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss")));
                            LogWriter = new System.IO.StreamWriter(logfile);
                        }
                        catch
                        {
                        }
                    }
                }
            }
            
            if (LogWriter != null)
            {
                this.SendMessage("CacheLog", new string[]{logString, stackTrace}, SendMessageOptions.DontRequireReceiver);
                
                string log = string.Format("{0}<{1}>: {2}", System.DateTime.Now.ToString("MM-dd HH:mm:ss"), type.ToString(), logString);
                LogWriter.WriteLine(log);
                if (!string.IsNullOrEmpty(stackTrace))
                {
                    var formatStackTrace = stackTrace.Replace("\n", "\n\t");
                    LogWriter.Write("\t");
                    LogWriter.WriteLine(formatStackTrace);
                }
                LogWriter.Flush();
            }
        }
    }
}

