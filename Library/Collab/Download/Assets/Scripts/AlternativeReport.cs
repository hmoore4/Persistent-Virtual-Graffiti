using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
public class AlternativeReport : MonoBehaviour
{
    public static string originalTime;
    static float GPO_lat, GPO_lon,GPO_heading;
    public static string sessionIDToSendToPainter;

    static string temphash;

 public struct altReport
        {
            public string ID;
            public string sessionID;
            public string geohash;
          
            public string time;

            public List<float> dingdong;
            public List<Vector3> planeCenter;
            public List<Vector3> camPositionWhenPlaneFound;

            public List<float> headings;
            public List<Quaternion> cameraRotations;

            public List<ARPlane> planes;
            public List<TrackableId> trackableIds;
            public int planeBoundaryLength;
            public List<List<Vector2>> planeBoundary; 
            public List<Vector2> planeSizes;
            public List<Vector3> planeNormals;

            public List<PlaneAlignment> planeAlignments;
            public string endOfDocument;

            public void Add(float value)
            {
                if (dingdong == null)
                    dingdong = new List<float>();
                dingdong.Add(value);
            }
            public altReport(string geohash, string sessionID, string ID, string time, int planeBoundaryLength, string endOfDocument/*, Vector4 vector*/)
            {
                //var theCam = GetComponent<Camera>();

                dingdong = new List<float>();
                planes = new List<ARPlane>();
                trackableIds = new List<TrackableId>();
                planeCenter = new List<Vector3>();
                camPositionWhenPlaneFound = new List<Vector3>();
                this.planeBoundaryLength = planeBoundaryLength;
                planeBoundary = new List<List<Vector2>>();
                headings = new List<float>();
                cameraRotations= new List<Quaternion>();
                planeSizes = new List<Vector2>();
                planeNormals = new List<Vector3>();
                planeAlignments = new List<PlaneAlignment>();
                //dingdong.Add(camera.transform.position.x);
                this.geohash = geohash;
                this.ID = ID;
                this.sessionID = sessionID;
                this.time = time;
                this.endOfDocument = endOfDocument;
                //this.vector = vector;
            }
        }
    public static altReport makePlaneReport(){
        GPO_lat = GPS.latitude;
		GPO_lon = GPS.longitude;
		GPO_heading=GPS.initial_heading;
        temphash = Network_Manager.Encode(GPO_lat,GPO_lon);
        altReport newestReport = new altReport(temphash, "SessionID", SystemInfo.deviceUniqueIdentifier, DateTime.UtcNow.ToString(), 0, "~");
        
        sessionIDToSendToPainter = Network_Manager.sha256(originalTime + SystemInfo.deviceUniqueIdentifier);

        newestReport.sessionID = sessionIDToSendToPainter;

        foreach(ARPlane p in Painter.getPlaneList()){
            List<Vector2> boundaryPoints = new List<Vector2>();
            newestReport.planeCenter.Add(p.center);
            newestReport.trackableIds.Add(p.trackableId);  
            foreach(Vector2 pt in p.boundary){
                Vector2 tempVector2 = new Vector2 (pt.x, pt.y);
                boundaryPoints.Add(tempVector2);
            }
            newestReport.planeBoundary.Add(boundaryPoints);
        }
        newestReport.planeBoundaryLength = newestReport.planeBoundary.Count;


        foreach(float heading in Painter.headings){
            newestReport.headings.Add(heading);
        }

        foreach(Vector3 camPosition in Painter.camPositionsWhenPlaneFound){
            newestReport.camPositionWhenPlaneFound.Add(camPosition);
        }

        foreach(Vector2 planeDimension in Painter.planeSize){
            newestReport.planeSizes.Add(planeDimension);
        }

        foreach(Quaternion camRotation in Painter.cameraRotations){
            newestReport.cameraRotations.Add(camRotation);
        }

        foreach(Vector3 planeNormal in Painter.planeNormals){
            newestReport.planeNormals.Add(planeNormal);
        }

        foreach(PlaneAlignment planeAligment in Painter.planeAlignment){
            newestReport.planeAlignments.Add(planeAligment);
        }
        
        if(newestReport.planes == null){
            Debug.Log("ERROR: PLANE LIST EMPTY");
        }
       
        return newestReport;
    }
    // Start is called before the first frame update
    void Start()
    {
        originalTime = DateTime.UtcNow.ToString();

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
