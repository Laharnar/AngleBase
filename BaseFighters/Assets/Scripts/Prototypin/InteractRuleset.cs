using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;

[CreateAssetMenu(menuName="InteractLib/InteractRuleset")]
[RenamedFrom("InteractPref")]
public class InteractRuleset : ScriptableObject
{
    public List<InteractModule.InteractRules> interactions = new List<InteractModule.InteractRules>();
	
	
	public void Serialize(string path){
		var rulePath = NodeManager.Path(path, name);
		for (int i = 0; i < interactions.Count; i++){
			var pathI = NodeManager.Path(rulePath, i);
			interactions[i].Serialize(pathI);
		}
	}
}
