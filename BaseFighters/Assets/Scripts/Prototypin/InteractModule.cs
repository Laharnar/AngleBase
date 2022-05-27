using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class InteractModule : MonoBehaviour
{
    [System.Serializable]
    public class InteractTrigger{
        // trigger names: timed, tick, overlap(trigger collision), collision
        public string trigger;
        public InteractRuleset rules;
    }

    [FormerlySerializedAs("rules")]
    [SerializeField] InteractRuleset overlapRules; // trigger rules
	[SerializeField] InteractRuleset timedRules;
    [SerializeField] public List<InteractTrigger> triggerRules;


    public List<InteractModule.InteractRules> GetRules(string trigger){
        List<InteractModule.InteractRules> interactions = new List<InteractModule.InteractRules>();
        for (int i = 0; i < triggerRules.Count; i++)
        {
            if(triggerRules[i].trigger == trigger && triggerRules[i].rules != null){
                interactions.AddRange(triggerRules[i].rules.interactions);
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