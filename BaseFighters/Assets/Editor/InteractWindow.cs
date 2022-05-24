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

    // Add menu named "My Window" to the Window menu
    [MenuItem("DEV/Interact")]
    static void Init()
    {
        // Get existing open window or if none, make a new one:
        InteractWindow window = (InteractWindow)EditorWindow.GetWindow(typeof(InteractWindow));
        window.Show();
    }

    void DrawItems(List<string> list, string label = ""){
        if(label != "" && list.Count > 0)
            EditorGUILayout.LabelField(label);
        EditorGUI.indentLevel ++;
        for (int i = 0; i < list.Count; i++)
            list[i] = EditorGUILayout.TextField(list[i]);
        EditorGUI.indentLevel --;
    }

    void OnGUI()
    {
        if(GUILayout.Button("refresh |"+time) || time + 5 < Time.time){
            time = (int)Time.time;
            objs.Clear();
            objs.AddRange(GameObject.FindObjectsOfType<InteractState>());
        }
        scroll = EditorGUILayout.BeginScrollView(scroll);

        GUILayout.Label("roots");
        int count = 0;
        for(int i = 0; i < objs.Count; i++)
        {
            if(objs[i] == null)continue;
            if(objs[i].transform.parent == null){
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.ObjectField(objs[i], typeof(InteractState), true);
                foldouts[i] = EditorGUILayout.Toggle(foldouts[i]);
                EditorGUILayout.EndHorizontal();

                if(!foldouts[i])
                    continue;
                objs[i].InitAwake();
                EditorGUI.indentLevel ++;
                for (int j = 0; j < objs[i].module.triggerRules.Count; j++)
                {
                    var rule = objs[i].module.triggerRules[j];
                    EditorGUILayout.ObjectField(rule.rules, typeof(InteractRuleset), false);
                    EditorGUILayout.LabelField(rule.trigger);
                    EditorGUI.indentLevel ++;
                    foreach (var action in rule.rules.interactions)
                    {
                        if(action.note!= "")
                            EditorGUILayout.LabelField($"<{action.note}");
                        DrawItems(action.action.conditions, "if");
                        DrawItems(action.action.codes, "do");
                        DrawItems(action.action.elseCodes, "elsedo");
                    }
                    EditorGUI.indentLevel --;
                }
                count++;
                EditorGUI.indentLevel --;
            }
            if(count%3 == 0 && count > 0)
                EditorGUILayout.Space();
        }
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        count = 0;
        GUILayout.Label("children");
        for(int i = 0; i < objs.Count; i++)
        {
            if(objs[i] == null)continue;
            if(objs[i].transform.parent != null){
                count++;
                EditorGUILayout.ObjectField(objs[i], typeof(InteractState));
            }
            if(count%3 == 0 && count > 0)
                EditorGUILayout.Space();
        }

        EditorGUILayout.EndScrollView();
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