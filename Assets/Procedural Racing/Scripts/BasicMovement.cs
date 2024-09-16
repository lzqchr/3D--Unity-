using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicMovement : MonoBehaviour {
	
	//public variables visible in the inspector
	public float movespeed;
	public float rotateSpeed;
	public bool lamp;
	
	//not visible in the inspector
	WorldGenerator generator;
	
	Car car;
	Transform carTransform;
	
	void Start(){
		//find the car and the world generator
		car = GameObject.FindObjectOfType<Car>();
		generator = GameObject.FindObjectOfType<WorldGenerator>();
		
		if(car != null)
			carTransform = car.gameObject.transform;
	}

	void Update(){
		//move towards the car
		transform.Translate(Vector3.forward * movespeed * Time.deltaTime);
		
		//if there's a car, rotate when the player car rotates
		if(car != null)
			CheckRotate();
	}
	
	void CheckRotate(){
		//the directional light rotates over an other axis than the world objects
		Vector3 direction = (lamp) ? Vector3.right : Vector3.forward;
		//get the car rotation
		float carRotation = carTransform.localEulerAngles.y;
		
		//get the left rotation (eulerAngles always returned positive rotations)
		if(carRotation > car.rotationAngle * 2f)
			carRotation = (360 - carRotation) * -1f;
		
		//rotate this object based on the direction value, speed value, car rotation and world dimensions
		transform.Rotate(direction * -rotateSpeed * (carRotation/(float)car.rotationAngle) * (36f/generator.dimensions.x) * Time.deltaTime);
	}
}
