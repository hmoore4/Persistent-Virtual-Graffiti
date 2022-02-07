
//If you see a plane, give last camera info
using System.Collections;
using System.Collections.Generic;
using UnityEngine.XR.ARFoundation;
using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Net;
using System.Net.Sockets;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Driver;

using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Text;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.UI;
using System.Threading.Tasks;
using System.Threading;


public class Network_Manager : MonoBehaviour
{
    //TODO: Add list of vector3/4 to struct and test that it can take new stuff
    //TODO: Figure out why camera.transform.position.x is null and correct the issue
    //TODO: Figure out how to store an image file on mongo (What they're looking at): Lead GridFS
//TODO: More responsible sending of camera positions - less frequent(server code)(Session id, append something on the fly). Or only points where they find a plane
    //If you see a plane, give last camera info
    //Only send camera position when a plane is found
    //TODO: Byte read and message size don't match - why
    	public static Camera camera;
    	static float GPO_lat, GPO_lon,GPO_heading;
        string temphash;
        public List<Vector3> planeBoundaryVector3;
        int count = 0;
        static int iFixedUpdate = 0;
        public static string originalTime;
        public static string sessionIDToSendToPainter;
        string massiveStringFromReports;
        int totalReceived = 0;
        TrackableId filler;
        int iterations = 1;
        HashSet<string> hashForDrawingsFromServer = new HashSet<string>();
        public string StrtoDeserialize;
        private static String response = String.Empty;
        public String setup;
          private static ManualResetEvent receiveDone = new ManualResetEvent(false);


     struct altReport
        {
            public string ID;
            public string geohash;
            //public Vector4 vector;

            public string time;

            public List<float> dingdong;
            public List<Vector3> planeCenter;
            public List<Vector3> camPositionWhenPlaneFound;

            public List<float> headings;
            public List<Quaternion> cameraRotations;
            public string sessionID;

            //public List<ARPlane> planes;
            //public List<TrackableId> trackableIds;
            public List<String> trackableIdString;
            public List<List<Vector2>> planeBoundary;
            public List<Vector2> planeSizes;
            public List<Vector3> planeNormals;
            public int planeBoundaryLength;
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
              //  planes = new List<ARPlane>();
                //trackableIds = new List<TrackableId>();
                trackableIdString = new List<String>();
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

    	//public static IMongoClient client = new MongoClient("mongodb+srv://testUser2:testPassword@cluster0.h9orn.mongodb.net/InputDB?retryWrites=true&w=majority");
        //public static IMongoDatabase database;
        //public static IMongoCollection<BsonDocument> collection;
    private static Socket _clientSocket = new Socket(AddressFamily.InterNetwork,SocketType.Stream,ProtocolType.Tcp);
    private byte[] _recieveBuffer = new byte[8142];
    // Establish the remote endpoint for the socket.
    public static IPAddress ipAddress = IPAddress.Parse("72.92.230.97");
    public static IPEndPoint remoteEP = new IPEndPoint(ipAddress, 8989);
	public static int hasSent = 0;
	private List<Vector3> vectors;

    void Start()
    {
      originalTime = DateTime.UtcNow.ToString();
        SetupServer();
		Debug.Log("Line 106 from Start in Network_Manager\n");

        //StartCoroutine(Upload());
    }

    public void SetupServer()
    {
        try
        {
            _clientSocket.Connect(new IPEndPoint(ipAddress,8989));
			//Debug.Log("Line 116 from SetupServer in Network_Manager\n");

        }
        catch(SocketException ex)
        {
            Debug.Log(ex.Message);
        }


        _clientSocket.BeginReceive(_recieveBuffer,0,_recieveBuffer.Length,SocketFlags.None,new AsyncCallback(ReceiveCallback),null);

    }
    public static bool ContainsInvalidCharacters(string name)
        {
            return name.IndexOfAny(new[]
            {
        '\u0000', '\u0002', '\u0003',
    }) != -1;
        }
    private void Update(){
        //MAYBE Move to FixedUpdate
        if (vectors == null)
            vectors = new List<Vector3>();
        if(Camera.main.transform.position == null){
            Debug.Log("ERROR");
        }

        //vectors.Add(Camera.main.transform.position);
		//Debug.Log("Line 137 from Update in Network_Manager\n");
    }
    private void FixedUpdate(){

        //newestReport.Add(Camera.main.transform.position);

		//we need access to getCount and getReportSize from Report_Manager
		//GameObject rreport = FindObjectOfType<Report_Manager>();

		//On a whim, i looked at the GPS system again and GPS.latitude.ToString() looked like class -> struct or obj -> function call
		//so you get this Report_Manager.instance.getCount() stuff.
        //Debug.Log("PAINTER.COUNT = " + Painter.planes.Count);
        //Debug.Log("I = " + iFixedUpdate);
	//Debug.Log("calling from Network_Manager : Painter.planes.Count : " +Painter.planes.Count);
	//	Debug.Log("calling from Network_Manager : Painter.count : " +Painter.count);
		//Debug.Log("calling from Network_Manager : iFixedUpdate : " + iFixedUpdate);

		if(Painter.planes.Count > iFixedUpdate/*Report_Manager.count >= (i * Report_Manager.REPORT_SIZE)*/){
            Upload();
            iFixedUpdate = Painter.planes.Count;
			hasSent = iFixedUpdate;
		}

        else{
        }
        //Debug.Log("STEP 5");

        //ReceiveCallback();
    }
    //TODO: Change SessionId, Change trackableIds,
    private  void Upload()
    {
        try{
            if(_clientSocket.Connected){
                Debug.Log("Still connected.");
            }
            else{
                Debug.Log("Starting to try to reconnect");
                SetupServer();
                Debug.Log("Trying to reconnect.");
            }
        }
        catch(SocketException){

        }

        GPO_lat = GPS.latitude;
		    GPO_lon = GPS.longitude;
		    GPO_heading=GPS.initial_heading;

        //byte[] myData = System.Text.Encoding.UTF8.GetBytes("Device ID : " + SystemInfo.deviceUniqueIdentifier + ", TIMESTAMP : " + DateTime.UtcNow.ToString() +"\nLAT : " + GPS.latitude.ToString() + "\n LON : " +GPS.longitude.ToString() + "\n HEADING : " +GPS.heading.ToString() );
        //database = client.GetDatabase("InputDB");
        //collection = database.GetCollection<BsonDocument>("PlaneLocation");
        temphash = Network_Manager.Encode(GPO_lat,GPO_lon);
        altReport newestReport = new altReport(temphash,"SessionID" ,SystemInfo.deviceUniqueIdentifier, DateTime.UtcNow.ToString(),0,"~");
        sessionIDToSendToPainter = sha256(originalTime + SystemInfo.deviceUniqueIdentifier);
        foreach(ARPlane p in Painter.getPlaneList()){
            List<Vector2> boundaryPoints = new List<Vector2>();
            //newestReport.planes.Add(p);
            //p.alignment.ToString();       //Get plane alignment
            Debug.Log("PLANE CENTER: " + p.center/*boundary(planeBoundaryVector3, Space.Self)*/);
            newestReport.planeCenter.Add(p.center);
            //newestReport.trackableIds.Add(p.trackableId);
            newestReport.trackableIdString.Add(p.trackableId.ToString());
            /*foreach(TrackableId t in newestReport.trackableIds){
              Debug.Log("TRACKABLE ID OF PLANE" + t);
            }*/
            Debug.Log("TRACKABLE ID OF PLANE" + p.trackableId);
            Debug.Log("COUNT OF PLANE CENTER  " + newestReport.planeCenter.Count);
            foreach(Vector2 pt in p.boundary){
                Vector2 tempVector2 = new Vector2 (pt.x, pt.y);
                boundaryPoints.Add(tempVector2);

            }
            newestReport.planeBoundary.Add(boundaryPoints);
            sessionIDToSendToPainter = sha256(originalTime + SystemInfo.deviceUniqueIdentifier);
            newestReport.sessionID = sessionIDToSendToPainter;



        }

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



       // else{}

        string json = JsonConvert.SerializeObject(newestReport);


        Debug.Log("GETTING SENT: " + json);
        byte[] myData = System.Text.Encoding.UTF8.GetBytes(json);
        Debug.Log("Length OF MY DATA:  " + myData.Length);

		SendData(myData);

      //  }

       // UnityWebRequest www = UnityWebRequest.Put("192.168.1.184:8989", myData);
        //yield return www.SendWebRequest();
        /*
        if(www.isNetworkError || www.isHttpError) {
            Debug.Log(www.error);
        }

        else
        */
      //  Debug.Log("Upload complete!");
    }
    private void ReceiveCallback(IAsyncResult AR)
   {


     //int recieved = _clientSocket.EndReceive(AR);

     //if(recieved <= 0)
      //   return;

     //Copy the recieved data into new buffer , to avoid null bytes
     //byte[] recData = new byte[recieved];
     //Buffer.BlockCopy(_recieveBuffer,0,recData,0,recieved);

     //Process data here the way you want , all your bytes will be stored in recData

     //Start receiving again
     //_clientSocket.BeginReceive(_recieveBuffer,0,_recieveBuffer.Length,SocketFlags.None,new AsyncCallback(ReceiveCallback),null);

       try{
       int recievedSize = 0;
       Debug.Log("Amount of times ran through this: " + iterations);

       //Check how much bytes are recieved and call EndRecieve to finalize handshake
       recievedSize = _clientSocket.EndReceive(AR);
       totalReceived += recievedSize;
       Debug.Log("RECIEVEDSIZE: " + recievedSize);
       Debug.Log("totalReceived: " + totalReceived);

       byte[] recData= new byte[recievedSize];
       string toDeserialize;
       string json;
       Buffer.BlockCopy(_recieveBuffer,0,recData,0,recievedSize);
       toDeserialize = System.Text.Encoding.UTF8.GetString(recData);
       massiveStringFromReports += toDeserialize;

       Debug.Log("Does toDeserialize have null: " + ContainsInvalidCharacters(toDeserialize));
       Painter.drawing testing = new Painter.drawing("drawingID", "geohash", "sessionID", "TRACKABLEID",  "endOfDocument");
       Painter.drawing finalTest = new Painter.drawing("drawingID", "geohash", "sessionID", "TrackableID",  "endOfDocument");

      if(recievedSize > 0 && !ContainsInvalidCharacters(toDeserialize)){
           Debug.Log("ENTER IF");
          // StrtoDeserialize += Encoding.ASCII.GetString(recData,0,recievedSize);
           toDeserialize = toDeserialize.Replace("$numberDouble", "");
           toDeserialize = toDeserialize.Replace("$numberLong", "");
           massiveStringFromReports = massiveStringFromReports.Replace("$numberInt", "");
           toDeserialize = toDeserialize.Replace("\\", "").Replace("\"", ""); //Gets rid of \"
          // Debug.Log("STRTODESERIALIZE " + StrtoDeserialize);
           massiveStringFromReports = massiveStringFromReports.Replace("$numberDouble", "");
           massiveStringFromReports = massiveStringFromReports.Replace("$numberLong", "");
           massiveStringFromReports = massiveStringFromReports.Replace("$numberInt", "");
           massiveStringFromReports = massiveStringFromReports.Replace("\\", "").Replace("\"", ""); //Gets rid of \"


       _clientSocket.BeginReceive(_recieveBuffer,0,_recieveBuffer.Length,SocketFlags.None,new AsyncCallback(ReceiveCallback),null);
       }

       else{
          //Newtonsoft.Json.Utilities.AotHelper.EnsureList<string>();
           Debug.Log("ENTER ELSE");
           setup = setupDrawingObject();
           Debug.Log("PAST SETUP DRAWING OBJECT");
           foreach(var x in hashForDrawingsFromServer){
               Debug.Log("X: " + x);
               finalTest = JsonConvert.DeserializeObject<Painter.drawing>(x,
               new JsonSerializerSettings
               {
                   NullValueHandling = NullValueHandling.Ignore
               });
              // drawingsFromServer.Add(finalTest);
               Debug.Log("GEOHASH: " + finalTest.geohash);
               Debug.Log("DRAWINGID: " + finalTest.drawingID);

               Debug.Log("trackableIdString: " + finalTest.trackableIdAsString);


              }
            }
          }
          catch(Exception e){
              Debug.Log("CATCH: " + e.ToString());
          }/*
                 {
                       ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                   });
                   Debug.Log("HERE");
                   //BsonDocument document2 = BsonDocument.Parse(finalTestString);
                         //  Debug.Log("FINALTESTSTRING: " + finalTestString);

                   Debug.Log("EEE");
           }
          // drawingsFromServer.Add(finalTest);
           //Debug.Log("AMOUNT OF DRAWINGS IN LIST: " + drawingsFromServer.Count);
            if (StrtoDeserialize.Count() > 1) {
                   response = StrtoDeserialize;
                   Debug.Log("RESPONSE: " + response);
               }
               // Signal that all bytes have been received.
               receiveDone.Set();
       }
           testing.drawingID = massiveStringFromReports;
           testing.geohash = StrtoDeserialize;
           testing.sessionID = toDeserialize;
           testing.endOfDocument = setup;
           json = JsonConvert.SerializeObject(testing/*report*///,  new JsonSerializerSettings()
           /*{
               ReferenceLoopHandling = ReferenceLoopHandling.Ignore
           });
           Debug.Log("HERE");
           //BsonDocument document = BsonDocument.Parse(json);
           Debug.Log("THERE");
       }

       Debug.Log("AFTER FUNCTION");
       iterations++;*/
       //Check how much bytes are recieved and call EndRecieve to finalize handshake
    }

//}


   //Maybe have it return a string?
   public string /*BsonDocument*/ setupDrawingObject(){
       int i = 1;
       string stringToDeserialize="";
       massiveStringFromReports = massiveStringFromReports.Replace("$numberDouble", "");
        massiveStringFromReports = massiveStringFromReports.Replace("$numberLong", "");
        massiveStringFromReports = massiveStringFromReports.Replace("\\", "").Replace("\"", ""); //Gets rid of \"
        massiveStringFromReports = massiveStringFromReports.Replace("{  : ", "");
        massiveStringFromReports = massiveStringFromReports.Replace(" },", ",");
        massiveStringFromReports = massiveStringFromReports.Replace(", rotation", "} , { rotation");
        massiveStringFromReports = massiveStringFromReports.Replace(", forward", "} , { forward");
        massiveStringFromReports = massiveStringFromReports.Replace(", right", "} , { right");
        massiveStringFromReports = massiveStringFromReports.Replace(", up", "} , { up");
        massiveStringFromReports = massiveStringFromReports.Replace("{ $oid : ", "");

        massiveStringFromReports = massiveStringFromReports.Replace(", { position", " , { position");
        //Above line may work, I haven't tested. That's first thing to do.
        massiveStringFromReports = massiveStringFromReports.Replace(" } } } ]", "} } ]");
        //This above line also hasn't been tested. I think it will resolve the first issue state on 343.
        massiveStringFromReports = massiveStringFromReports.Replace(", normalized", "} , normalized");
        //Code adding way too many } in front of normalized. This is an attempt to get rid of the issue. Not a good solution, needs to be revised
        massiveStringFromReports = massiveStringFromReports.Replace("}} , normalized", "} , normalized");
        massiveStringFromReports = massiveStringFromReports.Replace("}} } , normalized", "} , normalized");
        massiveStringFromReports = massiveStringFromReports.Replace("}} } } , normalized", "} , normalized");
        massiveStringFromReports = massiveStringFromReports.Replace("}} } } } , normalized", "} , normalized");
        massiveStringFromReports = massiveStringFromReports.Replace("}} } } } } , normalized", "} , normalized");
        massiveStringFromReports = massiveStringFromReports.Replace("}} } } } } } , normalized", "} , normalized");
        massiveStringFromReports = massiveStringFromReports.Replace("}} } } } } } } , normalized", "} , normalized");
        massiveStringFromReports = massiveStringFromReports.Replace("}} } } } } } } } , normalized", "} , normalized");
        massiveStringFromReports = massiveStringFromReports.Replace("}} } } } } } } } } , normalized", "} , normalized");
        massiveStringFromReports = massiveStringFromReports.Replace("}} } } } } } } } } } , normalized", "} , normalized");
        massiveStringFromReports = massiveStringFromReports.Replace("}} } } } } } } } } } } , normalized", "} , normalized");
        massiveStringFromReports = massiveStringFromReports.Replace("} } , normalized", "} , normalized");
        massiveStringFromReports = massiveStringFromReports.Replace("} } } , normalized", "} , normalized");
        massiveStringFromReports = massiveStringFromReports.Replace("} } } } , normalized", "} , normalized");
        massiveStringFromReports = massiveStringFromReports.Replace("} } } } } , normalized", "} , normalized");
        massiveStringFromReports = massiveStringFromReports.Replace("} } } } } } , normalized", "} , normalized");
        massiveStringFromReports = massiveStringFromReports.Replace("} } } } } } } , normalized", "} , normalized");
        massiveStringFromReports = massiveStringFromReports.Replace("} } } } } } } } , normalized", "} , normalized");
        massiveStringFromReports = massiveStringFromReports.Replace("} } } } } } } } } , normalized", "} , normalized");
        massiveStringFromReports = massiveStringFromReports.Replace("} } } } } } } } } } , normalized", "} , normalized");
        massiveStringFromReports = massiveStringFromReports.Replace("} } } } } } } } } } } , normalized", "} , normalized");
        massiveStringFromReports = massiveStringFromReports.Replace("} } } } } } } } } } } } , normalized", "} , normalized");

        massiveStringFromReports = massiveStringFromReports.Replace(", geohash : ", "', 'geohash' : '");
        massiveStringFromReports = massiveStringFromReports.Replace("drawingID : ", "'drawingID' : '");
        massiveStringFromReports = massiveStringFromReports.Replace(", sessionID : ", "', 'sessionID' : '");
        //NEW
        massiveStringFromReports = massiveStringFromReports.Replace(", trackableIdAsString :", "', 'trackableIdAsString' : '");        massiveStringFromReports = massiveStringFromReports.Replace("', 'trackableIdAsString' : ' " , "', 'trackableIdAsString' : '");

        massiveStringFromReports = massiveStringFromReports.Replace(", poseList :", "', 'poseList' :");

        massiveStringFromReports = massiveStringFromReports.Replace("endOfDocument :", "'endOfDocument' :");
        massiveStringFromReports = massiveStringFromReports.Replace("_id : ", "");

        massiveStringFromReports = massiveStringFromReports.Replace("paintColors :", "'paintColors' :");
        massiveStringFromReports = massiveStringFromReports.Replace("paintList :", "'paintList' :");

        //try commenting these out
       // massiveStringFromReports = massiveStringFromReports.Replace("maxColorComponent : 0.0 } } ]", "maxColorComponent : 0.0 } ]");
        //massiveStringFromReports = massiveStringFromReports.Replace("maxColorComponent : 1 } } ]", "maxColorComponent : 1 } ]");
        //massiveStringFromReports = massiveStringFromReports.Replace("maxColorComponent : 1.0 } } ]", "maxColorComponent : 1 } ]");

        //massiveStringFromReports = massiveStringFromReports.Replace("z : 0.1 } } ], 'paintColors'", "z : 0.1 } ], 'paintColors'");
       // massiveStringFromReports = massiveStringFromReports.Replace("z : 0.02999999999999999889 } } ], 'paintColors'", "z : 0.02999999999999999889 } ], 'paintColors'");
        massiveStringFromReports = massiveStringFromReports.Replace("} } ], 'paintColors'", "} ], 'paintColors'");
        massiveStringFromReports = massiveStringFromReports.Replace("} } ], 'endOfDocument'", "} ], 'endOfDocument'");
        massiveStringFromReports = massiveStringFromReports.Replace("} } ], 'colorList'", "} ], 'colorList'");

       int IndexOfFirstSpace = massiveStringFromReports.IndexOf(" ")+1;
       massiveStringFromReports = massiveStringFromReports.Substring(IndexOfFirstSpace);

      // massiveStringFromReports = massiveStringFromReports.Replace("\\", "").Replace("\"", ""); //Gets rid of \"
       while(massiveStringFromReports.Contains("drawingID")){
                   bool isDigit = char.IsDigit(massiveStringFromReports[0]);

           if(isDigit == true){
               Debug.Log("IS DIGIT IS TRUE");
               StringBuilder sb = new StringBuilder(massiveStringFromReports);
               sb.Remove(0,25);
               massiveStringFromReports = sb.ToString();
               Debug.Log("MAsSIVE AFTER SB:  " + massiveStringFromReports);

               //massiveStringFromReports = "{" + massiveStringFromReports;
           }
           else{
               Debug.Log("IS DIGIT IS FALSE");
           }
               stringToDeserialize = GetUntilOrEmpty(massiveStringFromReports);
              // stringToDeserialize.Replace("{ 'drawingID'","{'drawingID'");
               int firstTilda = massiveStringFromReports.IndexOf('~');
               massiveStringFromReports = massiveStringFromReports.Substring(firstTilda + 6);
               Debug.Log("MASSIVESTRING AFTER CHOPPING START: " + massiveStringFromReports);
               stringToDeserialize += "'~'}";
               stringToDeserialize = "{" + stringToDeserialize;

               Debug.Log("PRE BIG TEST");
               if(!hashForDrawingsFromServer.Contains(stringToDeserialize)){
                   hashForDrawingsFromServer.Add(stringToDeserialize);
               }

               //drawingss = GameObject.FindWithTag("Painter").GetComponent<Painter>().parseServerReport(drawing);
               Painter.drawing drawingss = new Painter.drawing("drawingID", "geohash", "sessionID", "TrackableID",  "endOfDocument");
               Debug.Log("HOW");

               drawingss.geohash = massiveStringFromReports;

               Debug.Log("FAR");
               drawingss.drawingID = stringToDeserialize;
               Debug.Log("DOES");

               string drawingString = JsonConvert.SerializeObject(drawingss,  new JsonSerializerSettings()
               {
                   ReferenceLoopHandling = ReferenceLoopHandling.Ignore
               });
               Debug.Log("IT GET");
               Debug.Log("drawing string: " + drawingString);
               //BsonDocument document = BsonDocument.Parse(drawingString);
               //Debug.Log("SIZE OF INSERTED DOC " + document.ToBson().Length);

               Debug.Log("AFTER  PARSING");
               Debug.Log("THIS IS WHAT IS RETURNED:  " + stringToDeserialize);
   }
           foreach(var x in hashForDrawingsFromServer){
               Debug.Log("FROM HASH: " + x);
               Debug.Log("I: " + i + "HASH HAS: " + hashForDrawingsFromServer.Count + " ITEMS");
               i++;
           }
       return stringToDeserialize;
   }

   public static string GetUntilOrEmpty(string text, string stopAt = "~")
   {
       if (!String.IsNullOrWhiteSpace(text))
       {
           int charLocation = text.IndexOf(stopAt, StringComparison.Ordinal);

           if (charLocation > 0)
           {
               return text.Substring(0, charLocation);
           }
       }
       return String.Empty;
   }

    public void SendData(byte[] data)
    {
        SocketAsyncEventArgs socketAsyncData = new SocketAsyncEventArgs();
        socketAsyncData.SetBuffer(data,0,data.Length);
        _clientSocket.SendAsync(socketAsyncData);
    }
    public static string sha256(string str)
    {
        System.Security.Cryptography.SHA256Managed crypt = new System.Security.Cryptography.SHA256Managed();
        System.Text.StringBuilder hash = new System.Text.StringBuilder();
        byte[] crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(str), 0, Encoding.UTF8.GetByteCount(str));
        foreach (byte bit in crypto)
        {
            hash.Append(bit.ToString("x2"));
        }
        return hash.ToString().ToLower();
    }

    public static string Encode(double latitude, double longitude, int numberOfChars = 9)
    {   const string Base32Codes = "0123456789bcdefghjkmnpqrstuvwxyz";

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


        public static string EncodeDouble(double number)
    {
        char[] digitToLetter = new char[] {'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k' }; //Digits 0-9 are a-j. the . = k
        string DoubleToEncode = number.ToString(); //"12.3456789
        char[] DoubleToCharArray = DoubleToEncode.ToCharArray();
        char[] DoublesTurnedToChars = new char[DoubleToCharArray.Length];


        for(int i = 0; i < DoubleToCharArray.Length; i++){
            if(DoubleToCharArray[i] == '0'){
                DoublesTurnedToChars[i] = 'a';
            }
            else if(DoubleToCharArray[i] == '1'){
                DoublesTurnedToChars[i] = 'b';
            }
            else if(DoubleToCharArray[i] == '2'){
                DoublesTurnedToChars[i] = 'c';
            }
            else if(DoubleToCharArray[i] == '3'){
                DoublesTurnedToChars[i] = 'd';
            }
            else if(DoubleToCharArray[i] == '4'){
                DoublesTurnedToChars[i] = 'e';
            }
            else if(DoubleToCharArray[i] == '5'){
                DoublesTurnedToChars[i] = 'f';
            }
            else if(DoubleToCharArray[i] == '6'){
                DoublesTurnedToChars[i] = 'g';
            }
            else if(DoubleToCharArray[i] == '7'){
                DoublesTurnedToChars[i] = 'h';
            }
            else if(DoubleToCharArray[i] == '8'){
                DoublesTurnedToChars[i] = 'i';
            }
            else if(DoubleToCharArray[i] == '9'){
                DoublesTurnedToChars[i] = 'j';
            }
            else if(DoubleToCharArray[i] == '.'){
                DoublesTurnedToChars[i] = 'k';
            }
        }
        string encodedDouble = DoublesTurnedToChars.ToString();

    return encodedDouble;

    }





  }
