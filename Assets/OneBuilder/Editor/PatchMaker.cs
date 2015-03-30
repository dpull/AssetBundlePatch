using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections.Generic;

namespace dpull
{
	public static class PatchMaker
	{
		public static void MakePatch()
		{
			string[] args = System.Environment.GetCommandLineArgs();
			var target = (BuildTarget)Enum.Parse(typeof(BuildTarget), args[1]);
			var baseSvnRevision = int.Parse(args[3]);
			PerformPatchMaker(target, args[2], baseSvnRevision);
		}
		
		public static void Diff()
		{
			string[] args = System.Environment.GetCommandLineArgs();
			AssetBundleParser.Diff("", args[1], args[2], args[3]);
		}
		
		public static void Merge()
		{
			string[] args = System.Environment.GetCommandLineArgs();
			AssetBundleParser.Merge("", args[1], args[2], args[3]);
		}
		
		public static void PerformPatchMaker(BuildTarget target, string output, int baseSvnRevision)
		{
			var resources = "Assets/Resources";
			var patchFiles = ResRevisionChecker.CheckModifiedFiles(resources, baseSvnRevision, new string[]{".cs", ".js"});
			if (patchFiles.Length == 0)
				return;

			var assets = new List<UnityEngine.Object>();
			var names = new List<string>();

			var resourcesStrLen = resources.Length;
			if (!PathEx.EndWithDirectorySeparatorChar(resources))
				resourcesStrLen++;

			foreach (var file in patchFiles)
			{
				var asset = AssetDatabase.LoadMainAssetAtPath(file);
				assets.Add(asset);
					
				var ext = Path.GetExtension(file);
				var name = file.Substring(resourcesStrLen, file.Length - resourcesStrLen - ext.Length);
				names.Add(name);
			}

			BuildPipeline.BuildAssetBundleExplicitAssetNames(
				assets.ToArray(), 
				names.ToArray(), 
				output, 			                                                 
				BuildAssetBundleOptions.CollectDependencies | BuildAssetBundleOptions.CompleteAssets| BuildAssetBundleOptions.UncompressedAssetBundle | BuildAssetBundleOptions.DisableWriteTypeTree, 
				target);
		}
	}
	
}

