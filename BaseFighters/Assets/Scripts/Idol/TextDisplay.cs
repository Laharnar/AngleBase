using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TextDisplay:MonoBehaviour, ITFuncStr{
	public IdolTexting target;
	public TMPro.TextMeshProUGUI text;
	public string toDisplay="";
	public float rate = 0.1f;

	IEnumerator Start(){
		while (true)
		{
			if (target != null && target.display != toDisplay && target.display != null)
				toDisplay = target.display;
			if (text != null)
			{
				text.text = "";
				for (int i = 0; i < toDisplay.Length; i++)
				{
					text.text += toDisplay[i];
					yield return new WaitForSeconds(rate);
				}
			}
			yield return null;
		}
	}
	
	public void Func(List<string> args){
		if(args[0] == "SetText"){
			if(toDisplay != args[1] && args[1]!= null)
				toDisplay = args[1];
		}
    }
}