using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdolTexting : MonoBehaviour, ITFuncStr
{
	public string transmissionId = "69";
	public string text;
	List<string> lines = new List<string>();
	int active = 0;
	internal string display;
	
    void Start()
    {
		lines.Add($"------- Playing recorded transmission {transmissionId} -------");
        lines.AddRange(text.Split("\n"));
		lines.Add($"------- Ending transmission {transmissionId} -------");
    }

    void Update()
    {
        display = lines[active++];
		active %= lines.Count;
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