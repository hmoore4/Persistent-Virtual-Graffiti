
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
 using UnityEngine.Events;
 using Newtonsoft.Json.Linq;

 using UnityEngine.EventSystems;
 using UnityEngine.XR.ARFoundation;
 using UnityEngine.XR.ARSubsystems;
 using UnityEngine.UI;
 using Newtonsoft.Json;
 using MongoDB.Bson;
 using MongoDB.Driver;
using System;

public class Painter: MonoBehaviour
{
   [SerializeField]
   ARRaycastManager  m_RaycastManager;
   static List<ARRaycastHit> s_Hits = new List<ARRaycastHit>();
   static List<ARRaycastHit> all_hits = new List<ARRaycastHit>();
   [SerializeField]
   ARPlaneManager m_PlaneManager;
   [SerializeField]
   Button apply;
   [SerializeField]
   public GameObject m_ObjectToPlace;
   [SerializeField]
   public Slider Rslider;
   [SerializeField]
   public Slider Gslider;

   [SerializeField]
   public Slider Bslider;
   [SerializeField]
   public Graphic previewGraphic;
   [SerializeField]
   public Button sprayToggle;
   Vector3 camPosition;
   public Camera cam;
   float lon;
   float lat;
   float heading;
   public Color color;
   TrackableId filler;
   public static Vector2 screenCenter;
   public static List <Quaternion> cameraRotations = new List<Quaternion>();
   private string geohashOfDevice;
   static public List<float> headings = new List<float>();
    public ARPlane tempWPlane;

   public ARPlane tempEPlane;

   public ARPlane tempNPlane;

   public ARPlane tempSPlane;

   public ARPlane tempfloorPlane;
   public static int count = 0;
   static List<Pose> poses = new List<Pose>();
   public static List<GameObject> objects = new List<GameObject>();
   static public List<ARPlane> planes = new List<ARPlane>();
   static public List<Vector2> planeSize = new List<Vector2>();
   public static List<Vector3> camPositionsWhenPlaneFound = new List<Vector3>();
   public static List<Vector3> planeNormals = new List<Vector3>();
  public static List<float> radii = new List<float>();

   public static bool spray;
   public static List<PlaneAlignment> planeAlignment = new List<PlaneAlignment>();
   public GameObject master;
   TrackableId idForStruct;
   public static double pi = Math.PI;

public static double THETA = Math.Sin(pi/12);
//Add 2 float []s, another float [] named headings, and vector3 []. AxialDistances, PlaneCentersToArt, Headings, PlaneCenter

  public struct drawing{
    public string drawingID;
    public string geohash;
    public string sessionID;
    //public TrackableId trackableIdOfPlane;
    public string trackableIdAsString;
    public List<Pose> poseList;
    public List<float> paintList;
    public List<Vector4> paintColors;
    public List<float> headings;
    public List<float> AxialDistances;
    public List<float> PlaneCentersToArt;
    public List<Vector3> PlaneCenters;
    public List<float> CameraRotationsY;


    public string endOfDocument;


    public drawing(string drawingID ,string geohash, string sessionID, string trackableIdAsString, string endOfDocument){
      this.drawingID = drawingID;
      this.geohash = geohash;
      this.sessionID = sessionID;
//      this.trackableIdOfPlane = trackableIdOfPlane;
      this.trackableIdAsString = trackableIdAsString;
      this.endOfDocument = endOfDocument;
      poseList = new List<Pose>();
      paintList = new List<float>();
      paintColors = new List<Vector4>();
      headings = new List<float>();
      AxialDistances = new List<float>();
      PlaneCentersToArt = new List<float>();
      PlaneCenters = new List<Vector3>();
      CameraRotationsY = new List<float>();
    }
  }
   void Start(){
     //m_PlaneManager.planesChanged += Network_Manager.Upload;
     screenCenter = new Vector2(Screen.width / 2, (Screen.height * 15)/16 ); //try 559 next
     Rslider.onValueChanged.AddListener(delegate{ColorChange();});
     Gslider.onValueChanged.AddListener(delegate{ColorChange();});
     Bslider.onValueChanged.AddListener(delegate{ColorChange();});

     apply.onClick.AddListener(CreateSphere);
     //prayToggle.OnPointerDown(EventData);
   }

   public void CreateSphere(){

     GameObject sphere = Instantiate(m_ObjectToPlace);
     Material m = sphere.GetComponent<Renderer>().material;
     m.SetColor("_Color", color);
     setMaster(sphere);
     Rslider.value = 0;
     Gslider.value = 0;
     Bslider.value = 0;

   }
   /*public void paint(){
         if(m_RaycastManager.Raycast(screenCenter,s_Hits, TrackableType.PlaneWithinPolygon)){
           ARRaycastHit hit = s_Hits[0];
           TrackableId planeId = hit.trackableId;
           ARPlane plane = m_PlaneManager.GetPlane(planeId);
           Vector3 newA = DistanceFromPlane(plane.center);
           Pose hitPose = hit.pose;
           GameObject temp = getMaster();
           Color c = temp.GetComponent<Renderer>().material.color;
           c.a = SetAlpha(newA);
           Debug.Log(c);
           temp.GetComponent<Renderer>().material.SetColor("_Color", c);
           Instantiate(temp/*getMaster()*, hitPose.position,hitPose.rotation);
           poses.Add(hitPose);
           objects.Add(/*getMaster()*temp);
           idForStruct = planeId;
         }
    }*/
    public void paint(){

         // diameter of phone camera is 4.5mm

         if(m_RaycastManager.Raycast(screenCenter,s_Hits, TrackableType.PlaneWithinPolygon)){

           ARRaycastHit hit = s_Hits[0];

           TrackableId planeId = hit.trackableId;

           ARPlane plane = m_PlaneManager.GetPlane(planeId);

           Vector3 newA = DistanceFromPlane(plane.center);

           Pose hitPose = hit.pose;

           //GameObject temp = getMaster();
           GameObject temp = Instantiate(getMaster());


           float nw = Convert.ToSingle(getNewRad(s_Hits[0]));

           radii.Add(nw);

           float scaleChange = temp.transform.localScale.z - nw;

           Debug.Log("Scale Change: " + scaleChange);

           Vector3 newScale = new Vector3(scaleChange, 0, scaleChange);

           temp.transform.localScale -= newScale;

           Debug.Log("Local Scale is: " + temp.transform.localScale);

           Color c = temp.GetComponent<Renderer>().material.color;

           //c.a = SetAlpha(s_Hits[0]);
           //c.a = 1;
           c.r = color.r;
           c.g = color.g;
           c.b = color.b;
          //c = Color.red;
//update to deserialize
//change for drawingreport
//Add 2 float []s, another float [] named headings, and vector3 []. AxialDistances, PlaneCentersToArt, Headings, PlaneCenter
           Debug.Log("C: " + c);

           temp.GetComponent<Renderer>().material.SetColor("_Color", c);
           Debug.Log("TEMP: " + temp.GetComponent<Renderer>().material.color);

           Instantiate(temp, hitPose.position,hitPose.rotation);

           poses.Add(hitPose);
           Debug.Log("TEMP: " + temp);

           objects.Add(temp);

           idForStruct = planeId;

         }

    }
   public void setMaster(GameObject m){
     master = m;
   }
   public Vector3 DistanceFromPlane(Vector3 pCenter){
     //3 feet in a meter so 1/3 is a foot
     Vector3 handSet  = cam.transform.position;
     float newZ = pCenter.z - handSet.z;
     handSet.z = newZ;
     return handSet;

   }
   public float SetAlpha(ARRaycastHit hit){
      float meter = 1.0f;
      float newAlpha = meter - hit.distance;
      if(newAlpha < 0 ){
        newAlpha=0;
      }
      return newAlpha;

   }
   public GameObject getMaster(){
     return master;
    
   }
   public void newInstantiate(GameObject obj, Vector3 position, Quaternion quat){


   }
   public void ColorChange(){

    Color temp = new Color(Rslider.value, Gslider.value, Bslider.value, 1);
    //apply.GetComponent<Image>().color = temp;
    previewGraphic.color = temp;
    //color = temp;
    color = Color.Lerp(color, temp,Mathf.PingPong(Time.time,1));
    apply.GetComponent<Image>().color = color;
    Debug.Log(color);

    //color.r = Rslider.value;
    //color.g = Gslider.value;
    //color.b = Bslider.value;

     //m_ObjectToPlace = sphere;
   }
    // Update is called once per frame
    void Update()
    {
       bool enablePaint = true;

        //  if(Input.touchCount > 0){

        //    Touch touch = Input.GetTouch(0);

        //    if(touch.phase == TouchPhase.Began || touch.phase == TouchPhase.Moved){

        if(planes.Count < 1){
          enablePaint = false;
        }
          if(enablePaint && spray){
            paint();
          }
              /*if(m_RaycastManager.Raycast(screenCenter,s_Hits ,TrackableType.PlaneWithinPolygon)){
                sprayToggle.GetComponent<Image>().color = Color.green;
              }
              else{
                sprayToggle.GetComponent<Image>().color = Color.black;
              }*/
      //      }
      //    }
        if(m_RaycastManager.Raycast(cam.transform.position, all_hits, TrackableType.PlaneWithinPolygon)){
          int i = 0;
          foreach(ARRaycastHit id in all_hits){
            TrackableId tid = all_hits[all_hits.Count-1].trackableId;
            //Debug.Log("TID = " + id.trackableId);
            ARPlane p = m_PlaneManager.GetPlane(tid);
            bool floorFlag = GetFloor(p);
            bool EWFlag = GetEorW(p);
            bool NSFlag = GetNorS(p);
            if((tempSPlane == null || tempNPlane == null ) && (tempWPlane == null || tempEPlane == null) && tempfloorPlane == null){
              if(tempfloorPlane == null){
                Debug.Log("Disabled: floor needed");
              }
              if(tempSPlane == null || tempNPlane == null ){
                Debug.Log("Disabled: NS needed");
              }
              if(tempWPlane == null || tempEPlane == null){
                Debug.Log("Disabled: EW needed");
              }
              Debug.Log("Paint Disabled");
            }
            else{
              enablePaint = true;
            }
            //Debug.Log("PLANE p = " + p);
            if(p == null){
             // Debug.Log(tid + "is null");
            }
            else{
              if(!planes.Contains(p) ){
                count++;
                camPosition = Camera.main.transform.position;
                planes.Add(p);
                cameraRotations.Add(Camera.main.transform.rotation);
                headings.Add(GPS.heading);
                planeSize.Add(p.size);
                planeNormals.Add(p.normal);
                planeAlignment.Add(p.alignment);
                Debug.Log("PLANE CENTER FROM PAINTER: " + p.center);
                Debug.Log("PLANE FOUND FROM PAINTER " + p);
                camPositionsWhenPlaneFound.Add(Camera.main.transform.position);
                Debug.Log("found id: " + tid + "with center: " + p.center + "and boundary: " + p.boundary);
              }
            }
            i++;
          } 
      }
      /*
        //  if(Input.touchCount > 0){
        //    Touch touch = Input.GetTouch(0);
        //    if(touch.phase == TouchPhase.Began || touch.phase == TouchPhase.Moved){
          if(spray){
            paint();
          }
              //if(m_RaycastManager.Raycast(screenCenter,s_Hits ,TrackableType.PlaneWithinPolygon)){
                //sprayToggle.GetComponent<Image>().color = Color.green;
              //}
              //else{
                //sprayToggle.GetComponent<Image>().color = Color.black;
              //}
      //      }
      //    }
        if(m_RaycastManager.Raycast(cam.transform.position, all_hits, TrackableType.PlaneWithinPolygon)){
          int i = 0;
          foreach(ARRaycastHit id in all_hits){

            TrackableId tid = all_hits[all_hits.Count-1].trackableId;
            //Debug.Log("TID = " + id.trackableId);

            ARPlane p = m_PlaneManager.GetPlane(tid);
            //Debug.Log("PLANE p = " + p);
            if(p == null){
             // Debug.Log(tid + "is null");
            }
            else{
              if(!planes.Contains(p) ){
                count++;
                camPosition = Camera.main.transform.position;
                planes.Add(p);
                cameraRotations.Add(Camera.main.transform.rotation);
                headings.Add(GPS.heading);
                planeSize.Add(p.size);
                planeNormals.Add(p.normal);
                planeAlignment.Add(p.alignment);
                Debug.Log("PLANE CENTER FROM PAINTER: " + p.center);
                Debug.Log("PLANE FOUND FROM PAINTER " + p);
                camPositionsWhenPlaneFound.Add(Camera.main.transform.position);
                Debug.Log("found id: " + tid + "with center: " + p.center + "and boundary: " + p.boundary);
              }
            }
            i++;
          }



      }*/
    }
    public void drawingUpload(){
      Debug.Log("STARTING UPLOAD");

      lat = GPS.latitude;
      lon = GPS.longitude;
      geohashOfDevice = Network_Manager.Encode(lat, lon);
      drawing drawingReport = new drawing("Drawing ID Dummy", geohashOfDevice, "SessionIDHere", "TrackableId" , "~");
      drawingReport.sessionID = Network_Manager.sessionIDToSendToPainter;
    //  drawingReport.trackableIdOfPlane = idForStruct;
      drawingReport.trackableIdAsString = idForStruct.ToString();
      drawingReport.drawingID = Network_Manager.sha256(Network_Manager.originalTime + SystemInfo.deviceUniqueIdentifier + "drawing");
      foreach(Pose pose in poses){
        drawingReport.poseList.Add(pose);
        //DUMMY VALUES FOR NEW LISTS
        drawingReport.AxialDistances.Add(1.443F);
        drawingReport.headings.Add(2.019F);
        drawingReport.PlaneCentersToArt.Add(9.098F);
        Vector3 tempVector3 = new Vector3 (1.4F, 1.5F, 4.3F);

        drawingReport.PlaneCenters.Add(tempVector3);
        drawingReport.CameraRotationsY.Add(69.69F);

      }
            Debug.Log("HALFWAY");
      foreach(float r in radii){
        drawingReport.paintList.Add(r);

      }
      try{
      foreach(GameObject go in objects){
        //drawingReport.paintList.Add(go.transform.localScale);
        //drawingReport.paintColors.Add(go.GetComponent<Renderer>().material.color);
        float r = 0;
        float g = 0;
        float b = 0;
        float a = 0;
        if(go.GetComponent<Renderer>()==null){
          Debug.Log("RENDERER NULL");
        }
        else{
          Debug.Log("RENDERER: " + go.GetComponent<Renderer>()==null);
        }
         r = go.GetComponent<Renderer>().material.color.r;
                            Debug.Log("R GETCOMP: " + go.GetComponent<Renderer>().material.color.r);

          Debug.Log("R: " + r);
        
                 g = go.GetComponent<Renderer>().material.color.g;
         b = go.GetComponent<Renderer>().material.color.b;
         a = go.GetComponent<Renderer>().material.color.a;
                Vector4 tempVector4 = new Vector4 (r, g, b, a);
drawingReport.paintColors.Add(tempVector4);
        /*  Debug.Log("A: " + go.GetComponent<Renderer>().material.color.a);
                    Debug.Log("R: " + go.GetComponent<Renderer>().material.color.r);
          Debug.Log("B: " + go.GetComponent<Renderer>().material.color.b);
          Debug.Log("G: " + go.GetComponent<Renderer>().material.color.g);
          Debug.Log("lineear: " + go.GetComponent<Renderer>().material.color.linear);*/

        
        //foreach(char asdf in go.GetComponent<Renderer>().material.color.ToString()){
        //  Debug.Log(asdf);
        //}
        Debug.Log("GO: " + go.ToString());
      }
            Debug.Log("DONE LOADING LIST");

      Debug.Log("POSE LIST COUNT " + drawingReport.poseList.Count);
      Debug.Log("PAINT COLOR LIST COUNT " + drawingReport.paintColors.Count);
      }
      catch(Exception e){
        Debug.Log(e.ToString());
      }
      
try{
   /*JObject o = (JObject)JToken.FromObject(drawingReport);
      o.Property("Linear").Remove();
    foreach(char asd in o.ToString()){
      Debug.Log(asd);
    }*/
      string json = JsonConvert.SerializeObject(drawingReport/*o*/,  new JsonSerializerSettings()
      {
        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
      });
            Debug.Log("JSON: "  + json);

      /*JObject rss = JObject.Parse(json);
      JObject channel = (JObject)rss["color"];
      channel.Property("r").Remove();
      Debug.Log("CHANNEL COLOR" + channel.Property("color"));
      string json2 = JsonConvert.SerializeObject(channel/*o*,  new JsonSerializerSettings()
      {
        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
      });
            Debug.Log("JSON2: "  + json2);*/

      byte[] myData = System.Text.Encoding.UTF8.GetBytes(json/*channel*/.ToString());


      Debug.Log("HERE");


      GameObject.FindWithTag("Network Tag").GetComponent<Network_Manager>().SendData(myData);
            }
catch(Exception e){
  Debug.Log("CATCH: " + e);
}

    }

public double getNewRad(ARRaycastHit hit){
    double toRet = hit.distance*THETA;
    Debug.Log("New Rad is: " + toRet);
    return toRet;
   }



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

public bool GetFloor(ARPlane p){

     if(p.center.y < cam.transform.position.y){

       tempfloorPlane = p;

       return true;

     }

     return false;

   }

   public bool GetNorS(ARPlane p){

     if(GPS.heading <= 15 || GPS.heading > 345){
       tempNPlane = p;
       Debug.Log("NS FOUND");
       return true;
     }

     if(GPS.heading < 195 && GPS.heading > 165){
       tempSPlane = p;
       Debug.Log("NS FOUND");
       return true;
     }
     return false;
   }
      public bool GetEorW(ARPlane p){

     if(GPS.heading < 105 && GPS.heading > 75){
       tempWPlane = p;
       Debug.Log("EW FOUND");
       return true;
     }

     if(GPS.heading < 285 && GPS.heading > 255){
       tempEPlane = p;
       Debug.Log("EW FOUND");
       return true;
     }
     return false;
   }

    public static void changeColor(){
      //color = ColorPreview.getColor();
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
    /*public static void CreateNewSphere(Color c){
      //Debug.Log("Before: " + c.r + ", " + c.g + ", " + c.b);
      //c.r *= 255;
      //c.g *= 255;
      //c.b *= 255;
      float r = c.r;
      float g = c.g;
      float b = c.b;
      float a = 1.0f;
      //Debug.Log("Color should be: " + c.r + ", " + c.g + ", " + c.b);

      color = new Color(r,g,b,a);
      Debug.Log("color changed to :" + color.r + "," + color.g + "," + color.b + ","+ color.a);
      GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
      Vector3 vec = new Vector3(.9F,1,.9F);
      sphere.transform.localScale -= vec;
      Material m = sphere.GetComponent<Renderer>().sharedMaterial;
      m.SetColor("_Color", color);
      m_ObjectToPlace = sphere;
      //c.r = 255;
      //c.g = 127;

    }*/
 }
