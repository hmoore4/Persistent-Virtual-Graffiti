using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.ARFoundation;

[Serializable]
public class Report : MonoBehaviour
{
    private string geo_hash;
	private string device_ID;
	private string time;
	private float GPO_lat,GPO_lon, GPO_heading;
	private List<Vector3> cam_positions;
	private  List<Quaternion> cam_rotations;
	private  List<ARPlane> planes_witnessed;
	private Hashtable planes_hashtable;
	private List<float> GPS_headings;
	private Hashtable Heads_HT;
	private int update_num;

	private  List<List<string>> cam_positions_as_string;

	//constructor

	public void ReportConstructor(string geo_hash, string device_ID, string time, float GPO_lat, float GPO_lon, float GPO_heading, List<Vector3> cam_positions, List<Quaternion> cam_rotations, List<ARPlane> planes_witnessed, Hashtable planes_hashtable, List<float> GPS_headings,Hashtable Heads_HT,int update_num, List<List<string>> cam_positions_as_string)
	{
		this.geo_hash=geo_hash;
		this.device_ID=device_ID;
		this.time=time;
		this.GPO_lat=GPO_lat;
		this.GPO_lon=GPO_lon;
		this.GPO_heading=GPO_heading;
		this.cam_positions=cam_positions;
		this.cam_rotations=cam_rotations;
		this.planes_witnessed=planes_witnessed;
		this.planes_hashtable=planes_hashtable;
		this.GPS_headings=GPS_headings;
		this.Heads_HT=Heads_HT;
		this.update_num=update_num;
		this.cam_positions_as_string = cam_positions_as_string;

	}

	public void addCamPos(Vector3 new_cam_pos){

		cam_positions.Add(new_cam_pos);
	}

	public void addCamPosAsString(string x, string y, string z){
		cam_positions_as_string.Add(new List<string> {x, y, z});
	}

	public List<Vector3> getCamPos(){
		return cam_positions;
	}

	public void updateDeviceID(string dev_id){

		device_ID=dev_id;
	}
	public void addCamRot(Quaternion new_cam_rot){

		cam_rotations.Add(new_cam_rot);
	}

	public void addPlane(ARPlane new_plane){

		planes_witnessed.Add(new_plane);
	}

	public void addPlaneHash(int update_num, ARPlane new_plane){

		planes_hashtable.Add(update_num,new_plane);
	}

	public void addHeadingsHash(int update_num, float heading){

		Heads_HT.Add(update_num,heading);
	}

	public bool planeListContains(ARPlane plane){

		if(planes_witnessed.Contains(plane)){
			return true;
		}else return false;
	}




}
