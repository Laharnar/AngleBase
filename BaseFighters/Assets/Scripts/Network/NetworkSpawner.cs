using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class NetworkSpawner : NetworkBehaviour, IInteractTunnel
{
	InteractState state;
	public bool always = false;
	
	void Awake(){
		state = GetComponent<InteractState>();
	}
	
    public void Tick(InteractState x, List<InteractModule.InteractRules> interactions, bool log, bool timeBound){
		if(IsServer || always)
			state.Tick(x, interactions, log, timeBound);
	}
}
