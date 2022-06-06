using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class InteractPickup:MonoBehaviour{

	public bool autoLoadGlobals = false;
    InteractState states;
	public List<InteractStorage> storages; // Can use storage from other objects.


    void Awake(){
        states = GetComponent<InteractState>();
		if (GetComponent<InteractStorage>()!= null)
			storages.Add(GetComponent<InteractStorage>());
    }

	public void LoadGlobals(){
		if(autoLoadGlobals){
			storages.Add(InteractStorage.global);
		}
	}

    public void Trigger(string transitionTo, List<InteractModule.InteractRules> interactions, bool log = false){
		if(log)
		Debug.Log("count: "+states.actions.Count);
		foreach (var action in states.actions){
			if(action == null)
				continue;
			bool pass = true;
			string passFail = "";
			if(action.conditions != null && action.conditions.Count > 0){
				for (int j = 0; j < action.conditions.Count; j++){
					var condition = action.conditions[j];
					for (int i = 0; i < storages.Count; i++){
						if (!InteractStorage.Activate(condition, action, storages, log)){
							pass = false;
							passFail = condition;
							break;
						}
					}
					if(!pass)
						break;
				}
			}
			
			if(log)
				Debug.Log($"Passing: {pass} condFailed: '{passFail}'", action.self);
			List<string> codes;
			if (pass){		
				var x = Transition(states.state, transitionTo, interactions);
				states.state = x != "" ? x : states.state;

				codes = action.codes;
			}else{ 
				codes = action.elseCodes;
			}
			if(codes!= null){
				for (int i = 0; i < codes.Count; i++)
					Handle(codes[i], action, log);
			}
		}
		states.actions.Clear();
    }
	
    public static string Transition(string from, string to, List<InteractModule.InteractRules> interactions){
        if(to == "" || from == "")
            return from;
        for (int i = 0; i< interactions.Count; i++){
			if(!interactions[i].enabled)
				continue;
            if(interactions[i].IsMatch(from, to))
                return interactions[i].result;
        }
        return from;
    }

	public void Handle(string code, InteractAction action, bool log){
		if(code == "")
			return;
		if(log) 
			Debug.Log($"handling {action.self} '{code}' ->target:{action.target}", action.self);

		bool pass = true;
		if(code == "drop")
			transform.parent = transform.parent.parent;
		else if(code == "death")
			Destroy(transform.gameObject);
		else {
			pass = false;
			for (int i = 0; i < storages.Count; i++){
				if (InteractStorage.Activate(code, action, storages, log)){
					pass = true;
					break;
				}
			}
		}
		if(!pass)
			Debug.Log($"Unhandled code, missed code '{code}' {action.self}", action.self);
	}
}
