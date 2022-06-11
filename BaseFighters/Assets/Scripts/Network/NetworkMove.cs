using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkMove: NetworkBehaviour{
	Move move;
	public InteractModule module;
	public bool isPlayer = false;
	
	void Awake(){
		if(!GameObject.FindObjectOfType<NetworkManager>()){
			enabled = false;
		}
	}
	
	void OnStartClient(){
		move = GetComponent<Move>();
		module = module == null ? GetComponent<InteractModule>() : module;
		Debug.Log("handling "+IsLocalPlayer + " "+ isPlayer);
		module.layers.Get("player").enabled = IsLocalPlayer && isPlayer;
		module.layers.Get("ai").enabled = !IsLocalPlayer || !isPlayer;
	}
	
}
