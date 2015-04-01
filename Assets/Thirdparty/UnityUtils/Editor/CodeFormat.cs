using UnityEngine;
using System.Collections;
using UnityEditor;

namespace dpull
{
	[ExecuteInEditMode]
	public static class CodeFormat
	{
		[MenuItem("XStudio/Tools/Code Format")]
		public static void Process()
		{
			var srcs = System.IO.Directory.GetFiles("Assets/Component", "*.cs", System.IO.SearchOption.AllDirectories);
			var utf8Encoding = new System.Text.UTF8Encoding(true);
			
			foreach(var src in srcs)
			{
				var auto = System.IO.File.ReadAllText(src);
				auto = auto.Replace("\r\n", "\n");
				System.IO.File.WriteAllText(src, auto, utf8Encoding);
			}
			Debug.Log("Code format finish");
		}
	}
}