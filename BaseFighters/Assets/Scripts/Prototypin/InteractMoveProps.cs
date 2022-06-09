using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractMoveProps : MonoBehaviour
{
	public string prop;
	public int value;
	public InteractStorage storage;
	
    public void UpdateProp(){
		storage.SetPropInt(prop, value.ToString());
	}
}
