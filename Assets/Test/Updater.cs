using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;

namespace dpull
{
    public class Updater : MonoBehaviour
	{	
		public const string Diff = "diff.asset";
		public const string DiffZip = "diff.rtttl"; // for Android http://ponystyle.com/blog/2010/03/26/dealing-with-asset-compression-in-android-apps/
		private string DebugMessage = string.Empty;
		private string ProgressMessage = string.Empty; 
        private Progress CurProgress = null;
        private Rect WindowRect = new Rect(Screen.width / 4, Screen.height / 4, Screen.width / 2, Screen.height / 2); 
		private bool UpdateSucess;

		string GetAppDataDir()
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

		IEnumerator Start()
        {
			if (Application.platform != RuntimePlatform.Android && Application.platform != RuntimePlatform.IPhonePlayer)
				yield break;

			UpdateSucess = false;

			// Real environment, you need to download this file
			var diffZip = Path.Combine(Application.streamingAssetsPath, DiffZip);
			Debug.Log("DiffZip:" + diffZip);

			CurProgress = new UnzipProgress(diffZip, Application.persistentDataPath);
			yield return StartCoroutine(ProcessProgress(CurProgress));

			if (CurProgress.CurState != Progress.State.Succeed)
			{
				DebugMessage = "Unzip failed." + CurProgress.GetDebug();
				yield break;
			}

			var diff = Path.Combine(Application.persistentDataPath, Diff);
			Debug.Log("Diff:" + diff);

			var appDataDir = GetAppDataDir();
			Debug.Log("AppDataDir:" + appDataDir);
			
			var update = Path.Combine(Application.persistentDataPath, "update.asset");
			var ret = AssetBundleParser.Merge(appDataDir, null, update, diff);
			if (!ret)
			{
				DebugMessage = "Merge failed.";
				yield break;
			}

			AssetBundle.CreateFromFile(update);
			UpdateSucess = true;
		}

        void OnGUI()
        {
			if (UpdateSucess)
			{
				if (GUILayout.Button("Enter Level 2"))
				{
					Application.LoadLevel("Level2");
				}
				return;
			}

            WindowRect = GUILayout.Window(0, WindowRect, (windowId)=>{
                GUILayout.Label(ProgressMessage);
                
                if (CurProgress != null)
                {
                    var progress = 100 * CurProgress.ProgressCurValue / CurProgress.ProgressTotalValue;
                    GUILayout.HorizontalScrollbar((float)progress, 0f, 0f, 100f);  
                    GUILayout.Label(progress.ToString() + "%");
                    GUILayout.Label(CurProgress.CurState.ToString());
                }
                GUILayout.Label(DebugMessage);
            }, "Updater");
        }

        IEnumerator ProcessProgress(Progress curProgress)
        {
            while (true)
            {
				curProgress.Update();
				if (curProgress.CurState != Progress.State.Uncompleted)
					yield break;
                yield return null;
            }
        }
	}
}
