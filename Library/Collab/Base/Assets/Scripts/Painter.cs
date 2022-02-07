/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.UI;

public class Painter: MonoBehaviour
{
   [SerializeField]
   ARRaycastManager  m_RaycastManager;
   static List<ARRaycastHit> s_Hits = new List<ARRaycastHit>();
   [SerializeField]
   Button finish;
   [SerializeField]
   GameObject m_ObjectToPlace;
   float lon;
   float lat;
   float heading;
   static List<Pose> poses = new List<Pose>();
   void Start(){
     //GPS gps = new GPS();
     while(GPS.latitude != 0 || GPS.longitude != 0){
       lat = GPS.latitude;
       lon = GPS.longitude;
       heading =GPS.heading;
     }
   }

    // Update is called once per frame
    void Update()
    {
          if(Input.touchCount > 0){
            Touch touch = Input.GetTouch(0);
            if(touch.phase == TouchPhase.Began || touch.phase == TouchPhase.Moved){
              if(m_RaycastManager.Raycast(touch.position,s_Hits, TrackableType.PlaneWithinPolygon)){
                Pose hitPose = s_Hits[0].pose;
                //Instantiate a sphere do the math then add as the parameter
                Instantiate(m_ObjectToPlace, hitPose.position,hitPose.rotation);
                poses.Add(hitPose);
              }
            }
          }
    }
    void TaskOnClick(){
      DrawingCollection dc  = new DrawingCollection(poses);
      dc.setInitialGPSHeading(lat,lon,heading);
      dc.setPseudoHash();
      Debug.Log("Drawing Saved!");
    }
 }*/




using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.UI;
using Newtonsoft.Json;
using MongoDB.Bson;
using MongoDB.Driver;



public class Painter: MonoBehaviour
{
   [SerializeField]
   ARRaycastManager  m_RaycastManager;
   static List<ARRaycastHit> s_Hits = new List<ARRaycastHit>();
   static List<ARRaycastHit> all_hits = new List<ARRaycastHit>();
   [SerializeField]
   ARPlaneManager m_PlaneManager;
   [SerializeField]
   Button finish;
   [SerializeField]
   GameObject m_ObjectToPlace;
   Vector3 camPosition;
  TrackableId filler;

   public Camera cam;
   float lon;
   float lat;
   float heading;
   int currentPosesCount=0;
   private string geohashOfDevice;
  public static List <Quaternion> cameraRotations = new List<Quaternion>();
   static public List<float> headings = new List<float>();
   public static int count = 0;
   static List<Pose> poses = new List<Pose>();
   static public List<ARPlane> planes = new List<ARPlane>();
   public static List<GameObject> objects = new List<GameObject>();
   static public List<Vector2> planeSize = new List<Vector2>();
   public static List<Vector3> camPositionsWhenPlaneFound = new List<Vector3>();
   public static List<Vector3> planeNormals = new List<Vector3>();
   public static List<PlaneAlignment> planeAlignment = new List<PlaneAlignment>();

public static IMongoClient client = new MongoClient("mongodb+srv://testUser2:testPassword@cluster0.h9orn.mongodb.net/InputDB?retryWrites=true&w=majority");



        public static IMongoDatabase database = client.GetDatabase("InputDB");
        public static IMongoCollection<BsonDocument> collection;
         public static IMongoCollection<BsonDocument> TestCasesCollection = database.GetCollection<BsonDocument>("TestCases");


  TrackableId idForStruct;
 
  public struct drawing{
    public string drawingID;

    public string geohash;
    public string sessionID;

    public TrackableId trackableIdOfPlane;
    public List<Pose> poseList;
    public string endOfDocument;

    public drawing(string drawingID, string geohash, string sessionID, TrackableId trackableIdOfPlane, string endOfDocument){
      this.drawingID = drawingID;
      this.geohash = geohash;
      this.sessionID = sessionID;

      this.trackableIdOfPlane = trackableIdOfPlane;
      this.endOfDocument = endOfDocument;
      poseList = new List<Pose>();
    }
  }

   void Start(){

   }
   //Store poses and attatch them to a plane.
   //When check for hit, store planeID of hit and the pose of the hit

    // Update is called once per frame
    void Update()
    {
          if(Input.touchCount > 0){
            Touch touch = Input.GetTouch(0);
            if(touch.phase == TouchPhase.Began || touch.phase == TouchPhase.Moved){
              if(m_RaycastManager.Raycast(touch.position,s_Hits, TrackableType.PlaneWithinPolygon)){
                ARRaycastHit hit = s_Hits[0];
                TrackableId planeId = hit.trackableId;
                idForStruct = hit.trackableId;
                Pose hitPose = hit.pose;
                GameObject obj = Instantiate(m_ObjectToPlace, hitPose.position, hitPose.rotation);
                Instantiate(m_ObjectToPlace, hitPose.position,hitPose.rotation);
                poses.Add(hitPose);
                objects.Add(obj);
              }
            }
          }
        if(m_RaycastManager.Raycast(cam.transform.position, all_hits, TrackableType.PlaneWithinPolygon)){
          int i = 0;
          foreach(ARRaycastHit id in all_hits){
            
            TrackableId tid = all_hits[all_hits.Count-1].trackableId;
            //Debug.Log("TID = " + id.trackableId);
          
            ARPlane p = m_PlaneManager.GetPlane(tid);
            //Debug.Log("PLANE p = " + p);
            if(p == null){
              Debug.Log(tid + "is null");
            }
            else{
              if(planes.Contains(p) == false){
                count++;
                camPosition = Camera.main.transform.position;
                planes.Add(p);
                cameraRotations.Add(Camera.main.transform.rotation);
                headings.Add(GPS.heading);
                planeSize.Add(p.size);
                planeNormals.Add(p.normal);
                planeAlignment.Add(p.alignment);
               // Debug.Log("PLANE CENTER FROM PAINTER: " + p.center);
                //Debug.Log("PLANE FOUND FROM PAINTER " + p);
                camPositionsWhenPlaneFound.Add(Camera.main.transform.position);
                //Debug.Log("found id: " + tid + "with center: " + p.center + "and boundary: " + p.boundary);
              }
            }
            i++;
          }

          

      }
      if(poses.Count > currentPosesCount){
       // drawingUpload();
        currentPosesCount = poses.Count;

       }
    }

    public void drawingUpload(){

      lat = GPS.latitude;
      lon = GPS.longitude;
      geohashOfDevice = Network_Manager.Encode(lat, lon);
      drawing drawingReport = new drawing("Drawing ID Dummy", geohashOfDevice, "SessionIDHere", filler , "~");
      drawingReport.sessionID = Network_Manager.sessionIDToSendToPainter;
      drawingReport.trackableIdOfPlane = idForStruct;
      drawingReport.drawingID = Network_Manager.sha256(Network_Manager.originalTime + SystemInfo.deviceUniqueIdentifier + "drawing");
      foreach(Pose pose in poses){
        drawingReport.poseList.Add(pose);
      }
      string json = JsonConvert.SerializeObject(drawingReport,  new JsonSerializerSettings()
      { 
        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
      });

      byte[] myData = System.Text.Encoding.UTF8.GetBytes(json);



      GameObject.FindWithTag("Network Tag").GetComponent<Network_Manager>().SendData(myData);

    }
    /*public drawing parseServerReport(string Report){
      drawing serverDrawing = new drawing("drawingID", "geohash", "sessionID", filler,  "endOfDocument");
      int firstStringPosition = Report.IndexOf("geohash");
      serverDrawing.geohash = Report.Substring(firstStringPosition + 5, firstStringPosition + 14);

      return serverDrawing;
    }*/
  

    void FixedUpdate(){
      /*if(m_RaycastManager.Raycast(cam.transform.position, all_hits, TrackableType.PlaneWithinPolygon)){
        TrackableId tid = all_hits[0].trackableId;
        Debug.Log("TID = " + tid);
        ARPlane p = m_PlaneManager.GetPlane(tid);
        Debug.Log("PLANE p = " + p);

        if(p == null){
          Debug.Log(tid + "is null");
        }
        else{
          if(!planes.Contains(p)){
            camPosition = Camera.main.transform.position;
            planes.Add(p);
             Debug.Log("PLANE CENTER FROM PAINTER: " + p.center);
             Debug.Log("PLANE FOUND FROM PAINTER " + p);
            camPositionsWhenPlaneFound.Add(Camera.main.transform.position);
            Debug.Log("found id: " + tid + "with center: " + p.center + "and boundary: " + p.boundary);
          }
        }
      }*/
    }


    public static List<ARPlane> getPlaneList(){
      return planes;
    }

   public static List<float> getHeadingList(){
      return headings;
    }

    public static string getPlaneInfo(Report r){
      string toSend = "";
      List<TrackableId> ids = new List<TrackableId>();
      foreach(ARPlane p in planes){
        if(ids.Contains(p.trackableId) == false){
          toSend = toSend + /*"location: " + r.getGeoHash() +*/ "plane id:" + p.trackableId + "plane center: " + p.center;
        }
      }
      return toSend;
    }

    public Vector3 getCameraPosition(){
      return camPosition;
    }
    public static List<Pose> getPoses(){
      return poses;
    }
    public static List<GameObject> getObjects(){
      return objects;
    }
 }
