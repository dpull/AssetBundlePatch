using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System;
using System.IO;

namespace dpull
{
    public class AssetBundlePatch  
    {
        public static bool IsSupport(string file)
        {
            var bundle = assetbundle_load(file);
            if (bundle == IntPtr.Zero)
                return false;

            var ret = assetbundle_check(bundle);
            assetbundle_destory(bundle);
            return ret;
        }

        public static bool Diff(string appDataDir, string fromAssetbundle, string toAssetbundle, string diff)
        {
            return assetbundle_diff(appDataDir, fromAssetbundle, toAssetbundle, diff) == 0;
        }

        public static bool Merge(string appDataDir, string fromAssetbundle, string toAssetbundle, string diff)
        {
            AppDataDir = appDataDir;
            return assetbundle_merge(ReadFile, IntPtr.Zero, fromAssetbundle, toAssetbundle, diff) == 0;
        }

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
        
        [DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "assetbundle_destory")]
        static extern void assetbundle_destory(IntPtr bundle);
        
        [DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "assetbundle_diff")]
        static extern int assetbundle_diff(string appDataDir, string fromAssetbundle, string toAssetbundle, string diff);

        delegate bool ReadFileCallback(IntPtr buffer, string fileName, uint offset, uint length, IntPtr userdata);
        
        [DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "assetbundle_merge")]
        static extern int assetbundle_merge(ReadFileCallback callback, IntPtr userdata, string fromAssetbundle, string toAssetbundle, string diff);
    }
}
