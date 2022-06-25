using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class NetworkMove: NetworkBehaviour{
	Move move;
	public InteractModule module;
	public bool isPlayer = false;
	
	void Awake(){
		if(!NetworkManager.Singleton){
			enabled = false;
		}
	}
	
	void OnStartClient(){
		move = GetComponent<Move>();
		module = module == null ? GetComponent<InteractModule>() : module;
		Debug.Log("handling "+IsLocalPlayer + " "+ isPlayer);
		module.RealtimeLayers.Get("player").enabled = IsLocalPlayer && isPlayer;
		module.RealtimeLayers.Get("ai").enabled = !IsLocalPlayer || !isPlayer;
	}
	
	
	[ServerRpc]
	void DespawnServerRpc(){
		NetworkObject.Despawn();
	}
	
	void OnApplicationQuit(){
		if(enabled){
			DespawnServerRpc();
			NetworkManager.Singleton.Shutdown();
		}
	}
}
