using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// making a way to navigate between scenes to explore more options with demo
public class SceneCtrl : MonoBehaviour {

	public void LoadScene(string sceneName){

		SceneManager.LoadScene (sceneName);
	
	}

}
