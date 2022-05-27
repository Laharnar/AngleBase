using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;

[CreateAssetMenu]
[RenamedFrom("InteractPref")]
public class InteractRuleset : ScriptableObject
{
    
    public List<InteractModule.InteractRules> interactions = new List<InteractModule.InteractRules>();
}
