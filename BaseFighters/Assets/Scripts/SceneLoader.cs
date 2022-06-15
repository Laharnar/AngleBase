using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
	public string levelSelectorScene = "";
	public string[] names;
	
	public void BTNUI_LoadScene(int id){
		if(id < names.Length)
			SceneManager.LoadScene(names[id]);
	}
	
	public void BTNUI_LoadSelector(){
		SceneManager.LoadScene(levelSelectorScene);
	}
}
