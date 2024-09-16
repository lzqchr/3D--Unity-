using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarGameOverTrigger : MonoBehaviour {
	
	//reference to the game manager
	GameManager manager;
	
	void Start(){
		//find the game manager
		manager = GameObject.FindObjectOfType<GameManager>();
	}

	void OnTriggerEnter(Collider other){
		//if the top of the car hits the ground, the car fell over so we need to stop the game
		if(other.gameObject.name == "World piece")
			manager.GameOver();
	}
}
