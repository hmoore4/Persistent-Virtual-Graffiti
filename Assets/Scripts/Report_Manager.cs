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
	static string deviceID = "11111";

	static string timeOfReport = DateTime.UtcNow.ToString();
		static List<List<string>> cam_positions_as_string;



	public static int count = 0; // note for tomorrow 11 27 2020: I need to make this accessible in Network_Manager, which will initiate the upload of the Report and Key (GPS location)
	//public static Report rreport;
	//static var planeManager = GetComponent<ARPlaneManager>();
	//public Report theReport
	public static Report theReport;
	//public static AlternativeReport alternativeReport;//MAYBE PUT THIS IN NETWORK_MANAGER
	public static Report_Manager instance;

    void Start()
	{

		GPO_lat = GPS.initial_latitude;
		GPO_lon = GPS.initial_longitude;
		GPO_heading=GPS.initial_heading;
		string tempHash = Network_Manager.Encode(GPO_lat,GPO_lon);
		Debug.Log("tempHash: "+ tempHash);
		theReport = this.gameObject.AddComponent(typeof(Report)) as Report;
		theReport.ReportConstructor(tempHash , deviceID, timeOfReport, GPO_lat, GPO_lon, GPO_heading, camera_positions, camera_rotations, planes, planeHT,GPS_headings,headingsHT,count, cam_positions_as_string);

		theReport.updateDeviceID(SystemInfo.deviceUniqueIdentifier);
		//Report theReport= new Report( Network_Manager.Encode(GPO_lat,GPO_lon),deviceID, timeOfReport, GPO_lat, GPO_lon, GPO_heading, camera_positions, camera_rotations, planes, planeHT,GPS_headings,headingsHT,count);
	}

    // Update is called once per frame
    void Update()
    {
			 List<Vector3> cam_positions = theReport.getCamPos();

		if(count>=REPORT_SIZE && Network_Manager.hasSent < 1){
			//send report and clear lists to start another report. Make a Key of GPS location, make a value Report object of GPS
			count++;
			//Debug.Log("THE REPORT - SHOULD BE PRINTING A LOT: " + theReport.getCamPos());//NEED TO TEST

			//count = 0;
		}

		//while(count<REPORT_SIZE){
		else{
			//camera_positions.Add(camera.transform.position);
			//camera_rotations.Add(camera.transform.rotation);
			//Vector3 cameraPosition = camera.transform.position;
			//Quaternion cameraRotation = camera.transform.rotation;
			theReport.addCamPos(camera.transform.position);
			theReport.addCamRot(camera.transform.rotation);
		//Debug.Log("THE REPORT - SHOULD BE PRINTING A LOT: " + theReport);//NEED TO TEST



			count++;



		}
		/*int counter = 0;
		for(int i = 0; i < camera_positions.Count-1; i++){
			Debug.Log("CAM POSITIONS " + cam_positions[i]);
			List<string> list = SerializeVector3ToString(cam_positions[i]);		
			cam_positions_as_string.Add(list);
			Debug.Log("w");

			Debug.Log("CAM POS: " + cam_positions[i]);
			Debug.Log("P");
			Debug.Log("COUNTER " + counter++);
			Debug.Log("MAX " + (camera_positions.Count-1));
						//HERE

		}*/

		//Debug.Log("R");


    }
	public static int getCount(){
		return count;
	}
	public int getReportSize(){
		return REPORT_SIZE;
	}
	public Report getReport(){
		return theReport;
	}
	void OnPlanesChanged(ARPlanesChangedEventArgs eventArgs){
		var planeManager = GetComponent<ARPlaneManager>();


			foreach (ARPlane plane in planeManager.trackables)
			{
			    // Do something with the ARPlane
				if(!theReport.planeListContains(plane)) {
					theReport.addPlane(plane);
					theReport.addPlaneHash(count,plane);
					//theReport.GPS_headings.Add(GPS.heading);
					theReport.addHeadingsHash(count,GPS.heading);

				}
			}


	}
	            //TODO:: THE VECTOR3 CAM_POSITIONS LIST SHOULD NOT BE A PART OF THE REPORT. IT NEEDS TO BE HARVESTED, AND REMOVED
            //       NEED TO PASS THE NEW LIST (ADDCAMPOSASSTRING) TO THE DB

		//Converts an array of vector3 to strings
	/*public static void SerializeVector3ArrayToString(Vector3[] aVectors)
    {
        string X;
        string Y;
        string Z;
        foreach (Vector3 v in aVectors)
        {
            X = Network_Manager.EncodeDouble(v.x);
            Y = Network_Manager.EncodeDouble(v.y);
            Z = Network_Manager.EncodeDouble(v.z);
        	theReport.addCamPosAsString(X, Y, Z);
			Debug.Log("THERE");
        }
    }


		//Converts a single vector3 to a string
	public static List<string> SerializeVector3ToString(Vector3 Vector)
    {

        string X;
        string Y;
        string Z;

        X = Network_Manager.EncodeDouble(Vector.x);
		Debug.Log("X " + Vector.x);
        Y = Network_Manager.EncodeDouble(Vector.y);
		Debug.Log("y " + Vector.y);

        Z = Network_Manager.EncodeDouble(Vector.z);
		Debug.Log("z " + Vector.z);
		List<string> cam_positions_as_string = new List<string> {X, Y, Z};
        //theReport.addCamPosAsString(X, Y, Z);
        Debug.Log("J");
		return cam_positions_as_string;

    }*/

}
