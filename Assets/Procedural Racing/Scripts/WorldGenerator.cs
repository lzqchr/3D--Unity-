using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class WorldGenerator : MonoBehaviour {
	
	//variables visible in the inspector
	public Material meshMaterial;
	//两个点距离
	public float scale;
	//平面二维点数量
	public Vector2 dimensions;
	//perlin 平滑参数 越小越平滑
	public float perlinScale;
    public float waveHeight;
	//自定义偏移量
    public float offset;
	public float randomness;
	public float globalSpeed;
	public int startTransitionLength;
	public BasicMovement lampMovement;
	public GameObject[] obstacles;
	public GameObject gate;
	public int startObstacleChance;
	public int obstacleChanceAcceleration;
	public int gateChance;
	public int showItemDistance;
	public float shadowHeight;
	
	//not visible in the inspector
	Vector3[] beginPoints;
	
	GameObject[] pieces = new GameObject[2];
	
	GameObject currentCylinder;
	
	void Start(){
		//create an array to store the begin vertices for each world part (we'll need that to correctly transition between world pieces) 
		beginPoints = new Vector3[(int)dimensions.x + 1];
		
		//两个地图交替使用
		for(int i = 0; i < 2; i++){
			GenerateWorldPiece(i);
		}
	}
	
	void LateUpdate(){
		//目前已经开到第二个地点
		if(pieces[1] && pieces[1].transform.position.z <= 0)
			StartCoroutine(UpdateWorldPieces());
		
		//检测物体 是否渲染
		UpdateAllItems();
	}
	
	void UpdateAllItems(){
	
		GameObject[] items = GameObject.FindGameObjectsWithTag("Item");
		
		for(int i = 0; i < items.Length; i++){
		
			foreach(MeshRenderer renderer in items[i].GetComponentsInChildren<MeshRenderer>()){
				//如果需要显示物品，则根据物品的 y 坐标更新其阴影投射模式。只有在圆柱体下半部分的物品才会投射阴影，以避免在圆柱体顶部看到奇怪的阴影。
				bool show = items[i].transform.position.z < showItemDistance;

				
				if (show)
					renderer.shadowCastingMode = (items[i].transform.position.y < shadowHeight) ? ShadowCastingMode.On : ShadowCastingMode.Off;
				
				//only enable the renderer if we want to show this item
				renderer.enabled = show;
			}
		}
	}
	
	void GenerateWorldPiece(int i){
		//create a new cylinder and put it in the pieces array
		pieces[i] = CreateCylinder();
		//创建完后移动
		pieces[i].transform.Translate(Vector3.forward * (dimensions.y * scale * Mathf.PI) * i);
		
		//update this piece so it will have an endpoint and it will move etc.
		UpdateSinglePiece(pieces[i]);
	}
	
	IEnumerator UpdateWorldPieces(){
		//销毁走完的
		Destroy(pieces[0]);
		
	
		pieces[0] = pieces[1];
		

		pieces[1] = CreateCylinder();
		
		//调整两个地图位置
		pieces[1].transform.position = pieces[0].transform.position + Vector3.forward * (dimensions.y * scale * Mathf.PI);
		pieces[1].transform.rotation = pieces[0].transform.rotation;
		
		//update this newly generated world piece
		UpdateSinglePiece(pieces[1]);
		
		//wait a frame
		yield return 0;
	}
	
	void UpdateSinglePiece(GameObject piece){
		//增加移动组件
		BasicMovement movement = piece.AddComponent<BasicMovement>();
		//设置globalspeed
		movement.movespeed = -globalSpeed;
		
		//set the rotate speed to the lamp (directional light) rotate speed 
		if(lampMovement != null)
			movement.rotateSpeed = lampMovement.rotateSpeed;
		
		//设置终止点
		GameObject endPoint = new GameObject();
		endPoint.transform.position = piece.transform.position + Vector3.forward * (dimensions.y * scale * Mathf.PI);
		endPoint.transform.parent = piece.transform;
		endPoint.name = "End Point";
		
		//change the perlin noise offset to make sure each piece is different from the last one
		offset += randomness;
		
		//change the obstacle chance which means there will be more obstacles over time
		if(startObstacleChance > 5)
			startObstacleChance -= obstacleChanceAcceleration;
	}

	public GameObject CreateCylinder(){
		//新建基类
		GameObject newCylinder = new GameObject();
		newCylinder.name = "World piece";
		
		
		currentCylinder = newCylinder;
		
		//设置渲染组件
		MeshFilter meshFilter = newCylinder.AddComponent<MeshFilter>();
		MeshRenderer meshRenderer = newCylinder.AddComponent<MeshRenderer>();

		//设置渲染组件
		meshRenderer.material = meshMaterial;
		//生成mesh
		meshFilter.mesh = Generate();	
		
		//after creating the mesh, add a collider that matches the new mesh
		newCylinder.AddComponent<MeshCollider>();
		
		return newCylinder;
	}
	
	//this will return the mesh for our new world piece
	Mesh Generate(){
		//create and name a new mesh
		Mesh mesh = new Mesh();
		mesh.name = "MESH";
		
		//新建顶点 uv 三角数组
		Vector3[] vertices = null;
		Vector2[] uvs = null;
		int[] triangles = null;
		
		//赋值数组 创建图形
		CreateShape(ref vertices, ref uvs, ref triangles);
		
		//组件赋值
		mesh.vertices = vertices;
		mesh.uv = uvs;
		mesh.triangles = triangles;
		
		//recalculate the normals for our world piece
		mesh.RecalculateNormals();
		
		return mesh;
	}
	
	void CreateShape(ref Vector3[] vertices, ref Vector2[] uvs, ref int[] triangles){
		
		//xz表示需要的点数
		int xCount = (int)dimensions.x;
		int zCount = (int)dimensions.y;
		
		//初始化数组
		vertices = new Vector3[(xCount + 1) * (zCount + 1)];
		uvs = new Vector2[(xCount + 1) * (zCount + 1)];
		
		int index = 0;
		
		//粗略计算半径
		float radius = xCount * scale * 0.5f;
		
		//两重循环依靠三角函数计算xyz坐标
		for(int x = 0; x <= xCount; x++){
			for(int z = 0; z <= zCount; z++){
				//get the angle in the cylinder to position this vertice correctly
				float angle = x * Mathf.PI * 2f/xCount;

				//计算xyz坐标
				vertices[index] = new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, z * scale * Mathf.PI);
				
				//uv赋值
				uvs[index] = new Vector2(x * scale, z * scale);
				
				//开始使用perlincale
				float pX = (vertices[index].x * perlinScale) + offset;
				float pZ = (vertices[index].z * perlinScale) + offset;
				
				//确定中心点
				Vector3 center = new Vector3(0, 0, vertices[index].z);
				//赋值随机数 创造起伏
				vertices[index] += (center - vertices[index]).normalized * Mathf.PerlinNoise(pX, pZ) * waveHeight;
				
				//做两个地图的连接
				
				//check if there are begin points and if we're at the start of the mesh (z means the forward direction, so through the cylinder)
				if(z < startTransitionLength && beginPoints[0] != Vector3.zero){
					//if so, we must combine the perlin noise value with the begin points
					//we need to increase the percentage of the vertice that comes from the perlin noise 
					//and decrease the percentage from the begin point
					//this way it will transition from the last world piece to the new perlin noise values
					
					//the percentage of perlin noise in the vertices will increase while we're moving further into the cylinder
					float perlinPercentage = z * (1f/startTransitionLength);
				
					Vector3 beginPoint = new Vector3(beginPoints[x].x, beginPoints[x].y, vertices[index].z);
					
				
					vertices[index] = (perlinPercentage * vertices[index]) + ((1f - perlinPercentage) * beginPoint);
				}
				else if(z == zCount){
					
					beginPoints[x] = vertices[index];
				}
				
				//指定概率 而且障碍物空
				if(Random.Range(0, startObstacleChance) == 0 && !(gate == null && obstacles.Length == 0))
					CreateItem(vertices[index], x);
				
				//increase the current vertice index
				index++;
			}
		}
		
		//初始化三角形数组
		triangles = new int[xCount * zCount * 6];
		
		//创建正方形数组
		int[] boxBase = new int[6];
		
		int current = 0;
		
		//设置ibo 原理和顶点生成顺序相关
		for(int x = 0; x < xCount; x++){
		
			boxBase = new int[]{ 
				x * (zCount + 1), 
				x * (zCount + 1) + 1,
				(x + 1) * (zCount + 1),
				x * (zCount + 1) + 1,
				(x + 1) * (zCount + 1) + 1,
				(x + 1) * (zCount + 1),
			};
			
			
			//for all z positions
			for(int z = 0; z < zCount; z++){
				//increase all vertice indexes in the box by one to go to the next square on this z row
				for(int i = 0; i < 6; i++){
					boxBase[i] = boxBase[i] + 1;
				}
				
				//assign 2 new triangles based upon 6 vertices to fill in one new square
				for(int j = 0; j < 6; j++){					
					triangles[current + j] = boxBase[j] - 1;
				}
				
				//now increase current by 6 to go to the next square
				current += 6;
			}
		}
	}
	
	void CreateItem(Vector3 vert, int x){
		//获得中心
		Vector3 zCenter = new Vector3(0, 0, vert.z);
		
		//计算角度 特定角度不生成
		if(zCenter - vert == Vector3.zero || x == (int)dimensions.x/4 || x == (int)dimensions.x/4 * 3)
			return;
		
		//随机生成
		GameObject newItem = Instantiate((Random.Range(0, gateChance) == 0) ? gate : obstacles[Random.Range(0, obstacles.Length)]);
		
		//调整朝向
		newItem.transform.rotation = Quaternion.LookRotation(zCenter - vert, Vector3.up);
		//设置位置
		newItem.transform.position = vert;
		
		//确定父节点
		newItem.transform.SetParent(currentCylinder.transform, false);
	}
	
	public Transform GetWorldPiece(){
	
		return pieces[0].transform;
	}
}
