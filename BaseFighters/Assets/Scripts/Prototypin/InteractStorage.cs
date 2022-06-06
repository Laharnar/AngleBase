using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class InteractStorage:MonoBehaviour{
	
	public string key = "self";
	public InteractStorageList stored = new InteractStorageList();
	public InteractTransformList objects = new InteractTransformList();
	public InteractScriptList scripts = new InteractScriptList();
	public List<TransformsList> tbuckets;
	
	[Header("Act on proxy instead. Use when destroying obj would cancel events.")]
	public InteractStorage proxy;
	
	internal List<InteractState> recentlySpawned = new List<InteractState>();
	
	internal InteractState state;
	public static InteractStorage global;

	void Awake(){
		state = GetComponent<InteractState>();
		if(key == "globals" || key =="global"){
			global = this;
		}
	}

	///Finds correct storage based on source ("self", "target", "parent", "script+name", "obj+name", "anyparent", "parent")
	static InteractStorage Redirect(string source, InteractAction action, List<InteractStorage> storages = null, InteractStorage defValue = null){
		var flexSource = source;
		if(source == "self") // else is important here
			return action.self.store;
		else if(source == "target"){
			storages = action.target.pickup.storages;
			flexSource = "self"; // flexSelf
		}
		else if(source == "parent"){
			storages = action.self.transform.parent.GetComponent<InteractState>().pickup.storages;
			flexSource = "self"; // flexSelf
		}
		else if(source == "parentparent"){
			storages = action.self.transform.parent.parent.GetComponent<InteractState>().pickup.storages;
			flexSource = "self"; // flexSelf
		}
		else if(source == "anyparent"){
			storages = action.self.transform.parent.GetComponentInParent<InteractState>().pickup.storages;
			flexSource = "self"; // flexSelf
		}
		else if(source == "spawnBy"){
			return action.self.spawnBy.store;
		}else if(source.Contains('+')){
			var items = source.Split("+");
			if(items[0] == "script")
				return (InteractStorage)action.self.store.scripts[items[1]].script;
			else if(items[0] == "obj")
				return action.self.store.objects[items[1]].prefab.GetComponent<InteractStorage>();
		}
		
		if(storages != null){
			var x = Find(flexSource, storages, action.self.transform);
			if(x != null)
				return x;
		}
		if(defValue != null) // if it's null, expect null to be allowed
			Debug.Log("No matches, using object of storage. "+flexSource + " -> "+defValue, action.self);
		return defValue; // most unsafe
	}
	
	// invoked
	void Respawn(){
		Scene scene = SceneManager.GetActiveScene(); 
		SceneManager.LoadScene(scene.name);
	}
	
	static void SetValue(string source, string prop, string value, InteractAction action, List<InteractStorage> storages)	
	{
		// int/str value setter
		var storage = Redirect(source, action, storages);
		var dict = storage.stored;
		dict.Init(prop, 0);

		int res = 0;
		if(int.TryParse(value, out res))
			dict[prop].value = res;
		dict[prop].svalue = value.ToString();
	}

	static bool SecondActivation(string code, InteractAction action, List<InteractStorage> storages, bool log = false)
	{
		// parses by structure instead of by order
		// spawn prefs shields self parent=self
		// version command reference_Or_value subparams
		
		// iteration data
		InteractStorage source = null;
		KeyScript script;
		KeyTransform keyTransform;
		Transform tobj;
		string module = "";
		string tag = "";
		object ref1 = null;
		
		// states
		int mode = 1; // mode 0: unknown, mode 1: search
		int calculation = 0; // 0 : no calc, +1: calc modes
		
		// result data
		bool spawn = false;
		bool success = false;
		
		// results
		List<object> refs = new List<object>();
		Dictionary<string, object> parameters = new Dictionary<string, object>();
		// 2: source(self, target), 21: module(obj, script), 22:tag, 
		// 3: read reference
		// 4: calculation, 5: end check.
		
		// iterations
		Queue<string> codes = new Queue<string>();
		codes.Enqueue(code);
		while (codes.Count > 0)
		{
			var icode = codes.Dequeue();
			var items = icode.Split(" ");
			for (int i = 0; i < items.Length; i++)
			{
				if (mode == 1)
				{
					// reset
					source = null;
					ref1 = null; 
					script = null;
					keyTransform = null; 
					tobj = null;
					module = "";
					tag = "";

					// recognize commaand
					var command = items[i];
					if (command == "spawn")
					{
						spawn = true;
						mode = 2;
					}
					else if (command.Contains("=") && (command != "=" && command.Length > 2))
					{
						// it's dynamic argument
						parameters.Add(command, null);
						var arg = command.Split("=");
						codes.Enqueue(arg[1]);
						mode = 1;
					}
					else
					{
						// incorrect command -> maybe it's source parameter
						mode = 2;
						i--;
					}
				}
				else if (mode == 2)
				{
					var src = items[i];
					source = Redirect(src, action, storages, null);
					if (source == null)
					{
						// failed to find any -> it's not source, or source ended
						mode = 4;
						i--;
					}
					if (source != null)
					{
						ref1 = source;
						mode = 21;
					}
				}
				else if (mode == 21)
				{
					if (module == "")
					{
						module = items[i];
						if (module == "obj" || module == "prefs" || module == "script")
							mode = 22;
						else
						{
							// not valid module, or end of item
							module = "";
							mode = 4;
							i--;
						}
					}else { 
						mode = 22;
						i--;
					}
				}
				else if (mode == 22)
				{
					tag = items[i];
					mode = 3;
				}
				else if (mode == 3)
				{
					// collect module
					if (module == "script")
						ref1 = script = source.scripts[tag];
					else if (module == "obj")
						ref1 = keyTransform = source.objects[tag];
					else if (module == "prefs")
						ref1 = tobj = action.FindPrefab(tag);
					mode = 4;
					i--;
				}
				else if (mode == 4)
				{
					// apply(calc, ref)
					//if(calculation != 0)
					// todo
					// mode = 5
					//else {
					if (ref1 != null)
					{
						refs.Add(ref1);
						mode = 1;
						i--;
					}
					else
					{
						if(log)
							Debug.LogError($"Exit {icode}");
						mode = -1;
						break;
					}
				}
			}
			
			if(ref1 != null){
				refs.Add(ref1);
				ref1 = null;
			}
		}
	
		
		if(spawn){
			if(refs.Count < 3){
				Debug.Log($"Spawn err: {code}. unknown. last mode: {mode}. found: {refs.Count}");
				return success = false;
			}
			Transform GetTransform (object obj){
				if(log)
					Debug.Log($"Get transform{obj}");
				return obj is Transform ? (Transform)obj : obj is InteractStorage ? ((InteractStorage)obj).transform : ((KeyTransform)obj).prefab;
			}
			
			var pref = GetTransform(refs[0]);
			var pos = refs[1] is Vector3 ? (Vector3) refs[1] : GetTransform(refs[1]).position;
			var parent = refs.Count > 2 ? GetTransform(refs[2]) : null;
			
			var t = Instantiate(pref, parent).transform;
			t.transform.position = pos;
			t.transform.rotation = parent != null ? parent.rotation : action.self.transform.rotation;
			InteractState[] tStore = t.GetComponentsInChildren<InteractState>();
			for (int i = 0; i < tStore.Length; i++)
			{
				tStore[i].spawnBy = action.self;
				action.self.store.recentlySpawned.Add(tStore[i]);
			}
			success = true;
		}
		return success;
	}
		
	// return true -> handled/content matches, depending on type of command
	// see -> true/false by match
	// false if all fails.
	public static bool Activate(string code, InteractAction action, List<InteractStorage> storages, bool log = false){
		// for new blocks, manually return true.
		if(action == null) Debug.LogError("Action is null");
		code = ConvertKeys(code);
		KeyScript GetReference(string[] items, ref int id){
			var storage = Redirect(items[id], action, storages); id++;
			var typ = items[id]; id++;
			if (typ == "script"){
				var script = storage.scripts[items[id]]; id++;
				return script;
			}
			Debug.LogError($"Unsupposerted {typ}");
			return null;
		}
		
		// version 2 first, then v1.
		if(SecondActivation(code, action, storages, log)){
			return true;
		}
			
		string ConvertKeys(string code){
			// ... { targets+see self flag } ...{ .. 2 .. } ...
			StringBuilder build = new StringBuilder();
			
			for (int i = 0; i < code.Length; i++)
			{
				if(code[i] == '{')
				{
					StringBuilder subpiece = new StringBuilder();
					for (int j = i + 1; j < code.Length; j++)
					{
						i = j;
						if(code[j] == '}'){
							break;
						}
						subpiece.Append(code[j]);
					}
					var subs = subpiece.ToString().Split('+');
					foreach (var sub in subs)
					{
						int offset = sub[0] == ' ' ? 1 : 0;
						if(sub.Substring(offset, 3) == "see")
							build.Append(ReadInt(sub.Split(' '), 0));
						else build.Append(sub);
					}
				}
				else {
					build.Append(code[i]);
				}
			}
			return build.ToString();
		}

		int ReadInt(string[] items, int start){
			// see self prop
			var result = 0;
			if(items[start] == "see")
			{
				var sourceNm = items[start+1];
				var source = Redirect(sourceNm, action, storages);
				var prop = items[start+2];
				source.stored.Init(prop);
				result = source.stored[prop].value;
				if(log) Debug.Log($"Redir {sourceNm} -> {source}", source);
			}
			else result = int.Parse(items[start]);
			if(log) Debug.Log($"ReadInt {code} -> {result}", action.self);
			return result; // optimized start
		}

		if(log) Debug.Log($"code:: {code} '{action.self}'", action.self);
		var self = action.self.store;
		string[] items = code.Split(" ");
		if(items.Length == 1)
		{
			if(items[0] == "restart")
				self.proxy.Invoke("Respawn", 2);
			else return false;
			return true;
		}
		#region globals
		if(items.Length == 2 && (items[0] == "global" || items[0] == "globals")){
			if(items[1] == "restart"){
				self.proxy.Invoke("Respawn", 2);
			}
			
			else return false;
			return true;
		}
		#endregion globals

		if (items.Length < 3) 
			return false;
		
		#region special
		if(items[0] == "register"){ // register self globals targets
			var first = Redirect(items[1], action, storages);
			var second = Redirect(items[2], action, storages);
			var secondSub = items[3];
			if(log)
				Debug.Log("register transform to "+second.transform, self);
			second.tbuckets.Find(secondSub).transforms.Add(first.transform);
			return true;
		}
		else if (items[0] == "clear" && items.Length >= 3){ // clear null tbucket
			if(items[1] == "null"){
				int count = action.self.store.tbuckets.Find(items[2]).ClearNulls();
				if(items.Length == 5){
					if(items[3] == "reduce"){
						action.self.store.stored.Init(items[4]);
						action.self.store.stored[items[4]].value -= count;
					}
				}
			}else return false;
			return true;
		}
		#endregion special

		#region actions
		else if (items[1] == "action"){
			// target action picked
			// self action spawn ore random 1 local
			var source = items[0];
			var command = items[1];
			var doing = items[2];
			
			var target = Redirect(source, action, storages);
			var goSource = target.gameObject;
			if (doing == "pickup" || doing == "picked")
				target.transform.parent = self.transform;
			else if (doing == "drop")
				target.transform.parent = self.transform.parent;
			else if (doing == "death" || doing == "destroy")
				Destroy(goSource);
			else if (doing == "spawn"){
				var actionSelf = action.self;
				if (items.Length < 4) 
					Debug.LogError("wrong item count, need at least 4", action.self);
				// self action spawn ore
				// self action spawn ore random 1 local
				// self action spawn bullet child_0 
				var prefab = items[3];
				float range = 0;
				if(items.Length >= 6)
					range = float.Parse(items[5]);
				
				bool inLocalSpace = true;
				if(items.Length >= 7)
				{	
					var spaceStr = items[6];
					if (spaceStr == "world")
						inLocalSpace = false;
				}
				
				Transform parent = goSource.transform;
				if(items.Length >= 8)
				{	
					var spaceStr = items[7];
					if (spaceStr == "noparent")
						parent = null;
					else if (spaceStr == "parent")
						parent = parent.parent;
					else if (spaceStr == "parentparent")
						parent = parent.parent.parent;
					else if (spaceStr == "target")
						parent = action.target.transform;
				}
				if(items.Length == 5){
					parent = null;
				}
				
				Vector3 pos = Vector3.zero;
				if(items.Length >= 5){
					var posStrpre = items[4];
					var posItems = posStrpre.Split("+");
					
					foreach (var posStr in posItems){
						if(posStr == "self")
							pos += actionSelf.transform.position;
						else if(posStr == "target")
							pos += action.target.transform.position;
						else if(posStr == "parent")
							pos += actionSelf.transform.parent.position;
						else if(posStr == "random")
							pos += (Vector3)Random.insideUnitCircle * range;
						else if(posStr.StartsWith("child_")){
							// child_0, child_1...
							var sub =int.Parse(posStr.Substring("child_".Length));
							pos += action.self.transform.GetChild(sub).transform.position;
						}else if(posStr.StartsWith("obj_")){
							// objstr, objname...
							var sub = posStr.Substring("obj_".Length);
							var tt = self.objects[sub].prefab;
							pos += tt.transform.position;
						}
					}
				}
				var pref = action.FindPrefab(prefab);
				if(pref == null) pref = actionSelf.store.objects[prefab].prefab;
				if(pref == null) Debug.LogError($"Miss pref {prefab} in action or on storage.", actionSelf);
				var t = Instantiate(pref, parent).transform;
				t.transform.position = pos;
				t.transform.rotation = actionSelf.transform.rotation;
				InteractState[] tStore = t.GetComponentsInChildren<InteractState>();
				for (int i = 0; i < tStore.Length; i++)
				{
					tStore[i].spawnBy = actionSelf;
					actionSelf.store.recentlySpawned.Add(tStore[i]);
				}
				if (inLocalSpace)
					t.transform.localPosition = pos;
			}else return false;
			return true;
		}
		#endregion actions
		#region setter
		// standard setter : set reference1 value_OR_valueAtReference2
		else if (items.Length == 4 && items[0]== "set"){
			var command = items[0];
			// set source prop value
			SetValue(items[1], items[2], items[3], action, storages);
			return true;
		}else if(items.Length == 8 && items[0] == "set"){
			
			
			// set self script Move = self script AiMove
			int id = 1;
			var ref1 = GetReference(items, ref id);
			var op = items[id++];
			var ref2 = GetReference(items, ref id);
			ref1.script = ref2.script;
			return true;
		}else  if(items.Length == 5 && items[0] == "set"){
			
			// set reference reference_OR_value
			int id = 1;
			var ref1 = GetReference(items, ref id);
			var objStore = Redirect(items[id], action, storages);
			ref1.script = objStore;
			return true;
		}
		else if (items.Length == 4 || (items.Length == 6 && items[3] == "see")/*redirects second part*/){
			// add, remove, set, enable, also gravity on rigbody
			var source = items[0];
			var command = items[1];
			var prop = items[2];
			var value = items[3];
			var storage = Redirect(source, action, storages);
			var dict = storage.stored;
			dict.Init(prop, 0);
			if(command == "store" || command == "add")
			{
				int ivalue = ReadInt(items, 3); // refacator up.
				dict[prop].value = Mathf.Max(dict[prop].value + ivalue, 0);
				dict[prop].svalue = dict[prop].value.ToString();
			}
			else if(command == "reduce" || command == "take")
			{
				int ivalue = ReadInt(items, 3); // refacator up.
				if(log)
					Debug.Log($"Reduce::{storage} {ivalue}", storage);
				dict[prop].value = Mathf.Max(dict[prop].value - ivalue, 0);
				dict[prop].svalue = dict[prop].value.ToString();
			}
			else if(command == "set")
			{
				SetValue(source, prop, value, action, storages);
			}
			else if(command == "enable"){
				var objects = storage.objects;
				var scripts = storage.scripts;
				var module = storage.state.module;
				if(prop == "obj"){
					if(log)
						Debug.Log($"Enable code: {code} {value} {storage} '{objects[value]}'", action.self);
					if(objects[value].prefab != null)
						objects[value].prefab.gameObject.SetActive(true);
					else Debug.LogError($"Objects prefab {value} is null", action.self);
				}
				else if(prop == "script")
					((Behaviour)scripts[value].script).enabled = true;
				else if(prop == "layer")
					module.layers.Get(value).enabled = true;
			}
			else if(command == "disable"){
				var objects = storage.objects;
				var scripts = storage.scripts;
				var module = storage.state.module;
				if(log)
					Debug.Log($"Disable code: {code}", action.self);
				if(prop == "obj")
					objects[value].prefab.gameObject.SetActive(false);
				else if(prop == "script")
					((Behaviour)scripts[value].script).enabled = false;
				else if(prop == "component"){
					if(value == "Rigidbody2D")
						((Rigidbody2D)scripts[value].script).gravityScale = 0;
				}
				else if(prop == "layer")
					module.layers.Get(value).enabled = true;
			}else return false;
			return true;
		}
		#endregion setter
		#region check
		else if(items[0] == "see"){
			var command = items[0];
			InteractStored stored;
			InteractStored stored2;
			string op;
			if (items.Length == 5){
				
				// "see parent drill = 1"
				var source = items[1];
				var prop = items[2];
				op = items[3];
				var value = items[4];
				var ivalue = 0;
				bool isString = !int.TryParse(value, out ivalue);
				var target = Redirect(source, action);
				target.stored.Init(prop, 0);
				
				stored = target.stored[prop];
				if(log)
					Debug.Log(code + ":: "+target + " value:"+stored.value, action.self);
				if (op == "="){
					return (isString && stored.svalue.ToString() == value)
						|| (!isString && stored.value == ivalue);
				}else if (op == "!="){
					return stored.svalue.ToString() != value 
						|| stored.value != ivalue;
				}else if(op == "<"){
					return stored.value < ivalue;
				}else if(op == ">"){
					return stored.value > ivalue;
				}else if(op == ">="){
					return stored.value >= ivalue;
				}else if(op == "<="){
					return stored.value <= ivalue;
				}else Debug.LogError("Unknown operator" + op, action.self);
			}else if (items.Length == 7){
				// "see parent drill = see self drill"
				var source = items[1];
				var prop = items[2];
				op = items[3];
				var source2 = items[5];
				var prop2 = items[6];
				var target = Redirect(source, action);
				var target2 = Redirect(source2, action);
				target.stored.Init(prop, 0);
				target2.stored.Init(prop2, 0);
				
				stored = target.stored[prop];
				stored2 = target2.stored[prop2];
				//Debug.Log(code + ":: "+target + " stored:"+stored.svalue);
				if (op == "="){
					return stored.svalue.ToString() == stored2.svalue 
					|| stored.value == stored2.value;
				}else if (op == "!="){
					return stored.svalue.ToString() != stored2.svalue 
					|| stored.value != stored2.value;
				}else if(op == "<"){
					return stored.value < stored2.value;
				}else if(op == ">"){
					return stored.value > stored2.value;
				}else if(op == ">="){
					return stored.value >= stored2.value;
				}else if(op == "<="){
					return stored.value <= stored2.value;
				}else Debug.LogError("Unknown operator" + op);
			}else return false;
			return true;
		}
		#endregion check
		#region copy-paste obj1 -> obj2
		else if(items[0] == "copyAdd"){
			if(items.Length == 5){
				// copyAdd source1 source2 prop amount
				var command = items[0];
				var source1 = Redirect(items[1], action, storages);
				var source2 = Redirect(items[2], action, storages);
				var propName = items[3];
				var amount = Mathf.Max(ReadInt(items, 4), 0); // negatives are iffy.
				source1.stored.Init(propName, 0);
				source2.stored.Init(propName, 0);

				var taken = Mathf.Min(amount, source1.stored[propName].value);
				source2.stored[propName].value += taken;
			}
			else if(items.Length == 6)
			{
				// copyAdd 1 source bucket source2 bucket2
				var command = items[0];
				var count = int.Parse(items[1]);
				var first = Find(items[2], storages, action.self.transform);
				var tfirst = first.tbuckets.Find(items[3]);
				var second = Find(items[4], storages, action.self.transform);
				var tsecond = second.tbuckets.Find(items[5]);

				var temp = Redirect(items[4], action, storages);
				var dict = temp.stored;
				count = Mathf.Min(count, tfirst.transforms.Count);
				for (int i = 0; i < count; i++)
					tsecond.transforms.Add(tfirst.transforms[0]);
				dict.Init(tsecond.key, 0);
				dict[tsecond.key].value += count;
			}else return false;
			return true;
		}else if(items[0] == "copypaste"){
			// copypaste from to prop value|see
			// copyAdd source1 source2 prop amount
			var command = items[0];
			var source1 = Redirect(items[1], action, storages);
			var source2 = Redirect(items[2], action, storages);
			var propName = items[3];
			var amount = ReadInt(items, 4); // negatives are iffy.
			source1.stored.Init(propName, 0);
			source2.stored.Init(propName, 0);
			
			var taken = Mathf.Min(amount, source1.stored[propName].value);
			source2.stored[propName].value = taken;
			return true;
		}else if(items[0] == "copy*"){
			// copypaste from to prop value|see
			// copyAdd source1 source2 prop amount
			var command = items[0];
			var source1 = Redirect(items[1], action, storages);
			var source2 = Redirect(items[2], action, storages);
			var propName = items[3];
			var amount = ReadInt(items, 4); // negatives are iffy.
			source1.stored.Init(propName, 0);
			source2.stored.Init(propName, 0);
			
			var taken = Mathf.Min(amount, source1.stored[propName].value);
			source2.stored[propName].value *= taken;
			return true;
		}
		else if(items[0] == "transfer" || items[2] == "transfer"){
			if(items.Length == 5 && items[2] == "transfer"){
				var command = items[2];
				// a b transfer drill 1
				var first = Find(items[0], storages, action.self.transform);
				var target = Find(items[1], storages, action.self.transform);
				if(items[1] != "self")
					target = Redirect("self", action);
				var prop = items[3];
				var value = items[4];
				
				first.stored.Init(prop, 0);
				target.stored.Init(prop, 0);

				var prev = first.stored[prop].value;
				first.stored[prop].value = Mathf.Max(int.Parse(value) - prev, 0);
				var removed = prev - first.stored[prop].value;
				target.stored[prop].value = Mathf.Max(target.stored[prop].value + removed, 0);
			}
			else if(items.Length == 5 && items[0] == "transfer"){
				// transfer source1 source2 prop amount
				var command = items[0];
				var source1 = Redirect(items[1], action, storages);
				var source2 = Redirect(items[2], action, storages);
				var propName = items[3];
				var amount = Mathf.Max(int.Parse(items[4]), 0); // negatives are iffy.
				source1.stored.Init(propName, 0);
				source2.stored.Init(propName, 0);
				if(log)
					Debug.Log($"pretransfer {propName} {source1.stored[propName].value} {source2.stored[propName].value}");
				var taken = Mathf.Min(amount, source1.stored[propName].value);
				source1.stored[propName].value -= taken;
				source2.stored[propName].value += taken;
				if(log)
					Debug.Log($"transfer {propName} {taken}");
			}
			else if(items.Length == 6)
			{
				// transfer 1 source bucket source2 bucket2
				var command = items[0];
				var count = int.Parse(items[1]);
				var first = Find(items[2], storages, action.self.transform);
				var tfirst = first.tbuckets.Find(items[3]);
				var second = Find(items[4], storages, action.self.transform);
				var tsecond = second.tbuckets.Find(items[5]);

				var temp = Redirect(items[4], action, storages);
				var dict = temp.stored;
				count = Mathf.Min(count, tfirst.transforms.Count);
				for (int i = 0; i < count; i++)
				{
					tsecond.transforms.Add(tfirst.transforms[0]);
					tfirst.transforms.RemoveAt(0);
				}
				dict.Init(tsecond.key, 0);
				dict[tsecond.key].value += count;
			}else return false;
			return true;
		}else if(items.Length >= 4 && items[1] == "script"){
			// self script SetTarget targets 0      -> auto red self
			if(items.Length == 5){
				var write = Redirect(items[0], action, storages);
				var read = action.self;
				var bucketName = items[3];
				var id = int.Parse(items[4]);
				var bucket = self.tbuckets.Find(bucketName);
				if(bucket.transforms.Count > id){
					Transform t = self.tbuckets.Find(bucketName).transforms[id];
					var script = self.scripts[items[2]];	
					if(script != null){	
						if(script.script is ITFunc scrp)
							scrp.Func(items[2], t);
						else Debug.Log($"Unsupported script type {items[2]}", action.self);
					}else Debug.LogError($"Missing script of type ITFunc under name {items[2]}", self);
				}
			}
			// self script SetTarget self targets 0
			// 0	1		2		 3 	  4		  5  | l6
			else if(items.Length == 6){
				var write = Redirect(items[0], action, storages);
				var read = Redirect(items[3], action, storages);
				var id = items.Length == 6 ? int.Parse(items[5]) : 0;
				var bucket = self.tbuckets.Find(items[4]);
				if(bucket.transforms.Count > id){
					Transform t = self.tbuckets.Find(items[4]).transforms[id];
					var script = self.scripts[items[2]];	
					if(script != null){	
						if(script.script is ITFunc scrp)
							scrp.Func(items[2], t);
						else Debug.Log($"Unsupported script type {items[2]}", action.self);
					}else Debug.LogError($"Missing script of type ITFunc under name {items[2]}", self);
				}
			}
			else return false;
			return true;
		}
		#endregion region copy-paste
		return false;
	}
	
	public static InteractStorage Find(string name, List<InteractStorage> storages, Transform t){
		for (int i = 0; i< storages.Count; i++){
			if(storages[i] == null)
				Debug.LogError($"Storage is null, assign it.id:{i}", t);
			else if(storages[i].key == name)
				return storages[i];
		}
		if(name == "global" || name == "globals")
			Debug.Log("If crash, from global, activate auto global on object.", t);
		return null;
	}

	[System.Serializable]
	public class InteractStorageList{
		[SerializeField] List<InteractStored> items = new List<InteractStored>();
		public int Count => items.Count;
		public InteractStored this[string index]{ 
			get {
				for (int i = 0; i < Count; i++){
					if (items[i].key == index)
						return items[i];
				}
				return null;
			}
			set{
				for (int i = 0; i < Count; i++){
					if (items[i].key == index)
						items[i] = value;
				}
			}
		}
		
		public void Init(string prop, int defaultValue = 0){
			for (int i = 0; i < Count; i++){
				if (items[i].key == prop)
					return;
			}
			items.Add(new InteractStored(){ key = prop, value = defaultValue, svalue = defaultValue.ToString()});
		}
		
		public void InitStr(string prop, string defaultValue){
			for (int i = 0; i < Count; i++){
				if (items[i].key == prop)
					return;
			}
			items.Add(new InteractStored(){ key = prop, value = 0, svalue = defaultValue.ToString()});
		}
	}
}
	
[System.Serializable]
public class InteractStored{
	public string key;
	[FormerlySerializedAs("value")]
	public int _value;
	[FormerlySerializedAs("svalue")]
	public string _svalue;
	public int value { get => _value; set {
		_value = value;
		_svalue = value.ToString();
	}}
	public string svalue { get => _svalue; set {
		int.TryParse(value, out _value);
		_svalue = value;
	}}
}