using System;
using System.IO;
using UnityEngine;
using System.Collections;

namespace dpull
{
    public class DirectoryEx 
    {
        public static void CreateDirectory(string path)
        {
            path = Path.GetFullPath(path);
            CreateDirectory(new DirectoryInfo(path));
        }
        
        public static void CreateDirectory(DirectoryInfo path)
        {
            if (path.Exists)
                return;
            
            while (true)
            {
                var parent = path.Parent;
                if (parent == null || parent.Exists)
                    break;
                CreateDirectory(parent);
            }

            path.Create();
        }
        
        public static void DeleteDirectory(string path)
        {
            path = Path.GetFullPath(path);
            if (Directory.Exists(path))
                Directory.Delete(path, true);
        }
    }

    public class FileEx 
    {
        public static Stream OpenRead(string sourceFileName)
        {
            #if UNITY_ANDROID
            if (sourceFileName.StartsWith(Application.streamingAssetsPath))
            {
                var startIndex = Application.streamingAssetsPath.Length;
                if (!PathEx.EndWithDirectorySeparatorChar(Application.streamingAssetsPath))
                    startIndex++;
                
                sourceFileName = sourceFileName.Substring(startIndex);
                return new AndroidAssetStream(sourceFileName);
            }
            #endif
            return File.OpenRead(sourceFileName);
        }
    }

    public class PathEx
    {
        public static bool EndWithDirectorySeparatorChar(string path)
        {
            return path.EndsWith("\\") || path.EndsWith("/");
        }
    }
}