using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdolTexting : MonoBehaviour, ITFuncStr
{
	public string transmissionId = "69";
	public string[] text;
	List<string> lines = new List<string>();
	int active = 0;
	internal string display;
	public float rate = 1f;
	float time;
	
    void Start()
    {
		lines.Add($"------- Playing recorded transmission {transmissionId} -------");
		for (int i = 0; i < text.Length; i++){
			lines.AddRange(text[i].Split("."));
		}
		lines.Add($"------- Ending transmission {transmissionId} -------");
    }

    void Update()
    {
		if (Time.time > time)
		{
			display = lines[active++];
			active %= lines.Count;
			time = Time.time + rate;
		}
    }
	
	public void Func(List<string> args){
		if(args[0] == "AddText"){
			lines.Add(args[1]);
		}
		else if(args[0] == "ClearText"){
			lines.Clear();
		}
    }
}