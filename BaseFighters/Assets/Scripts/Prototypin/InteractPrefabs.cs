using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

[System.Serializable]
public class TransformsList{
	public string key;
	public List<Transform> transforms;

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
public class KeyTransform{
	public string key;
	public Transform prefab;

}


[System.Serializable]
public class InteractTransformList{
	[SerializeField] List<KeyTransform> items = new List<KeyTransform>();
	public int Count => items.Count;
	public KeyTransform this[string index]{ 
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
	
	public void Init(string prop, Transform defaultValue){
		for (int i = 0; i < Count; i++){
			if (items[i].key == prop)
				return;
		}
		items.Add(new KeyTransform(){ key = prop, prefab = defaultValue});
	}
}

[System.Serializable]
public class KeyValue<T>{
	public string key;
	public T script;
}

[System.Serializable]
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
}


[System.Serializable]
public class KeyScript{
	public string key;
	public Component script;
}

[System.Serializable]
public class InteractScriptList{
	[SerializeField] List<KeyScript> items = new List<KeyScript>();
	public int Count => items.Count;
	public KeyScript this[string index]{ 
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
	
	public void Init(string prop, Component defaultValue){
		for (int i = 0; i < Count; i++){
			if (items[i].key == prop)
				return;
		}
		items.Add(new KeyScript(){ key = prop, script = defaultValue});
	}
}

[CreateAssetMenu]
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
}