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
		//	public static Report_Manager Report = new Report_Manager(camera_positions, camera_rotations, planes, planeHT, GPS_headings, headingsHT);

	static string LocationHash = Encode(GPS.latitude, GPS.longitude); 
	public static List<Vector3> camera_positions = new List<Vector3>();
	public static List<Quaternion> camera_rotations = new List<Quaternion>();
	public static List<ARPlane> planes = new List<ARPlane>();
	public static Hashtable planeHT = new Hashtable();
	public static List<float> GPS_headings = new List<float>();
	public static Hashtable headingsHT = new Hashtable();
	static float GPO_lat, GPO_lon,GPO_heading;


	public static int count = 0; // note for tomorrow 11 27 2020: I need to make this accessible in Network_Manager, which will initiate the upload of the Report and Key (GPS location)
	//public static Report rreport;
	//static var planeManager = GetComponent<ARPlaneManager>();
			public static Report_Manager Report = new Report_Manager(camera_positions, camera_rotations, planes, planeHT, GPS_headings, headingsHT);

	public Report_Manager(List<Vector3> camera_positions_list, List<Quaternion> camera_rotations_list, List<ARPlane> planes_list, Hashtable planeHT_table, List<float> GPS_headings_list, Hashtable headingsHT_table){
		camera_positions = camera_positions_list;
		camera_rotations = camera_rotations_list;
		planes = planes_list;
		planeHT = planeHT_table;
		GPS_headings = GPS_headings_list;
		headingsHT = headingsHT_table;
	}

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

	 private static string Encode(double latitude, double longitude, int numberOfChars = 9)
    {   const string Base32Codes = "0123456789bcdefghjkmnpqrstuvwxyz";
       
       // string Base32CodesFeeder = "0,1,2,3,4,5,6,7,8,9,b,c,d,e,f,g,h,j,k,m,n,p,q,r,s,t,u,v,w,x,y,z";
       // List <string> tempSTRlist = Base32CodesFeeder.Split(',').ToList();
        
       // Dictionary<char, int> Base32CodesDict = tempSTRlist.ToDictionary(chr => chr, chr => tempSTRlist.IndexOf(chr));
        
        var chars = new List<char>();
        var bits = 0;
        var bitsTotal = 0;
        var hashValue = 0;
        var maxLat = 90D;
        var minLat = -90D;
        var maxLon = 180D;
        var minLon = -180D;
        while (chars.Count < numberOfChars)
        {
            double mid;
            if (bitsTotal % 2 == 0)
            {
                mid = (maxLon + minLon) / 2;
                if (longitude > mid)
                {
                    hashValue = (hashValue << 1) + 1;
                    minLon = mid;
                }
                else
                {
                    hashValue = (hashValue << 1) + 0;
                    maxLon = mid;
                }
            }
            else
            {
                mid = (maxLat + minLat) / 2;
                if (latitude > mid)
                {
                    hashValue = (hashValue << 1) + 1;
                    minLat = mid;
                }
                else
                {
                    hashValue = (hashValue << 1) + 0;
                    maxLat = mid;
                }
            }

            bits++;
            bitsTotal++;
            if (bits != 5)
            {
                continue;
            }

            var code = Base32Codes[hashValue];
            chars.Add(code);
            bits = 0;
            hashValue = 0;
        }

        return string.Join("", chars.ToArray());
    }
}
