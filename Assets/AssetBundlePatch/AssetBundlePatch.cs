using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System;
using System.IO;

namespace dpull
{
    public enum AssetBundlePatchResult
    {
        Failed = -1,
        Succeed = 0,
        
        FromAssetBundleLoadFailed,
        DiffFileSaveFailed,
        DiffFileLoadFailed,
        ToAssetBundleSaveFailed,
    }

    public class AssetBundlePatch  
    {
        public static bool IsSupport(string file)
        {
            var bundle = assetbundle_load(file);
            if (bundle == IntPtr.Zero)
                return false;

            var ret = assetbundle_check(bundle);
            assetbundle_destroy(bundle);
            return ret;
        }

        public static AssetBundlePatchResult Diff(string appDataDir, string fromAssetbundle, string toAssetbundle, string diff)
        {
            if (string.IsNullOrEmpty(appDataDir))
                appDataDir = null;

            if (string.IsNullOrEmpty(fromAssetbundle))
                fromAssetbundle = null;

            if (string.IsNullOrEmpty(toAssetbundle))
                throw new System.ArgumentNullException("toAssetbundle");

            if (string.IsNullOrEmpty(diff))
                throw new System.ArgumentNullException("diff");

            return (AssetBundlePatchResult)assetbundle_diff(appDataDir, fromAssetbundle, toAssetbundle, diff);
        }

        public static AssetBundlePatchResult Merge(string appDataDir, string fromAssetbundle, string toAssetbundle, string diff)
        {
            if (string.IsNullOrEmpty(appDataDir))
                appDataDir = string.Empty;
            
            if (string.IsNullOrEmpty(fromAssetbundle))
                fromAssetbundle = null;
            
            if (string.IsNullOrEmpty(toAssetbundle))
                throw new System.ArgumentNullException("toAssetbundle");
            
            if (string.IsNullOrEmpty(diff))
                throw new System.ArgumentNullException("diff");

            AppDataDir = appDataDir;
            return (AssetBundlePatchResult)assetbundle_merge(ReadFile, IntPtr.Zero, fromAssetbundle, toAssetbundle, diff);
        }

        public static void Print(string diff, string output)
        {
            if (string.IsNullOrEmpty(diff))
                throw new System.ArgumentNullException("diff");

            if (string.IsNullOrEmpty(output))
                output = null;

            assetbundle_diff_print(diff, output);
        }

        public static bool SetFileSize(string file, uint length)
        {
            return filemapping_truncate(file, length);
        }

        public static string GetAppDataDir()
        {
            switch (Application.platform)
            {
            case RuntimePlatform.IPhonePlayer:
                return Directory.GetParent(Application.streamingAssetsPath).FullName;
                
            case RuntimePlatform.Android:
                return Path.Combine(Application.streamingAssetsPath, "bin/Data"); 
                
            default:
                throw new UnityException("Not support:" + Application.platform.ToString());
            }
        }

#if UNITY_EDITOR
        public static string GetEditorAppDataDir(string clientPath, UnityEditor.BuildTarget target)
        {
            switch (target)
            {
            case UnityEditor.BuildTarget.iPhone:
                return Path.Combine(clientPath, "Payload/hero.app/Data");

            case UnityEditor.BuildTarget.Android:
                return Path.Combine(clientPath, "assets/bin/Data");

            default:
                throw new UnityException("Not support:" + Application.platform.ToString());
            }
        }
#endif

        static bool TrySplitPath(string fullPath, ref string splitFileName, ref int splitIndex)
        {
            var match = ".split";
            var findIndex = fullPath.LastIndexOf(match);
            if (findIndex == -1)
                return false;

            var subPath = fullPath.Substring(0, findIndex);
            var indexStr = fullPath.Substring(findIndex + match.Length);
            var index = 0;
            if (!int.TryParse(indexStr, out index))
                return false;

            splitFileName = subPath;
            splitIndex = index;
            return true;
        }

        static string GetNextSplitPath(string splitFileName, ref int splitIndex)
        {
            splitIndex++;
            return string.Format("{0}.split{1}", splitFileName, splitIndex);
        }
                
        [AOT.MonoPInvokeCallback(typeof(ReadFileCallback))]
        static bool ReadFile(IntPtr buffer, string fileName, uint fileOffset, uint length, IntPtr userdata) 
        {
            try
            {
                var fullPath = Path.Combine(AppDataDir, fileName);
                var offset = (int)fileOffset;
                var left = (int)length;
                var data = new byte[length];
                var splitFileName = string.Empty;
                var splitIndex = 0;

                while (left > 0)
                {
                    Debug.Log("Merge from file:" + fullPath);
    
                    using (var stream = FileEx.OpenRead(fullPath))
                    {
                        if (stream == null) 
                        {
                            Debug.LogError("File not exist:" + fullPath);
                            return false;
                        }

                        stream.Seek(offset, SeekOrigin.Begin);
                        while (left > 0) 
                        {
                            var ret = stream.Read(data, (int)length - left, left);
                            if (ret <= 0)
                            {
                                if (string.IsNullOrEmpty(splitFileName))
                                    TrySplitPath(fullPath, ref splitFileName, ref splitIndex);

                                if (string.IsNullOrEmpty(splitFileName))
                                {
                                    Debug.LogError(string.Format("stream read failed. {0}, offset:{1}, length{2}, left:{3}", fileName, offset, length, left));
                                    return false;
                                }

                                fullPath = GetNextSplitPath(splitFileName, ref splitIndex);
                                offset = 0;
                                break;
                            }                            
                            left -= ret;
                        }
                    }
                }

                Marshal.Copy(data, 0, buffer, data.Length);
                return true;
            }
            catch(System.Exception e)
            {
                Debug.LogException(e);
                return false;
            } 
        }

        static string AppDataDir;

        #if UNITY_IPHONE
        internal const string LIBNAME = "__Internal";
        #else
        internal const string LIBNAME = "AssetBundleParser";
        #endif
        
        [DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "assetbundle_load")]
        static extern IntPtr assetbundle_load(string file);
        
        [DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "assetbundle_check")]
        static extern bool assetbundle_check(IntPtr bundle);
        
		[DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "assetbundle_destroy")]
        static extern void assetbundle_destroy(IntPtr bundle);
        
        [DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "assetbundle_diff")]
        static extern int assetbundle_diff(string appDataDir, string fromAssetbundle, string toAssetbundle, string diff);

        delegate bool ReadFileCallback(IntPtr buffer, string fileName, uint offset, uint length, IntPtr userdata);
        
        [DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "assetbundle_merge")]
        static extern int assetbundle_merge(ReadFileCallback callback, IntPtr userdata, string fromAssetbundle, string toAssetbundle, string diff);
    
        [DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "assetbundle_diff_print")]
        static extern void assetbundle_diff_print(string filename, string output);

        [DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "filemapping_truncate")]
        static extern bool filemapping_truncate(string filename, uint length);
    }
}
