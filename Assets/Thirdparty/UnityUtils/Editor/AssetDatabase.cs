using UnityEngine;
using System.Collections;
using UnityEditor;

namespace dpull
{
	[ExecuteInEditMode]
	public static class AssetDatabaseEx
	{
		static string[] GetBeDependencies(string path)
		{
			var ret = new System.Collections.Generic.List<string>();
			var assets = AssetDatabase.GetAllAssetPaths();
			foreach (var asset in assets)
			{
				var dependencies = AssetDatabase.GetDependencies(new string[]{asset});
				if (ArrayUtility.Contains(dependencies, path))
					ret.Add(asset);
			}
			return ret.ToArray();
		}

		[MenuItem("XStudio/Tools/Search Assets")]
		public static void Process()
		{
			var files = GetBeDependencies("Assets/Component/ClientUIEvent.cs");
			foreach (var file in files)
				Debug.Log(file);
		}

		static bool HasDependencies(string asset, string srcCodeFile)
		{
			var dependencies = AssetDatabase.GetDependencies(new string[]{asset});
			foreach (var dependencie in dependencies)
			{
				if (!dependencie.Contains(srcCodeFile))
					continue;
				return true;
			}
			return false;
		}

		static void Replace<T>(string srcCodeFile) where T : Component 
		{
			var assets = AssetDatabase.GetAllAssetPaths();
			foreach (var asset in assets)
			{
				if (!asset.StartsWith("Assets"))
					continue;

				var assetObj = AssetDatabase.LoadMainAssetAtPath(asset);
				if (assetObj == null)
					continue;

				if (PrefabUtility.GetPrefabType(assetObj) != PrefabType.Prefab)
					continue;

				if (!string.IsNullOrEmpty(srcCodeFile))
				{
					if (HasDependencies(asset, srcCodeFile))
						continue;
				}
				
				var go = GameObject.Instantiate(assetObj) as GameObject;
				if (go == null)
					continue;

				go.name = assetObj.name;
				var isChange = false;

				var children = go.GetComponentsInChildren<T>();
				foreach (var child in children)
				{
					var serializedObject = new SerializedObject(child);
					var sp = serializedObject.FindProperty("prefabFile");
					var target = sp.objectReferenceValue;
					if (target != null)
					{
						var path = AssetDatabase.GetAssetPath(target);
						var spPath = serializedObject.FindProperty("prefabPath");
						if (path.StartsWith("Assets/Resources/"))
						{
							spPath.stringValue = path.Substring("Assets/Resources/".Length);
							sp.objectReferenceValue = null;
							serializedObject.ApplyModifiedProperties();
							isChange = true;
						}
						else
						{
							Debug.Log(string.Format("asset:{0}.{1}, path{2}", asset, child.name, path));
						}
					}
				}

				if (isChange)
					PrefabUtility.ReplacePrefab(go, assetObj);
			}
		}
	}
}