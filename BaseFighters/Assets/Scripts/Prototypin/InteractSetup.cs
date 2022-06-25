using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class InteractSetup : MonoBehaviour
{
	public bool all = true;
	public bool removeZOrder = true;
	
    public bool remove = false;
	
	void OnEnable(){
		if(all){
			if(GetComponent<InteractState>() == null)
				gameObject.AddComponent<InteractState>();
			if(GetComponent<InteractStorage>() == null)
				gameObject.AddComponent<InteractStorage>();
			if(GetComponent<InteractModule>() == null)
				gameObject.AddComponent<InteractModule>();
			if(GetComponent<InteractPickup>() == null)
				gameObject.AddComponent<InteractPickup>();
		}
	}
	
	void LateUpdate(){
		if (remove){
			DestroyImmediate(GetComponent<InteractStorage>());
			DestroyImmediate(GetComponent<InteractModule>());
			DestroyImmediate(GetComponent<InteractPickup>());
			DestroyImmediate(GetComponent<InteractState>());
			DestroyImmediate(this);
			remove =false;
		}
		if(removeZOrder){
			if(GetComponent(typeof(IZControl)) != null)
				DestroyImmediate(GetComponent(typeof(IZControl)));
		}
	}
}

public interface IZControl{

}

namespace Common{
	public partial class ZOrderController : IZControl{
	}
}
