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
    public List<InteractTrigger> EditorTriggers => triggers;
    public bool EditorEnabled { get => enabled; set => enabled = value; }
    public string EditorLayer => layer;

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

    public void Add(List<InteractTrigger> items)
    {
        if (items == null) return;
        triggers.AddRange(items);
    }

    public void Clear()
    {
        triggers.Clear();
    }
}

[System.Serializable]
public class InteractBox
{
    public List<InteractLayer> layers = new List<InteractLayer>();

    public void Add(InteractLayer add){
        foreach (var item in layers)
        {
            if(item.Matches(add.layer)){
                item.Add(add.triggers);
                return;
            }
        }
        InteractLayer layer ;
        layers.Add(layer = new InteractLayer(){
            enabled = true,
            layer = add.layer,
            triggers= new List<InteractTrigger>()
        });
        layer.Add(add.triggers);
    }

    /// <summary>
    /// Adds this to other where layers match.
    /// </summary>
    /// <param name="other"></param>
    public void Join(InteractBox other)
    {
        foreach (var item in layers)
        {
            if(item == null) continue;
            bool added = false;
            other.Add(item);
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
}

[System.Serializable]
public class InteractTrigger
{
    // trigger names: timed, tick, overlap(trigger collision), collision
    public string trigger;
    public InteractRuleset rules;
}

public class InteractModule : MonoBehaviour
{
    [SerializeField] public InteractBox layers;
    [SerializeField] List<InteractBoxPref> boxes;
    [Header("Start:Loaded to 'base', then cleared")]
    [SerializeField] List<InteractTrigger> triggerRules;
    [Header("Preferably obsolete")]
    [FormerlySerializedAs("rules")]
    [SerializeField] InteractRuleset overlapRules; // trigger rules
    [SerializeField] InteractRuleset timedRules;

    public List<InteractTrigger> EditorTriggerRules => triggerRules;

    void Awake()
    {
        foreach (var item in boxes)
            item.box.Join(layers);
        layers.Get("base").Add(triggerRules);
        triggerRules.Clear();
    }

    public List<InteractModule.InteractRules> GetRules(string trigger){
        List<InteractModule.InteractRules> interactions = new List<InteractModule.InteractRules>();
        foreach (var item in layers.layers){
            if(item.enabled){
                var triggers = item.triggers;
                for (int i = 0; i < triggers.Count; i++)
                {
                    if(triggers[i].trigger == trigger && triggers[i].rules != null){
                        interactions.AddRange(triggers[i].rules.interactions);
                    }
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
}