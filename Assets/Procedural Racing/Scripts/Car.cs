using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Car : MonoBehaviour {
	
	public Rigidbody rb;

	public Transform[] wheelMeshes;
	public WheelCollider[] wheelColliders;
	
	public int rotateSpeed;
	public int rotationAngle;
	public int wheelRotateSpeed;
	
	public Transform[] grassEffects;
	public Transform[] skidMarkPivots;
	public float grassEffectOffset;
	
	public Transform back;
	public float constantBackForce;
	
	public GameObject skidMark;
	public float skidMarkSize;
	public float skidMarkDelay;
	public float minRotationDifference;
	
	public GameObject ragdoll;
	
	int targetRotation;
	WorldGenerator generator;

	float lastRotation;
	bool skidMarkRoutine;
	
	void Start(){
		//find the world generator and start the skid mark coroutine
		generator = GameObject.FindObjectOfType<WorldGenerator>();
		StartCoroutine(SkidMark());
	}
	
	void FixedUpdate(){
		//update the skidmark and the grass effects
		UpdateEffects();
	}
	
	void LateUpdate(){
		//遍历轮子
		for(int i = 0; i < wheelMeshes.Length; i++){	
			//更新轮子碰撞体
			Quaternion quat;
			Vector3 pos;
			
			wheelColliders[i].GetWorldPose(out pos, out quat);
			
			wheelMeshes[i].position = pos;
			
			//旋转轮子 ，帧率无关
			wheelMeshes[i].Rotate(Vector3.right * Time.deltaTime * wheelRotateSpeed);
		}
		
		//是否转了车
		if(Input.GetMouseButton(0) || Input.GetAxis("Horizontal") != 0){
			UpdateTargetRotation();
		}
		else if(targetRotation != 0){
			//已经在上一帧旋转到了目标值 清零目标值
			targetRotation = 0;
		}
		
		//应用旋转
		Vector3 rotation = new Vector3(transform.localEulerAngles.x, targetRotation, transform.localEulerAngles.z);
		transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(rotation), rotateSpeed * Time.deltaTime);
	}
	
	void UpdateTargetRotation(){
		//鼠标控制
		if(Input.GetAxis("Horizontal") == 0){
			//使用鼠标移动 判断在左还是右
			if(Input.mousePosition.x > Screen.width * 0.5f){
				//rotate right
				targetRotation = rotationAngle;
			}
			else{
				//rotate left
				targetRotation = -rotationAngle;
			}
		}
		else{
			//键盘控制
			targetRotation = (int)(rotationAngle * Input.GetAxis("Horizontal"));
		}
	}
	
	void UpdateEffects(){
		//if both wheels are off the ground, add force will be true
		bool addForce = true;
		//c是否正在旋转
		bool rotated = Mathf.Abs(lastRotation - transform.localEulerAngles.y) > minRotationDifference;
		
		//for both grass effects (rear wheels)
		for(int i = 0; i < 2; i++){
			//同侧碰撞体数据
			Transform wheelMesh = wheelMeshes[i + 2];

			//是否在grassEffectOffset * 1.5f的距离中触碰到碰撞体
			if (Physics.Raycast(wheelMesh.position, Vector3.down, grassEffectOffset * 1.5f)){
				//启动特效
				if(!grassEffects[i].gameObject.activeSelf)
					grassEffects[i].gameObject.SetActive(true);
				
				//更新车胎痕迹位置
				float effectHeight = wheelMesh.position.y - grassEffectOffset;
				Vector3 targetPosition = new Vector3(grassEffects[i].position.x, effectHeight, wheelMesh.position.z);
				grassEffects[i].position = targetPosition;
				skidMarkPivots[i].position = targetPosition;
				
				//在地上就不加稳定力
				addForce = false;
			}
			else if(grassEffects[i].gameObject.activeSelf){
				//不在地上就关掉特效
				grassEffects[i].gameObject.SetActive(false);
			}
		}
		
		//离地就加稳定 力
		if(addForce){
			rb.AddForceAtPosition(back.position, Vector3.down * constantBackForce);
			//关闭痕迹
			skidMarkRoutine = false;
		}
		else{
			if(targetRotation != 0){
				//车辆旋转  目前没有痕迹 显示痕迹
				if(rotated && !skidMarkRoutine){
					skidMarkRoutine = true;
				}	//车辆不旋转 当前有痕迹 关闭
				else if(!rotated && skidMarkRoutine){
					skidMarkRoutine = false;
				}
			}
			else{
				//摆正不需要痕迹
				skidMarkRoutine = false;
			}
		}
		
		//update the last rotation (which is now the current rotation since everything has been updated)
		lastRotation = transform.localEulerAngles.y;
	}
	
	public void FallApart(){
		//destroy the car
		Instantiate(ragdoll, transform.position, transform.rotation);
		gameObject.SetActive(false);
	}
	
	IEnumerator SkidMark(){
		//loops continuesly
		while(true){
			//调度间隔
			yield return new WaitForSeconds(skidMarkDelay);
			
			//show skidmarks if we need skidmarks now
			if(skidMarkRoutine){
				//在特定位置生成立方体
				for(int i = 0; i < skidMarkPivots.Length; i++){
					GameObject newskidMark = Instantiate(skidMark, skidMarkPivots[i].position, skidMarkPivots[i].rotation);
					newskidMark.transform.parent = generator.GetWorldPiece();
					newskidMark.transform.localScale = new Vector3(1, 1, 4) * skidMarkSize;
				}
			}
		}
	}
}
