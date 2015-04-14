using System;
using System.IO;
using UnityEngine;
using System.Collections;
using System.Text;

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

        public static bool SetFileLength(string filepath, long length)
        {
            try
            {
                using (FileStream fs = new FileStream(filepath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    fs.SetLength(length);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
                File.Delete(filepath);
                return false;
            }
            
            return true;
        }

        public static long GetFileLength(string filepath)
        {
            FileInfo fileInfo = new FileInfo(filepath);
            return fileInfo.Length;
        }
    }

    public class PathEx
    {
        public static bool EndWithDirectorySeparatorChar(string path)
        {
            return path.EndsWith("\\") || path.EndsWith("/");
        }

        /// <summary>
        /// 绝对路径转相对路径
        /// </summary>
        /// <param name="strBasePath">基本路径</param>
        /// <param name="strFullPath">绝对路径</param>
        /// <returns>strFullPath相对于strBasePath的相对路径</returns>
        public static string GetRelativePath(string strBasePath, string strFullPath)
        {
            if (strBasePath == null)
                throw new ArgumentNullException("strBasePath");

            if (strFullPath == null)
                throw new ArgumentNullException("strFullPath");
            
            strBasePath = Path.GetFullPath(strBasePath);
            strFullPath = Path.GetFullPath(strFullPath);
            
            var DirectoryPos = new int[strBasePath.Length];
            int nPosCount = 0;
            
            DirectoryPos[nPosCount] = -1;
            ++nPosCount;
            
            int nDirectoryPos = 0;
            while (true)
            {
                nDirectoryPos = strBasePath.IndexOf('\\', nDirectoryPos);
                if (nDirectoryPos == -1)
                    break;
                
                DirectoryPos[nPosCount] = nDirectoryPos;
                ++nPosCount;
                ++nDirectoryPos;
            }
            
            if (!strBasePath.EndsWith("\\"))
            {
                DirectoryPos[nPosCount] = strBasePath.Length;
                ++nPosCount;
            }     
            
            int nCommon = -1;
            for (int i = 1; i < nPosCount; ++i)
            {
                int nStart = DirectoryPos[i - 1] + 1;
                int nLength = DirectoryPos[i] - nStart;
                
                if (string.Compare(strBasePath, nStart, strFullPath, nStart, nLength, true) != 0)
                    break;
                
                nCommon = i;
            }
            
            if (nCommon == -1)
                return strFullPath;
            
            var strBuilder = new StringBuilder();
            for (int i = nCommon + 1; i < nPosCount; ++i)
                strBuilder.Append("..\\");
            
            int nSubStartPos = DirectoryPos[nCommon] + 1;
            if (nSubStartPos < strFullPath.Length)
                strBuilder.Append(strFullPath.Substring(nSubStartPos));
            
            string strResult = strBuilder.ToString();
            return strResult == string.Empty ? ".\\" : strResult;
        }
    }
}