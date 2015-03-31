using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace dpull
{
	class OneBuilderEditor : EditorWindow
	{
		string OutputDir = Path.Combine(Application.dataPath, "../Bin");
		string DebugInfo;

		[MenuItem("XStudio/Tools/One Builder")]
		public static void UIAdapter()
		{
			EditorWindow.GetWindow<OneBuilderEditor>(false, "One Builder", true).Show();
		}

		string BuildPlayer(BuildTarget target, BuildOptions options, params string[] levels)
		{
			var output = Path.Combine(OutputDir, target.ToString());
			DirectoryEx.DeleteDirectory(output);

			BuildPipeline.BuildPlayer(levels, output, target,  options);
			return output;
		}

		string BuildStreamedSceneAssetBundle(BuildTarget target, params string[] levels)
		{
			var output = Path.Combine(OutputDir, target.ToString() + ".asset");
			BuildPipeline.BuildStreamedSceneAssetBundle(levels, output, target, BuildOptions.UncompressedAssetBundle);
			return output;
		}

		long GetFileSize(string path)
		{
			var info = new FileInfo(path);
			return info.Length;
		}

		string GetFileSizeString(string path)
		{
			var units = new string[]{"B", "KB", "MB", "GB"};
			var size = (double)GetFileSize(path);
			foreach (var unit in units)
			{
				if (size < 1024d)
					return string.Format("{0:F}{1}", size, unit);
				size /= 1024;
			}
			return string.Format("{0:F}{1}", size, units[units.Length - 1]);
		}

		BuildTarget GetIOSBuildTarget()
		{
			#if UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6
			return BuildTarget.iPhone;
			#else
			return BuildTarget.iOS;
			#endif
		}

		void BuildiOS()
		{
			var oldTarget = EditorUserBuildSettings.activeBuildTarget;
			DirectoryEx.CreateDirectory(OutputDir);
			
			var app = BuildPlayer(GetIOSBuildTarget(), BuildOptions.None, "Assets/Level1.unity");
			
			var appDataDir = Path.Combine(app, "Data");
			var assetbundle = BuildStreamedSceneAssetBundle(GetIOSBuildTarget(), "Assets/Level2.unity");
			
			EditorUserBuildSettings.SwitchActiveBuildTarget(oldTarget);
			
			var diff = Path.Combine(OutputDir, Updater.Diff);
			var ret = AssetBundleParser.Diff(appDataDir, null, assetbundle, diff);
			if (!ret)
			{
				Debug.LogError("AssetBundleParser.Diff failed!!!!");
				return;
			}
			
			// Real environment, you need to place this file on http server
			var diffZip = Path.Combine(appDataDir, "Raw");
			diffZip = Path.Combine(diffZip, Updater.DiffZip);
			
			ZipFile.CreateFromDirectory(new string[]{diff}, diffZip);
			
			var sb = new StringBuilder();
			sb.AppendLine(DateTime.Now.ToString());
			sb.AppendLine(string.Format("Assetbundle size:{0}", GetFileSizeString(assetbundle)));
			sb.AppendLine(string.Format("Diff size:{0}", GetFileSizeString(diff)));
			sb.AppendLine(string.Format("Diff zip size:{0}", GetFileSizeString(diffZip)));
			DebugInfo = sb.ToString();
		}

		void BuildAndroid()
		{
			var oldTarget = EditorUserBuildSettings.activeBuildTarget;
			DirectoryEx.CreateDirectory(OutputDir);
			
			var app = BuildPlayer(BuildTarget.Android, BuildOptions.AcceptExternalModificationsToPlayer, "Assets/Level1.unity");
			var appDataDir = Path.Combine(app, "OneBuilder/assets/bin/Data");
			var assetbundle = BuildStreamedSceneAssetBundle(BuildTarget.Android, "Assets/Level2.unity");
			
			EditorUserBuildSettings.SwitchActiveBuildTarget(oldTarget);
			
			var diff = Path.Combine(OutputDir, Updater.Diff);
			var ret = AssetBundleParser.Diff(appDataDir, null, assetbundle, diff);
			if (!ret)
			{
				Debug.LogError("AssetBundleParser.Diff failed!!!!");
				return;
			}
			
			// Real environment, you need to place this file on http server
			var diffZip = Path.Combine(app, "OneBuilder/assets");
			diffZip = Path.Combine(diffZip, Updater.DiffZip);
			
			ZipFile.CreateFromDirectory(new string[]{diff}, diffZip);
			
			var sb = new StringBuilder();
			sb.AppendLine(DateTime.Now.ToString());
			sb.AppendLine(string.Format("Assetbundle size:{0}", GetFileSizeString(assetbundle)));
			sb.AppendLine(string.Format("Diff size:{0}", GetFileSizeString(diff)));
			sb.AppendLine(string.Format("Diff zip size:{0}", GetFileSizeString(diffZip)));
			DebugInfo = sb.ToString();
		}


		void OnGUI()
		{
			if (!string.IsNullOrEmpty(DebugInfo))
				GUILayout.TextArea(DebugInfo);

			GUILayout.Space(10);

			if (GUILayout.Button("Test iPhone"))
				BuildiOS();

			if (GUILayout.Button("Test Android"))
				BuildAndroid();
		}
	}
}