using UnityEditor;

public class NodeManagerWindow : EditorWindow
{
    [MenuItem("DEV/NodeManager")]
    static void Init()
    {
        // Get existing open window or if none, make a new one:
        NodeManagerWindow window = (NodeManagerWindow)GetWindow(typeof(NodeManagerWindow));
        window.Show();
    }

    void OnGUI()
    {
        EditorGUILayout.LabelField("TEST");
    }
}
