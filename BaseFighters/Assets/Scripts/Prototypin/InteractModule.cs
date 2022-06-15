using System.Diagnostics;
using System.Xml.XPath;
using System.Threading;
using System.Net.Http.Headers;
using System.Data;
using System.Runtime.CompilerServices;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
public class InteractLayer
{
    [SerializeField] internal string layer;
    [SerializeField] internal bool enabled;
    [SerializeField] internal List<InteractTrigger> triggers;
	private bool lastEnabled = false;
	
    public List<InteractTrigger> EditorTriggers => triggers;
    public bool EditorEnabled { get => enabled; set => enabled = value; }
    public string EditorLayer => layer;
	
	public InteractLayer(){
		lastEnabled = enabled;
	}
	
    public bool Matches(string layer)
    {
        return this.layer == layer;
    }

    public void Enabled(bool enabled)
    {
        this.enabled = enabled;
    }

    public void Add(InteractTrigger items)
    {
        if (items == null) return;
        triggers.Add(items);
    }

    public void AddList(List<InteractTrigger> items)
    {
        if (items == null) return;
        triggers.AddRange(items);
    }

    public void Clear()
    {
        triggers.Clear();
    }
	
	public bool HasToRefresh(){
		bool change = lastEnabled != enabled;
		lastEnabled = enabled;
		return change;
	}
	
	public InteractLayer Copy(){
		return new InteractLayer(){
			layer = this.layer,
			enabled = this.enabled,
			triggers = new List<InteractTrigger>(this.triggers),
			lastEnabled = this.lastEnabled
		};
	}
	
	public void Serialize(string path){
		path = NodeManager.Path(path, layer);
		var node = NodeManager.Node(path, layer);
		node.WriteAttrib("enabled", enabled);
		
		var triggersPath = NodeManager.Path(path, "triggers");
		for (var i = 0; i < triggers.Count; i ++){
			triggers[i].Serialize(triggersPath);
		}
	}
}

[System.Serializable]
public class InteractBox
{
    public List<InteractLayer> layers = new List<InteractLayer>();

    public void AddLayer(InteractLayer add){
        foreach (var item in layers)
        {
            if(item.Matches(add.layer)){
                item.AddList(new List<InteractTrigger>(add.triggers));
                return;
            }
        }
		layers.Add(add.Copy());
    }

    /// <summary>
    /// Adds this to other where layers match.
    /// </summary>
    /// <param name="other"></param>
    public void JoinInto(InteractBox other)
    {
        foreach (var item in layers)
        {
            if(item == null) continue;
            other.AddLayer(item);
        }
    }

    internal InteractLayer Get(string layerName)
    {
        foreach (var item in layers)
        {
            if (item.Matches(layerName))
                return item;
        }
        var created = new InteractLayer(){
            enabled = true,
            layer = layerName,
            triggers= new List<InteractTrigger>()
        };
        layers.Add(created);
        return created;
    }
	
	public void Serialize(string path){
		for (int i = 0; i < layers.Count; i++)
			layers[i].Serialize(NodeManager.Path(path, "boxes"));
	}
}

[System.Serializable]
public class InteractTrigger
{
    // trigger names: timed, tick, overlap(trigger collision), collision
    public string trigger;
    public InteractRuleset rules;
	
	public void Serialize(string path){
		var triggersPath = NodeManager.Path(path, trigger);
		var triggersNode = NodeManager.Node(triggersPath, trigger);
		triggersNode.WriteAttrib("rules", rules.name);
	}
}

public class InteractModule : MonoBehaviour
{
    [SerializeField] InteractBox layers;
    [SerializeField] List<InteractBoxPref> boxes;
    [Header("Start:Loaded to 'base', then cleared")]
    [SerializeField] List<InteractTrigger> triggerRules;
    [Header("Preferably obsolete")]
    [FormerlySerializedAs("rules")]
    [SerializeField] InteractRuleset overlapRules; // trigger rules
    [SerializeField] InteractRuleset timedRules;

	[Header("Realtime")]
    [SerializeField] public InteractBox activeLayers;
    public InteractBox RealtimeLayers => activeLayers;
	InteractState state;
	public bool editorRefresh;

    void Awake()
    {
		state = GetComponent<InteractState>();
        activeLayers = CreateBox();
    }
	
	void Update(){
		if((int)Time.time % (5+UnityEngine.Random.Range(-1,4)) == 0)
			activeLayers = CreateBox();
		if(editorRefresh){
			for (int i = 0; i < activeLayers.layers.Count; i++){
				if(activeLayers.layers[i].HasToRefresh()){
					state.StartReinitOnLayerUpdate();
				}
			}
			editorRefresh = false;
		}
	}

	public List<InteractLayer> EditorLayers()
	{
		List<InteractLayer> group = new List<InteractLayer>();
		group.AddRange(layers.layers);
		foreach (var item in boxes){
			if(item.box == null)
				continue;
            group.AddRange(item.box.layers);
		}
		group.Add(new InteractLayer(){
            enabled = true,
            layer = "base",
            triggers = new List<InteractTrigger>(triggerRules)
        });
		return group;	
	}
	
    /// <summary>
    /// Note that this is recreated for every call.
    /// </summary>
    public InteractBox CreateBox()
    {
        InteractBox temp = new InteractBox();
        foreach (var item in boxes){
			if(item.box == null)
				UnityEngine.Debug.LogError("Box is null", this);
            else item.box.JoinInto(temp);
		}
        // unique local load for current version.
        temp.Get("base").AddList(triggerRules);
        layers.JoinInto(temp);
        return temp;
    }

    public List<InteractRules> GetRules(string trigger){
        List<InteractRules> interactions = new List<InteractRules>();
        foreach (var item in activeLayers.layers){
            if(!item.enabled) continue;
            var triggers = item.triggers;
            for (int i = 0; i < triggers.Count; i++)
            {
                if(triggers[i].trigger == trigger && triggers[i].rules != null){
                    interactions.AddRange(triggers[i].rules.interactions);
                }
            }
        }

        
        if (trigger == "overlap" && overlapRules!= null)
            interactions.AddRange(overlapRules.interactions);
        if (trigger == "timed" && timedRules != null)
            interactions.AddRange(timedRules.interactions);
        return interactions;
    }
    
    [System.Serializable]
    public class InteractRules{
        public string note;
        public bool enabled = true;
        public string from, to, result;
        public InteractAction action;

        public bool IsMatch(string from, string to)
        {
            return this.from == from && this.to == to;
        }
		
		public void Serialize(string path){
			var ruleNode = NodeManager.Node(path, "");
			
			ruleNode.WriteAttrib("note", note);
			ruleNode.WriteAttrib("enabled", enabled);
			ruleNode.WriteAttrib("from", from);
			ruleNode.WriteAttrib("to", to);
			ruleNode.WriteAttrib("result", result);
			
			action.Serialize(NodeManager.Path(path, "action"));
		}
    }
}


[System.Serializable]
public class InteractAction{
    
	public List<string> conditions = new List<string>();
	public List<string> codes = new List<string>();
	public List<string> elseCodes = new List<string>();
	public InteractPrefabs spawnSet;
    
	internal InteractState target = null;// realtime data
	internal InteractState self = null;// realtime data
    [Header("don't use, use codes")]
	[HideInInspector]public string code = ""; // obsolete applies quick command
	
	public Transform FindPrefab(string key){
		return spawnSet != null ? spawnSet.FindPrefab(key) : null;
	}
	
	public void Serialize(string path){
		var actionNode = NodeManager.Node(path, "");
		
		NodeManager.List<string>(path, "conditions", conditions);
		NodeManager.List<string>(path, "codes", codes);
		NodeManager.List<string>(path, "elseCodes", elseCodes);
		
		actionNode.WriteAttrib("spawnSet", spawnSet.name);
	}
}