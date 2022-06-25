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
		always = always || !GameObject.FindObjectOfType<NetworkManager>();
	}
	
    public void Tick(InteractState x, List<InteractRules> interactions, bool log, bool timeBound){
		if (IsOwner) {
			//if(IsServer || always || IsLocalPlayer)
			state.Tick(x, interactions, log, timeBound);
		}
	}
	
	public void Postspawn(GameObject obj){
		NetworkObject.Spawn(obj);
	}
	
	public void Despawn(GameObject obj){
		Destroy(obj);
	}
}
