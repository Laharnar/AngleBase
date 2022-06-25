
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Node{
	Dictionary<string, object> dict = new Dictionary<string, object>();
	
	public Node(string name){
		WriteAttrib("name", name);
	}
	
	public void WriteAttrib(string attrib, object value){
		dict.Add(attrib, value);
	}
}

public class NodeManager{

	static Dictionary<string, Node> dict = new Dictionary<string, Node>();
	
	public static void Reset(){
		dict.Clear();
	}
	
	public static Node Node(string path, string name){
		Node node = new Node(name);
		dict.Add(path, node);
		return node;
	}
	
	public static Node Node(string path, int id, object value){
		Node node = new Node($"[{id}]");
		node.WriteAttrib("id", value);
		dict.Add(path, node);
		return node;
	}
	
	public static string Path(string path, string current){
		// item/item2
		if (path[path.Length-1] == '/')
			return $"{path}{current}";
		return $"{path}/{current}";
	}
	
	public static string Path(string path, int id){
		// item/item2
		if (path[path.Length-1] == '/')
			return $"{path}[{id}]";
		return $"{path}/[{id}]";
	}
	
	public static void List<T>(string path, string listName, List<T> objects){
		var listPath = NodeManager.Path(path, listName);
		for (int i = 0; i < objects.Count; i++){
			var pathI = NodeManager.Path(listPath, i);
			NodeManager.Node(pathI, i, objects[i]);
		}
	}
}