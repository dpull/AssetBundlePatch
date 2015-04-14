using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection;

namespace dpull
{
    public static class EditorGUITools
    {
        static public bool DrawHeader (string text)
        {
            return DrawHeader(text, null, false, false);
        }

        static public bool DrawHeader (string text, string key, bool forceOn, bool minimalistic)
        {
            key = (key == null) ? text : key;
            bool state = EditorPrefs.GetBool(key, true);
            
            if (!minimalistic) GUILayout.Space(3f);
            if (!forceOn && !state) GUI.backgroundColor = new Color(0.8f, 0.8f, 0.8f);
            GUILayout.BeginHorizontal();
            GUI.changed = false;
            
            if (minimalistic)
            {
                if (state) text = "\u25BC" + (char)0x200a + text;
                else text = "\u25BA" + (char)0x200a + text;
                
                GUILayout.BeginHorizontal();
                GUI.contentColor = EditorGUIUtility.isProSkin ? new Color(1f, 1f, 1f, 0.7f) : new Color(0f, 0f, 0f, 0.7f);
                if (!GUILayout.Toggle(true, text, "PreToolbar2", GUILayout.MinWidth(20f))) state = !state;
                GUI.contentColor = Color.white;
                GUILayout.EndHorizontal();
            }
            else
            {
                text = "<b><size=11>" + text + "</size></b>";
                if (state) text = "\u25BC " + text;
                else text = "\u25BA " + text;
                if (!GUILayout.Toggle(true, text, "dragtab", GUILayout.MinWidth(20f))) state = !state;
            }
            
            if (GUI.changed) 
                EditorPrefs.SetBool(key, state);
            
            if (!minimalistic) 
                GUILayout.Space(2f);
            GUILayout.EndHorizontal();
            GUI.backgroundColor = Color.white;
            if (!forceOn && !state) 
                GUILayout.Space(3f);
            return state;
        }
        
        static public void BeginContents ()
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.BeginHorizontal("AS TextArea", GUILayout.MinHeight(10f));
            GUILayout.BeginVertical();
            GUILayout.Space(2f);
        }
        
        static public void EndContents ()
        {
            GUILayout.Space(3f);
            GUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(3f);
            GUILayout.EndHorizontal();
            GUILayout.Space(3f);
        }
    }
}

