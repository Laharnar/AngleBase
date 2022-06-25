using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// use for network
public interface IInteractTunnel{
	void Tick(InteractState x, List<InteractRules> interactions, bool log, bool timeBound);
}

public class InteractState:ComponentMono{

	public string state;

	[Header("Any mode")]
	public List<InteractRules> statics = new List<InteractRules>();
	
	[Header("Misc")]
	[SerializeField] InteractPrefabs color;

    [HideInInspector]public InteractModule module;
	[HideInInspector]public InteractPickup pickup;
	[HideInInspector]public InteractStorage store;
	
	[Header("Timed")]
	[SerializeField] bool autoFresh = false;
	[SerializeField] int skipTimes = 0;
	[SerializeField] float freshRate = 0.5f;

	public InteractLogs logs;

	internal List<InteractAction> actions = new List<InteractAction>();
	internal InteractState spawnBy;
	IInteractTunnel tunnel;
	
	Action<InteractState, List<InteractRules>, bool, bool> TickE;

	bool first = false;
	float tickTime = 0;
	float lastTime = -1;

	void Awake(){
		ValidateComponents();
		TickE = Tick;
		tunnel = GetComponent<IInteractTunnel>();
		if(tunnel != null)
			TickE = tunnel.Tick;
	}
	
	void OnDestroy(){
		TickE(this, module.GetRules("predestroy"), logs.logPreDestroy, false);
	}

	public void ValidateComponents(){
		if(module == null) module = GetComponent<InteractModule>();
        if(pickup == null) pickup = GetComponent<InteractPickup>();
		if(store == null) store =  GetComponent<InteractStorage>();
	}

    protected override void Start(){
		base.Start();
		if(state == "")
			Debug.LogError("Make sure to assign state here", this);
		
		
		// also starts self
		if(transform.parent == null || Time.time > 0 || transform.parent.GetComponentInParent<InteractState>() == null)
		{
			var states = GetComponentsInChildren<InteractState>();
			foreach (var item in states)
				item.StartInit();
		}
	}

	void StartInit(){
		pickup.LoadGlobals();
		store.stored.InitStr("state", state);
		lastTime = -1;
		TickE(this, module.GetRules("start"), logs.logStart, false);
	}
	
	public void StartReinitOnLayerUpdate(){
		TickE(this, module.GetRules("start"), logs.logStart, false);
	}
	
	void Update(){
		if(!first){
			TickE(this, module.GetRules("start2"), logs.logStart, false);
			first = true;
		}
		if(autoFresh && Time.time > tickTime){
			tickTime = Time.time + freshRate;
			if(skipTimes == 0)
			{
				if(_collider != null)
					_collider.enabled = !_collider.enabled;
				OnTimed();
			}
			if(skipTimes > 0)
				skipTimes--;
		}
		else TickE(this, module.GetRules("tick"), logs.logTick, true);
	}

	public void OnSpawn(List<InteractState> spawnBlock){
		// spawnBlock: single set of spawn-spawner.
		for (int i = 0; i < spawnBlock.Count; i++)
		{
			var o = spawnBlock[i];
			if (o == null) continue;
			var mods = o.module.GetRules("spawn");
			mods.AddRange(this.module.GetRules("spawn"));
			if(logs.logSpawn)
				Debug.Log("OnSpawn "+" tick on "+o+" "+this+" "+mods.Count);
			o.TickE(o, mods, logs.logSpawn, true);
		}
	}
	
	public void CustomTrigger(string trigger){
		TickE(this, module.GetRules(trigger), logs.logCustom, false);
	}

	public void OnTimed(string timedRule ="timed")
    {
		TickE(this, module.GetRules(timedRule), logs.logTimed, false);
	}

    void OnTriggerEnter2D(Collider2D collider){
        InteractState x;
        if (collider.gameObject != gameObject && collider.TryGetComponent<InteractState>(out x))
			TickE(x, module.GetRules("overlap"), logs.logOverlap || x.logs.logOverlap, false);
    }
	
	// Prefer other public functions. This is to be used IInteractTunnel.
	public void Tick(InteractState x, List<InteractRules> interactions, bool log=false, bool timeBound = true){
		#if UNITY_EDITOR
		if (log)
			Debug.Log(interactions.Count + " same time:"+ (lastTime == Time.time));
		#endif

		if (interactions.Count == 0) // prevents tick overriding
			return;
        if(timeBound && lastTime == Time.time) // prevents overlapping from multiple calls
            return;
		var last = state;
	
		// statics, trigger on self
		/*actions = Action(state, x.state, statics);
		Fill(actions, x);
		pickup.Trigger(x.state, statics, log);
		Fill(actions, null);

		x.actions = Action(x.state, last, x.statics);
		Fill(x.actions, this);
		x.pickup.Trigger(last, x.statics, log);
		Fill(x.actions, null);*/
		if(statics.Count > 0)
			Debug.LogError("Errorrrr Obsolete");
		
		// prefs, trigger on both, or ""="any" on self
		var any = Action("", "", interactions);
		var selfOneWay = Action(state, "", interactions);
		actions.Clear();
		actions.AddRange(any);
		actions.AddRange(selfOneWay);
		actions.AddRange(Action(state, x.state, interactions));
		Fill(actions, x);
		pickup.Trigger(x.state, interactions, log);
		Fill(actions, null);

		x.actions = Action(x.state, last, interactions);
		Fill(x.actions, this);
		x.pickup.Trigger(last, interactions, log);
		Fill(x.actions, null);

		UpdateLocals(); 
		x.UpdateLocals();

		if(store.recentlySpawned.Count > 0){
			//Debug.Log("prespawn "+state + " "+x.state + " "+this+" "+x);
			store.recentlySpawned.Add(x);// add spawner
			List<InteractState> states = new List<InteractState>(store.recentlySpawned);
			store.recentlySpawned.Clear();
			OnSpawn(states);
		}
	}

	void UpdateLocals(){
		lastTime = Time.time;
		if(color != null)
			sprite.color = color.FindColor(state);
	}

 	public static List<InteractAction> Action(string from, string to, List<InteractRules> interactions){
		List<InteractAction> actions = new List<InteractAction>();
        for (int i = 0; i< interactions.Count; i++){
			if(!interactions[i].enabled)
				continue;
				
            if(interactions[i].IsMatch(from, to))
                actions.Add(interactions[i].action);
        }
        return actions;
    }

    void Fill(List<InteractAction> actions, InteractState target){
		foreach (var action in actions){
			action.target = target;
			action.self = this;
		}
	}
}

public class ComponentMono : MonoBehaviour
{
	protected SpriteRenderer sprite;
	public Collider2D _collider;

	protected virtual void Start()
	{
		sprite = transform.GetComponent<SpriteRenderer>();
		if (_collider == null) _collider = GetComponent<Collider2D>();
	}
}

[System.Serializable]
public class InteractLogs
{

	public bool logOverlap = false;
	public bool logTimed = false;
	public bool logTick = false;
	public bool logStart = false;
	public bool logSpawn = false;
	public bool logPreDestroy = false;
	public bool logCustom = false;
}