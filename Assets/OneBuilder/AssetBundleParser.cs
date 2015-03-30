using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System;
using System.IO;

namespace dpull
{
    public class AssetBundleParser  
    {
		string AppDataDir;
		
		[AOT.MonoPInvokeCallback(typeof(ReadFileCallback))]
		int ReadFile(IntPtr buffer, IntPtr fileNamePtr, uint offset, uint length, IntPtr userdata) 
		{
			try
			{
				var fileName = Marshal.PtrToStringAnsi(fileNamePtr);
				var fullPath = Path.Combine(AppDataDir, fileName);
				Debug.Log("Merge from file:" + fullPath);
				
				using (var stream = FileEx.OpenRead(fullPath))
				{
					if (stream == null || stream.Length < offset + length) 
					{
						Debug.LogError(fullPath);
						return 0;
					}
					
					var data = new byte[length];
					stream.Seek(offset, SeekOrigin.Begin);
					
					var left = (int)length;
					while (left > 0) 
					{
						var ret = stream.Read(data, (int)length - left, left);
						if (ret <= 0)
							return 0;
						
						left -= ret;
					}
					
					Marshal.Copy(data, 0, buffer, data.Length);
					return 1;
				}
			}
			catch(System.Exception e)
			{
				Debug.LogException(e);
				return 0;
			} 
		}

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
			var parser = new AssetBundleParser();
			parser.AppDataDir = appDataDir;
			return assetbundle_merge(parser.ReadFile, IntPtr.Zero, fromAssetbundle, toAssetbundle, diff) == 0;
		}
		
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

		delegate int ReadFileCallback(IntPtr buffer, IntPtr fileNamePtr, uint offset, uint length, IntPtr userdata);
        
		[DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "assetbundle_merge")]
		static extern int assetbundle_merge(ReadFileCallback callback, IntPtr userdata, string fromAssetbundle, string toAssetbundle, string diff);
	}
}
