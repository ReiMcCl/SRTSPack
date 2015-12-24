﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BuildingPlacement : MonoBehaviour {
	ResourceManager resourceManager;
	UnitSelection unitSelect;
	Fog fog;
	bool place = false;
	Building build;
	GameObject obj;
	UGrid grid;
	public int gridI;
	int loc = 0;
	int group;
	bool aStar = false;

	public void Start () {
		if(resourceManager == null){
			resourceManager = gameObject.GetComponent<ResourceManager>();
		}
		if(fog == null){
			GameObject nObj;
			nObj = GameObject.Find("Fog");
			if(nObj)
				fog = nObj.GetComponent<Fog>();
		}
		if(unitSelect == null){
			unitSelect = gameObject.GetComponent<UnitSelection>();
		}
		if(grid == null){
			GameObject nObj;
			nObj = GameObject.Find("UGrid");
			if(nObj)
				grid = nObj.GetComponent<UGrid>();
		}
	}
	
	public void Update () {
		if (place)
			PlaceBuild();

	}


	public void BeginPlace (Building nBuild) {
		bool canPlace = true;
		for(int x = 0; x < nBuild.cost.Length; x++){
			if(nBuild.cost[x] > resourceManager.resourceTypes[x].amount){
				canPlace = false;
			}
		}
		if(canPlace){
			place = true;
			build = nBuild;
			obj = Instantiate(nBuild.tempObj, Vector3.zero, Quaternion.identity) as GameObject;
		}
	}
	
	public void SetGroup (int id){
		group = id;
	}
	
	public void PlaceBuild () {
		if(Input.GetButtonDown("RMB")){
			Destroy(obj);
			place = false;
		}
		RaycastHit hit;
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		Physics.Raycast(ray, out hit, 10000);
		for(int x = 0; x < build.cost.Length; x++){
			if(resourceManager.resourceTypes[x].amount-build.cost[x] < 0){
				Destroy(obj);
				place = false;
			}
		}
		if (hit.collider) {
			int i = grid.DetermineLoc(hit.point, gridI);
			if(grid.grids[gridI].grid[i].state){
				loc = i;
				obj.transform.position = grid.grids[gridI].grid[i].loc;
			}
			bool canPlace = false;
			if(build.CheckPoints(grid, gridI, loc) && fog.CheckLocation(obj.transform.position)){
				canPlace = true;
			}
			if(canPlace){
				if(Input.GetButtonDown("LMB")){
					GameObject tempObj;
					build.ClosePoints(grid, gridI, loc, aStar);
					if(build.autoBuild){
						tempObj = Instantiate(build.obj, obj.transform.position, obj.transform.rotation) as GameObject; 
					}
					else{
						tempObj = Instantiate(build.progressObj, obj.transform.position, obj.transform.rotation) as GameObject; 
						unitSelect.SetTarget(tempObj, tempObj.transform.position);
					}
					if(Input.GetButton("ContinuePlace")){
						
					}
					else{
						Destroy(obj);
						place = false;
					}
					for(int x = 0; x < build.cost.Length; x++){
						resourceManager.resourceTypes[x].amount-=build.cost[x];
					}
					BuildingController script = tempObj.GetComponent<BuildingController>();
					script.building = build;
					script.loc = loc;
					script.group = group;
					
				}
			}
		}
	}
}