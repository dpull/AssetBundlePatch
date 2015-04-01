using UnityEngine;
using System.Collections;
using UnityEditor;

namespace dpull
{
	class UIAdapterEditor : EditorWindow
	{
		[MenuItem("XStudio/Tools/UI Adapter")]
		public static void UIAdapter()
		{
			EditorWindow.GetWindow<UIAdapterEditor>(false, "UI Adapter", true).Show();
		}
		
		UIRoot.Scaling Scaling;
		int ManualWidth = 1280;
		int ManualHeight = 720;
		int MinimumHeight = 320;
		int MaximumHeight = 1536;
		bool FitWidth = false;
		bool FitHeight = true;
		bool AdjustByDPI = false;
		bool ShrinkPortraitUI = false;
		
		void AddLine(string name, string width, string height, string widthScale, string heightScale, string widthView, string heightView, string desc)
		{
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(name, GUILayout.Width(100f));
			EditorGUILayout.LabelField(width, GUILayout.Width(60f));
			EditorGUILayout.LabelField(height, GUILayout.Width(60f));
			EditorGUILayout.LabelField(widthScale, GUILayout.Width(60f));
			EditorGUILayout.LabelField(heightScale, GUILayout.Width(60f));
			EditorGUILayout.LabelField(widthView, GUILayout.Width(60f));
			EditorGUILayout.LabelField(heightView, GUILayout.Width(60f));
			EditorGUILayout.LabelField(desc, GUILayout.Width(100));
			EditorGUILayout.EndHorizontal();
		}
		
		void AddLine(string name, int width, int height)
		{
			if (Scaling == UIRoot.Scaling.Flexible)
			{
				int manualWidth = width;
				int manualHeight = height;
				
				int heightScale = activeHeight(new Vector2(width, height));
				int widthScale = Mathf.RoundToInt((float)manualWidth * heightScale / manualHeight);
				AddLine(name, width.ToString(), height.ToString(), widthScale.ToString(), heightScale.ToString(), "", "", ((float)height / heightScale).ToString());
			}
			else
			{
				int heightScale = activeHeight(new Vector2(width, height));
				int widthScale = Mathf.RoundToInt((float)ManualWidth * heightScale / ManualHeight);
				int widthView = Mathf.RoundToInt((float)width * heightScale / height);
				int heightView = heightScale;
				
				AddLine(name, width.ToString(), height.ToString(), widthScale.ToString(), heightScale.ToString(), widthView.ToString(), heightView.ToString(), ((float)height / heightScale).ToString());
			}
			
		}
		
		void OnGUI()
		{
			Scaling = (UIRoot.Scaling)EditorGUILayout.EnumPopup("Scaling Style", Scaling);
			
			if (Scaling == UIRoot.Scaling.Flexible)
			{
				MinimumHeight = EditorGUILayout.IntField("Minimum Height", MinimumHeight);
				MaximumHeight = EditorGUILayout.IntField("Maximum Height", MaximumHeight);
				ShrinkPortraitUI = EditorGUILayout.Toggle("Shrink Portrait UI", ShrinkPortraitUI);
				AdjustByDPI = EditorGUILayout.Toggle("Adjust by DPI", AdjustByDPI);
				
				EditorGUILayout.LabelField("结果按手机实际显示，非编辑器预览显示");
			}
			else
			{
				GUILayout.BeginHorizontal();
				ManualWidth = EditorGUILayout.IntField("Content Width", ManualWidth, GUILayout.Width(260f));
				FitWidth = EditorGUILayout.Toggle("Fit", FitWidth);
				GUILayout.EndHorizontal();
				
				GUILayout.BeginHorizontal();
				ManualHeight = EditorGUILayout.IntField("Content Height", ManualHeight, GUILayout.Width(260f));
				FitHeight = EditorGUILayout.Toggle("Fit", FitHeight);
				GUILayout.EndHorizontal();			
			}
			
			EditorGUILayout.Space();
			EditorGUILayout.Space();
			EditorGUILayout.Space();
			EditorGUILayout.BeginVertical();
			AddLine("手机", "像素长", "像素宽", "UI缩放长", "UI缩放宽", "UI可视长", "UI可视宽", "缩放");
			AddLine("iPhone4", 960, 640);
			AddLine("iPhone5", 1136, 640);
			AddLine("iPhone6", 1334, 750);
			AddLine("iPhone6 plus", 1920, 1080);
			AddLine("iPad mini", 1024, 768);
			AddLine("iPad air", 2048, 1536);
			
			AddLine("Android1", 1280, 720);
			AddLine("Android2", 1920, 1080);
			
			EditorGUILayout.EndVertical();
		}
		
		UIRoot.Constraint constraint
		{
			get
			{
				if (FitWidth)
				{
					if (FitHeight) 
						return UIRoot.Constraint.Fit;
					return UIRoot.Constraint.FitWidth;
				}
				else if (FitHeight) 
					return UIRoot.Constraint.FitHeight;
				return UIRoot.Constraint.Fill;
			}
		}
		
		int activeHeight(Vector2 screen)
		{
			if (Scaling == UIRoot.Scaling.Flexible)
			{
				float aspect = screen.x / screen.y;
				
				if (screen.y < MinimumHeight)
				{
					screen.y = MinimumHeight;
					screen.x = screen.y * aspect;
				}
				else if (screen.y > MaximumHeight)
				{
					screen.y = MaximumHeight;
					screen.x = screen.y * aspect;
				}
				
				// Portrait mode uses the maximum of width or height to shrink the UI
				int height = Mathf.RoundToInt((ShrinkPortraitUI && screen.y > screen.x) ? screen.y / aspect : screen.y);
				
				// Adjust the final value by the DPI setting
				return AdjustByDPI ? NGUIMath.AdjustByDPI(height) : height;
			}
			else
			{
				var cons = constraint;
				if (cons == UIRoot.Constraint.FitHeight)
					return ManualHeight;
				
				float aspect = screen.x / screen.y;
				float initialAspect = (float)ManualWidth / ManualHeight;
				
				switch (cons)
				{
				case UIRoot.Constraint.FitWidth:
				{
					return Mathf.RoundToInt(ManualWidth / aspect);
				}
					
				case UIRoot.Constraint.Fit:
				{
					return (initialAspect > aspect) ? Mathf.RoundToInt(ManualWidth / aspect) : ManualHeight;
				}
				case UIRoot.Constraint.Fill:
				{
					return (initialAspect < aspect) ? Mathf.RoundToInt(ManualWidth / aspect) : ManualHeight;
				}
				}
				return ManualHeight;
			}
		}
	}
}
