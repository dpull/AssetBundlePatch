using UnityEngine;
using System.Collections;
using UnityEditor;

namespace dpull
{
    [ExecuteInEditMode]
    public static class CodeFormat
    {
        [MenuItem("XStudio/Tools/Code Format")]
        public static void Process()
        {
            // ProcessDirectory("Assets/Component");
            // ProcessDirectory("Assets/Thirdparty/AutoBuild");
            ProcessDirectory("Assets/Thirdparty/UnityUtils");
            ProcessDirectory("Assets/Thirdparty/NGUIParticleSystem");
            ProcessDirectory("Assets/Thirdparty/UnityLua/Expand");
            Debug.Log("Code format finish");
        }

        static void ProcessDirectory(string path)
        {
            var srcs = System.IO.Directory.GetFiles(path, "*.cs", System.IO.SearchOption.AllDirectories);
            var utf8Encoding = new System.Text.UTF8Encoding(true);
            
            foreach(var src in srcs)
            {
                var auto = System.IO.File.ReadAllText(src);
                auto = auto.Replace("\r\n", "\n");
                auto = auto.Replace("\t", "    ");
                System.IO.File.WriteAllText(src, auto, utf8Encoding);
            }
        }
    }
}