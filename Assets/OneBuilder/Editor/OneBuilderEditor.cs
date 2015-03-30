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

		string BuildPlayer(BuildTarget target, params string[] levels)
		{
			var output = Path.Combine(OutputDir, target.ToString());
			BuildPipeline.BuildPlayer(levels, output, target, BuildOptions.Development | BuildOptions.AllowDebugging);
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
			var size = (double)GetFileSize(path);
			int count = 0;
			while (size > 1024d && count != 3) 
			{
				size /= 1024;
				count++;
			}

			string unit = string.Empty;
			switch (count)
			{
			case 0:
				unit = "B";
				break;

			case 1:
				unit = "KB";
				break;

			case 2:
				unit = "MB";
				break;

			case 3:
				unit = "GB";
				break;
			}

			return string.Format("{0:F}{1}", size, unit);
		}
		
		void OnGUI()
		{
			if (!string.IsNullOrEmpty(DebugInfo))
			{
				GUILayout.TextArea(DebugInfo);
			}

			GUILayout.Space(10);

			if (GUILayout.Button("Test iPhone"))
			{
				var oldTarget = EditorUserBuildSettings.activeBuildTarget;
				DirectoryEx.CreateDirectory(OutputDir);
				var app = BuildPlayer(BuildTarget.iPhone, "Assets/Level1.unity");
				var appDataDir = Path.Combine(app, "Data");
				var assetbundle = BuildStreamedSceneAssetBundle(BuildTarget.iPhone, "Assets/Level2.unity");

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


		}
	}
}