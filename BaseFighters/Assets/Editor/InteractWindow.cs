using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

public class InteractWindow:EditorWindow{
    public bool refresh = false;
    public List<InteractState> objs = new List<InteractState>();

    Vector2 scroll;
    int time;

    // Add menu named "My Window" to the Window menu
    [MenuItem("DEV/Interact")]
    static void Init()
    {
        // Get existing open window or if none, make a new one:
        InteractWindow window = (InteractWindow)EditorWindow.GetWindow(typeof(InteractWindow));
        window.Show();
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
                EditorGUILayout.ObjectField(objs[i], typeof(InteractState));
                count ++;
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