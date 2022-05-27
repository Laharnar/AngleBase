using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ComponentMono:MonoBehaviour{
	protected SpriteRenderer sprite;
    public Collider2D _collider;

	protected virtual void Start(){
		sprite = transform.GetComponent<SpriteRenderer>();
        if(_collider == null) _collider = GetComponent<Collider2D>();
	}
}

[System.Serializable]
public class InteractLogs{

	public bool logOverlap=false;
	public bool logTimed=false;
	public bool logTick=false;
	public bool logStart=false;
	public bool logSpawn=false;
}

public class InteractState:ComponentMono{

    public string state;

	[Header("Any mode")]
	public List<InteractModule.InteractRules> statics = new List<InteractModule.InteractRules>();
	
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

	bool first = false;
	float tickTime = 0;
	float lastTime = -1;

	void Awake(){
		ValidateComponents();
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
		Tick(this, module.GetRules("start"), logs.logStart);
	}
	
	void Update(){
		if(!first){
			Tick(this, module.GetRules("start2"), logs.logStart);
			first = true;
		}
		if(autoFresh && Time.time > tickTime){
			tickTime = Time.time + freshRate;
			if(skipTimes == 0)
			{
				if(_collider != null)
					_collider.enabled = !_collider.enabled;
				Tick(this, module.GetRules("timed"), logs.logTimed);
			}
			if(skipTimes > 0)
				skipTimes--;
		}
		else Tick(this, module.GetRules("tick"), logs.logTick);
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
			o.Tick(o, mods, logs.logSpawn);
		}
	}

    void OnTriggerEnter2D(Collider2D collider){
        InteractState x;
        if (collider.gameObject != gameObject && collider.TryGetComponent<InteractState>(out x))
			Tick(x, module.GetRules("overlap"), logs.logOverlap || x.logs.logOverlap);
    }
	
	void Tick(InteractState x, List<InteractModule.InteractRules> interactions, bool log=false){
		if(interactions.Count == 0) // prevents tick overriding overlap
			return;
        if(lastTime == Time.time)
            return;
		var last = state;

		// statics, trigger on self
		actions = Action(state, x.state, statics);
		Fill(actions, x);
		pickup.Trigger(x.state, statics, log);
		Fill(actions, null);

		x.actions = Action(x.state, last, x.statics);
		Fill(x.actions, this);
		x.pickup.Trigger(last, x.statics, log);
		Fill(x.actions, null);
		
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
			OnSpawn(store.recentlySpawned);
			store.recentlySpawned.Clear();
		}
	}

	void UpdateLocals(){
		lastTime = Time.time;
		if(color != null)
			sprite.color = color.FindColor(state);
	}

 	public static List<InteractAction> Action(string from, string to, List<InteractModule.InteractRules> interactions){
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
