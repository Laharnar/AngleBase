using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TextDisplay:MonoBehaviour, ITFuncStr{
	public IdolTexting target;
	public TMPro.TextMeshProUGUI text;
	
	void Update(){
		if(text!= null && target != null)
			text.text = target.display;
	}
	
	public void Func(List<string> args){
		if(args[0] == "SetText"){
			text.text = args[1];
		}
    }
}