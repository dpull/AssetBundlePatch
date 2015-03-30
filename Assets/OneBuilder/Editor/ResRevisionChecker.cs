using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace dpull
{
	public class ResRevisionChecker
	{
		public static string SvnPath = "svn";

		string WorkingDirectory = string.Empty;
		int CompareRevision = 0;
		List<string> InvalidExts = null;
		Dictionary<string, int> FileRevisions = new Dictionary<string, int>();
		List<string> RecursivedFiles = new List<string>();
		List<string> ModifiedFiles = new List<string>();

		/*
		 * 返回值: svn版本大于指定版本的所有文件的文件路径
		 * @param: svn地址
		 * @param: 以workingDirectory为根节点遍历检查其中的assets
		 * @param: 用作对比的版本号
		 * @param: 被依赖的asset的后缀过滤。比如invalidExts=new List<string>{".cs", ".mat"}表示如果某个asset依赖了某个被改动的.cs或.mat文件，也不会因此将这个asset视为需要更新
		 */
		public static string[] CheckModifiedFiles(string workingDirectory, int compareRevision, string[] invalidExts)
		{
			ResRevisionChecker checker = new ResRevisionChecker(workingDirectory, compareRevision, invalidExts);
			return checker.GetModifiedFiles();
		}
		
		public ResRevisionChecker(string workingDirectory, int compareRevision, string[] invalidExts)
		{
			WorkingDirectory = workingDirectory;
			CompareRevision = compareRevision;
			
			InvalidExts = new List<string>(invalidExts);
			for (int i = 0; i < InvalidExts.Count; ++i)
			{
				string s = InvalidExts[i] as string;
				InvalidExts[i] = s.ToLower();
			}

			List<string> allFile = GetAll();
			foreach(string filepath in allFile)
			{
				if (ProcessCheckModified(filepath, filepath))
					ModifiedFiles.Add(filepath);
			}
		}
		
		public string[] GetModifiedFiles()
		{
			return ModifiedFiles.ToArray();
		}
		
		List<string> GetAll()
		{
			List<string> allAssets = new List<string>();

			string curDir = Path.GetFullPath(System.Environment.CurrentDirectory);

			string[] allFile = Directory.GetFiles(WorkingDirectory, "*", SearchOption.AllDirectories);

			foreach(string file in allFile)
			{
				string fullpath = Path.GetFullPath(file);
				
				//这里的处理是为了让fileList里的路径格式与AssetDatabase里的路径格式 保持一致
				string relativedPath = fullpath.Substring(curDir.Length + 1);
				relativedPath = relativedPath.Replace("\\", "/");
				
				var asset = AssetDatabase.LoadMainAssetAtPath(relativedPath);
				if (asset != null)
					allAssets.Add(relativedPath);
			}

			return allAssets;
		}
		
		//fileRevision > compareRevision则返回true
		bool ProcessCheckModified(string root, string filepath)
		{
			if (RecursivedFiles.Contains(filepath))
				return false;

			RecursivedFiles.Add(filepath);

			if (FileRevisions.ContainsKey(filepath))
				return (FileRevisions[filepath] > CompareRevision);

			string stdout = null;
			while (true)
			{
				try
				{
					System.Diagnostics.Process proc = new System.Diagnostics.Process();
					proc.StartInfo.FileName = SvnPath;
					proc.StartInfo.Arguments = string.Format("info {0}", filepath);
					proc.StartInfo.WorkingDirectory = System.Environment.CurrentDirectory;
					proc.StartInfo.CreateNoWindow = true;
					proc.StartInfo.UseShellExecute = false;
					proc.StartInfo.RedirectStandardOutput = true;
					proc.Start();
					
					StreamReader reader = proc.StandardOutput;
					stdout = reader.ReadToEnd();
					break;
				}
				catch(System.ComponentModel.Win32Exception e)
				{
					Debug.LogException(e);
				}
			}
			
			MatchCollection matches = Regex.Matches(stdout, "Last Changed Rev: (?<revision>\\d+)");
			foreach(Match match in matches)
			{
				Group group = match.Groups["revision"];
				if (group != null)
				{
					int revision = int.Parse(group.Value);
					FileRevisions.Add(filepath, revision);
					if (revision > CompareRevision)
						return true;
				}
			}
			
			string[] dependencies = AssetDatabase.GetDependencies(new string[]{filepath});
			foreach(string dependency in dependencies)
			{
				string ext = Path.GetExtension(dependency).ToLower();
				if (InvalidExts.Contains(ext))
					continue;

				if (ProcessCheckModified(root, dependency))
					return true;
			}
			
			return false;
		}
	}
}