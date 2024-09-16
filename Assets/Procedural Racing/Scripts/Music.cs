using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Music : MonoBehaviour {
	
	//music instance
	private static Music instance;
 
    void Awake(){
		//check instance and if there is one already, destroy this object
        if(!instance){
            instance = this;
		}
		else{
            Destroy(gameObject);
		}
		
		//make sure this object won't be destroyed (so background music keeps playing)
        DontDestroyOnLoad(this.gameObject);
    }
}
