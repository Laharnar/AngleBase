using System.Net.Http.Headers;
using System.IO;
using System.Runtime.CompilerServices;
using System.Data;
using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

public class InteractWindow:EditorWindow{
    public bool refresh = false;
    public List<InteractState> objs = new List<InteractState>();

    Vector2 scroll;
    int time;
    bool[] foldouts = new bool[300];
    GameObject manualRoot;
    string searchObj="";
    string searchRule="";
    string searchLine="";

    // Add menu named "My Window" to the Window menu
    [MenuItem("DEV/Interact")]
    static void Init()
    {
        // Get existing open window or if none, make a new one:
        InteractWindow window = (InteractWindow)GetWindow(typeof(InteractWindow));
        window.Show();
    }

    static bool ContainsSearch(List<string> list, string search){
        if(search != ""){
            for (int i = 0; i < list.Count; i++){
                if(list[i].ToLower().Contains(search)) 
                    return true;
            } 
        }
        return false;
    }

    void DrawItems(List<string> list, string label = ""){
        if(label != "" && list.Count > 0 )
            EditorGUILayout.LabelField(label);
        EditorGUI.indentLevel ++;
    
        for (int i = 0; i < list.Count; i++){
            list[i] = EditorGUILayout.TextField(list[i]);
        }
        EditorGUI.indentLevel --;
    }

    static void ClearField(string label, ref string value){
        EditorGUILayout.BeginHorizontal();
        if(GUILayout.Button("x", GUILayout.Width(26))){
            value = "";
        }
        value = EditorGUILayout.TextField(label, value).ToLower();
        EditorGUILayout.EndHorizontal();
    }

    void OnGUI()
    {
        manualRoot = (GameObject)EditorGUILayout.ObjectField(manualRoot, typeof(GameObject), true);
        if (GUILayout.Button("refresh |"+time) || time + 5 < Time.time){
            time = (int)Time.time;
            objs.Clear();
            if(manualRoot == null)
                objs.AddRange(FindObjectsOfType<InteractState>());
            else objs.AddRange(manualRoot.GetComponentsInChildren<InteractState>());
        }
        ClearField("object  *..", ref searchObj);
        ClearField("rule  *..", ref searchRule);
        ClearField("line  *", ref searchLine);

        scroll = EditorGUILayout.BeginScrollView(scroll);

        GUILayout.Label("roots");
        int count = 0;
		List<InteractState> filteredObjs = new List<InteractState>();
        for(int i = 0; i < objs.Count; i++)
        {
            var obj = objs[i];
            if(obj == null) continue;
            var tobj = obj.transform;
            if(!PrefixSearch(tobj.name, searchObj)) continue;
			filteredObjs.Add(obj);
		}
		
		bool ShowObjectLayers(InteractState obj, bool[] foldouts, int i){
			// obj + show
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.ObjectField(obj, typeof(InteractState), true);
			foldouts[i] = EditorGUILayout.Toggle(foldouts[i]);
			EditorGUILayout.EndHorizontal();

			if(!foldouts[i])
				return false;
			obj.ValidateComponents();
			EditorGUI.indentLevel ++;
			var layers = obj.module.EditorLayers();
			for (int k = 0; k < layers.Count; k++)
			{
				var layer = layers[k];
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField(layer.EditorLayer, GUILayout.Width(100));
				layer.EditorEnabled = EditorGUILayout.Toggle("", layer.EditorEnabled, GUILayout.Width(20));
				EditorGUILayout.EndHorizontal();
				
				for (int j = 0; j < layer.EditorTriggers.Count; j++)
				{
					var rule = layer.EditorTriggers[j];
					if (!PrefixSearch(rule.rules.name, searchRule)) continue;
					DrawRule(rule);
				}
			}
			EditorGUI.indentLevel --;
			return true;
		}
		
		for(int i = 0; i < filteredObjs.Count; i++)
        {
            var obj = filteredObjs[i];
			var tobj = obj.transform;
            if(tobj.parent == null || (manualRoot!= null && tobj.parent == manualRoot.transform)){
				if(!ShowObjectLayers(obj, foldouts, i)) continue;
                count++;
				if(count%3 == 0 && count > 0)
					EditorGUILayout.Space();
            }
        }
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        count = 0;
        GUILayout.Label("children");
        for(int i = 0; i < filteredObjs.Count; i++)
        {
			var obj = filteredObjs[i];
            if(obj == null) continue;
            var tobj = obj.transform;
            if(tobj.parent != null && (manualRoot == null || tobj.parent != manualRoot.transform)){
				if(!ShowObjectLayers(obj, foldouts, i)) continue;
				
                count++;
				if(count%3 == 0 && count > 0)
					EditorGUILayout.Space();
            }
        }

        EditorGUILayout.EndScrollView();
    }

    private void DrawRule(InteractTrigger rule)
    {
		
        EditorGUI.indentLevel++;
		EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(rule.trigger, GUILayout.Width(200));
        EditorGUILayout.ObjectField(rule.rules, typeof(InteractRuleset), false);
		EditorGUILayout.EndHorizontal();
        EditorGUI.indentLevel++;
        foreach (var action in rule.rules.interactions)
        {
            var draw = searchLine == ""
            || ContainsSearch(action.action.conditions, searchLine)
            || ContainsSearch(action.action.codes, searchLine)
            || ContainsSearch(action.action.elseCodes, searchLine);
            if (draw)
            {
				if (action.note != "")
					EditorGUILayout.LabelField($"# {action.note}");
                DrawItems(action.action.conditions, "if");
                DrawItems(action.action.codes, "do");
                DrawItems(action.action.elseCodes, "elsedo");
            }
        }
        EditorGUI.indentLevel--;
        EditorGUI.indentLevel--;
    }

    bool PrefixSearch(string longName, string search){
        return search == "" || longName.ToLower().StartsWith(search);
    }

    private void OnEnable()
    {
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }
 
    private void OnDisable()
    {
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
    }
 
    private void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        time = 0;
    }
}