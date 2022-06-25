using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class Interact
{
    static Interact one;
    public static Interact ONE {
        get => one != null ? one : new Interact();
        private set => one = value;
    }

    Dictionary <GameObject, NetworkStorage> netDict = new Dictionary<GameObject, NetworkStorage>();
    Dictionary<GameObject, InteractStorage> objDictInv = new Dictionary<GameObject, InteractStorage>();
    char NETWORKSEPARATOR = '$';

    public Interact()
    {
        ONE = this;
    }

    public void Register(InteractStorage store, NetworkStorage net)
    {
        Debug.Log($"register net {store} {net}");
        netDict.Add(store.gameObject, net);
        objDictInv.Add(store.gameObject, store);
    }

    public void Run(List<string> code, GameObject storage)
    {
        if (!netDict.ContainsKey(storage)) {
            return; }
        var compressedCode = string.Join(NETWORKSEPARATOR, code);
        RunServerRpc(compressedCode, storage);
    }

    [ServerRpc]
    public void RunServerRpc(string code, GameObject storage)
    {
        var net = netDict[storage];
        net.SetNetClientRpc(code);
    }

    public void LocalRun(string code, InteractAction action, GameObject obj)
    {
        var store = objDictInv[obj];
        store.Exe(code);
    }

    public List<string> NetworkDecode(string last, out InteractAction action)
    {
        var lines = last.Split(NETWORKSEPARATOR).ToList();
        action = NetworkAction(lines[0]);
        return lines;
    }

    InteractAction NetworkAction(string actionMeta)
    {
        var ACTIONSEPARATOR = ',';
        var items = actionMeta.Split(ACTIONSEPARATOR);
        return new InteractAction()
        {
            
        };
    }
}

public class NetworkStorage : NetworkBehaviour
{
	NetworkVariable<ForceNetworkSerializeByMemcpy<FixedString64Bytes>>
 changes = new NetworkVariable<ForceNetworkSerializeByMemcpy<FixedString64Bytes>>
((FixedString64Bytes)"a"	  , NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    NetworkVariable<int> changeId = new NetworkVariable<int>
(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public int id;
	public string last;
    public int lastId;

    private void Awake()
    {
        Interact.ONE.Register(GetComponent<InteractStorage>(), this);
    }

    private void Update()
    {
        /*if (IsHost){
            Interact.ONE.RunServerRpc("set self time "+((int)Time.time), gameObject);
            //changes.Value = (FixedString64Bytes)("Asd" + Time.time);
        }*/
        /*if (lastId != changeId.Value)
        {
            last = changes.Value.Value.ToString();
            lastId = changeId.Value;
            Interact.ONE.LocalRun(last, gameObject);
        }*/
    }

    [ClientRpc]
    public void SetNetClientRpc(string value)
    {
        if (IsOwner) return;
        last = value;//changes.Value.Value.ToString();
        lastId = (changeId.Value + 1) % 1000000;//changeId.Value;
        var items = Interact.ONE.NetworkDecode(last, out InteractAction action);
        foreach (var item in items)
        {
            Interact.ONE.LocalRun(item, action, gameObject);
        }
        //changes.Value = (FixedString64Bytes)value;
        //changeId.Value = (changeId.Value + 1) % 1000000;
    }
}
 