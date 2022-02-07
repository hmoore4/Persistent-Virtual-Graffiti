using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.ARFoundation;


public class Report_Manager : MonoBehaviour
{
    // Start is called before the first frame update
	public Camera camera;
	public static Report_Manager instance;
	public const int REPORT_SIZE = 1000;
	//ARPlaneManager PM = new ARPlaneManager();
	//var planeManager = GetComponent<ARPlaneManager>();

	static List<Vector3> camera_positions = new List<Vector3>();
	static List<Quaternion> camera_rotations = new List<Quaternion>();
	static List<ARPlane> planes = new List<ARPlane>();
	static Hashtable planeHT = new Hashtable();
	static List<float> GPS_headings = new List<float>();
	static Hashtable headingsHT = new Hashtable();
	static float GPO_lat, GPO_lon,GPO_heading;

	public static int count = 0; // note for tomorrow 11 27 2020: I need to make this accessible in Network_Manager, which will initiate the upload of the Report and Key (GPS location)
	//public static Report rreport;
	//static var planeManager = GetComponent<ARPlaneManager>();

    void Start()
	{
		GPO_lat = GPS.initial_latitude;
		GPO_lon = GPS.initial_longitude;
		GPO_heading=GPS.initial_heading;
    }

    // Update is called once per frame
    void Update()
    {
		if(count>=REPORT_SIZE){
			//send report and clear lists to start another report. Make a Key of GPS location, make a value Report object of GPS

		}
        camera_positions.Add(camera.transform.position);
		camera_rotations.Add(camera.transform.rotation);



		count++;


    }
	public int getCount(){
		return count;
	}
	public int getReportSize(){
		return REPORT_SIZE;
	}
	void OnPlanesChanged(ARPlanesChangedEventArgs eventArgs){
		var planeManager = GetComponent<ARPlaneManager>();


			foreach (ARPlane plane in planeManager.trackables)
			{
			    // Do something with the ARPlane
				if(!planes.Contains(plane)){
					planes.Add(plane);
					planeHT.Add(count,plane);
					GPS_headings.Add(GPS.heading);
					headingsHT.Add(count,GPS.heading);

				}
			}


	}
}
