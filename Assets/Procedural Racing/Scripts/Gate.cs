using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gate : MonoBehaviour {
	
	//visible in the inspector (reference to the gate audio)
	public AudioSource scoreAudio;
	
	//not visible in the inspector
	GameManager manager;
	bool addedScore;
	
	void Start(){
		//find the game manager
		manager = GameObject.FindObjectOfType<GameManager>();
	}
	
	void OnTriggerEnter(Collider other){
		//check if the player drove through this gate and we didn't yet add any points
		if(!other.gameObject.transform.root.CompareTag("Player") || addedScore)
			return;
		
		//increase the score by one and play some audio
		addedScore = true;
		manager.UpdateScore(1);
		scoreAudio.Play();
	}
}
