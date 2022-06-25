using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Serialization;


[System.Serializable]
public class InteractStorageList:ListIter<InteractStored>
{
	public void Init(string prop, int defaultValue = 0)
	{
		for (int i = 0; i < Count; i++)
		{
			if (items[i].key == prop)
				return;
		}
		items.Add(new InteractStored() { key = prop, value = defaultValue, svalue = defaultValue.ToString() });
	}

	public void InitStr(string prop, string defaultValue)
	{
		for (int i = 0; i < Count; i++)
		{
			if (items[i].key == prop)
				return;
		}
		items.Add(new InteractStored() { key = prop, value = 0, svalue = defaultValue.ToString() });
	}
}

[System.Serializable]
public class TransformsList : IKeyValueBase{
	public string key;
	public List<Transform> transforms;

	public string Key => key;
	public object Value => transforms;

	public int ClearNulls(){
		int nullCount = 0;
		for (int i = transforms.Count - 1; i >= 0 ; i--)
		{
			if (transforms[i] == null){
				transforms.RemoveAt(i);
				nullCount ++;
			}
		}
		return nullCount;
	}
}

public static class FindHelper{
	
	public static TransformsList Find(this List<TransformsList> transforms, string key){
		foreach (var item in transforms)
		{
			if(item.key == key)
				return item;
		}
		var x = new TransformsList(){key = key, transforms = new List<Transform>()};
		transforms.Add(x);
		return x;
	}
}

[System.Serializable]
public class KeyTransform : IKeyValueBase{
	public string key;
	public Transform prefab;

	public string Key => key;
	public object Value => prefab;

	public void Serialize(string path){
		NodeManager.Node(NodeManager.Path(path, "key"), key);
		NodeManager.Node(NodeManager.Path(path, "prefab"), prefab.name);
	}
}

[System.Serializable]
public class KeyValue<T> : IKeyValueBase{
	public string key;
	public T script;

	public string Key => key;
	public object Value => script;
}


[System.Serializable]
public class InteractTransformList:ListIter<KeyTransform> {
	
	public void Init(string prop, Transform defaultValue){
		for (int i = 0; i < Count; i++){
			if (items[i].key == prop)
				return;
		}
		items.Add(new KeyTransform(){ key = prop, prefab = defaultValue});
	}
	
	/// reserve if missing
	public KeyTransform GetReserved(string index, Transform defaultValue = null){
		for (int i = 0; i < Count; i++){
			if (items[i].key == index)
				return items[i];
		}
		items.Add(new KeyTransform(){ key = index, prefab = defaultValue});
		return items[items.Count-1];
	}
	
}

/*[System.Serializable]
public class InteractList<T>{
	[SerializeField] List<KeyValue<T>> items = new List<KeyValue<T>>();
	public int Count => items.Count;
	public KeyValue<T> this[string index]{ 
		get {
			for (int i = 0; i < Count; i++){
				if (items[i].key == index)
					return items[i];
			}
			return null;
		}
		set{
			for (int i = 0; i < Count; i++){
				if (items[i].key == index)
					items[i] = value;
			}
		}
	}
	
	public void Init(string prop, T defaultValue){
		for (int i = 0; i < Count; i++){
			if (items[i].key == prop)
				return;
		}
		items.Add(new KeyValue<T>(){ key = prop, script = defaultValue});
	}
}*/


[System.Serializable]
public class KeyScript : IKeyValueBase{
	public string key;
	public Component script;

	public string Key => key;
	public object Value => script;
}

public interface IKeyValueBase{
	string Key { get; } // public string Key => key;
	object Value { get; } // public object Value => value;
}

[System.Serializable]
public class ListIter<T> where T: IKeyValueBase{
	[SerializeField] protected List<T> items = new List<T>();
	public int Count => items.Count;
	public T this[string index]{
		get {
			for (int i = 0; i < Count; i++){
				if (items[i].Key == index)
					return items[i];
			}
			return default;
		}
		set{
			for (int i = 0; i < Count; i++){
				if (items[i].Key == index)
					items[i] = value;
			}
		}
	}
}

[System.Serializable]
public class InteractScriptList:ListIter<KeyScript> {
	
	public void Init(string prop, Component defaultValue){
		for (int i = 0; i < Count; i++){
			if (items[i].key == prop)
				return;
		}
		items.Add(new KeyScript(){ key = prop, script = defaultValue});
	}
}

[CreateAssetMenu(menuName = "Interact/Prefabs")]
public class InteractPrefabs:ScriptableObject{
	public List<KeyTransform> items = new List<KeyTransform>();
	
	public Transform FindPrefab(string key){
		for (int i = 0; i< items.Count; i++){
			if (items[i].key == key)
				return items[i].prefab;
        }
		return null;
	}
	
	public Color FindColor(string key){
		var pref = FindPrefab(key);
		if(pref == null){
			Debug.Log($"Prefab not found at key '{key}'", this);
		}
		return pref.GetComponent<SpriteRenderer>().color;
	}
	
	public void Serialize(string path){
		var listPath = NodeManager.Path(path, "items");
		for (int i = 0; i < items.Count; i++){
			var pathI = NodeManager.Path(listPath, i);
			items[i].Serialize(pathI);
		}
	}
}