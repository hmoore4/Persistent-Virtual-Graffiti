using System.Collections;
using System.Collections.Generic;
using UnityEngine.XR.ARFoundation;
using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Web;
using System.Net;
using System.Net.Sockets;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Core;

using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Text;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.UI;
using System.Threading;

public class Network_Manager : MonoBehaviour
{           
    //TODO: Figure out how to store an image file on mongo (What they're looking at): Lead GridFS
public static Camera camera;
    HashSet<string> hashForDrawingsFromServer = new HashSet<string>(); 
public string StrtoDeserialize;

  TrackableId filler;
  int iterations = 1;
      private static String response = String.Empty; 
      public String setup;
 private static Boolean created = false;

        public static string originalTime;
    	static float GPO_lat, GPO_lon,GPO_heading;
        string temphash;
        string massiveStringFromReports;
        //public static string sessionIDToSendToPainter;
        public List<Vector3> planeBoundaryVector3;
        public List<Painter.drawing> drawingsFromServer;
        int count = 0;
        int totalReceived = 0;
        static int iFixedUpdate = 0;

    private static ManualResetEvent receiveDone = new ManualResetEvent(false);  


     struct altReport
        {
            public string ID;
            public string sessionID;
            public string geohash;
            //public Vector4 vector;
          
            public string time;

            public List<float> dingdong;
            public List<Vector3> planeCenter;
            public List<Vector3> camPositionWhenPlaneFound;

            public List<float> headings;
            public List<Quaternion> cameraRotations;

            public List<ARPlane> planes;
            public List<TrackableId> trackableIds;
            public List<String> trackableIdString;

            public List<List<Vector2>> planeBoundary; 
            public List<Vector2> planeSizes;
            public List<Vector3> planeNormals;

            public List<PlaneAlignment> planeAlignments;
            public string endOfDocument;


            public altReport(string geohash, string sessionID, string ID, string time, string endOfDocument/*, Vector4 vector*/)
            {
                //var theCam = GetComponent<Camera>();

                dingdong = new List<float>();
                planes = new List<ARPlane>();
                trackableIds = new List<TrackableId>();
                trackableIdString = new List<String>();
                planeCenter = new List<Vector3>();
                camPositionWhenPlaneFound = new List<Vector3>();
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
            }
        }

    public static IMongoClient client = new MongoClient("mongodb+srv://testUser2:testPassword@cluster0.h9orn.mongodb.net/InputDB?retryWrites=true&w=majority");

    public static IMongoDatabase database = client.GetDatabase("InputDB");
    public static IMongoCollection<BsonDocument> collection = database.GetCollection<BsonDocument>("PlaneLocation");
    public static IMongoCollection<BsonDocument> TestCasesCollection = database.GetCollection<BsonDocument>("TestCases");
    private Socket _clientSocket = new Socket(AddressFamily.InterNetwork,SocketType.Stream,ProtocolType.Tcp);
    private byte[] _recieveBuffer = new byte[131072];
    // Establish the remote endpoint for the socket.
    public static IPAddress ipAddress = IPAddress.Parse("72.92.230.97");
    public static IPEndPoint remoteEP = new IPEndPoint(ipAddress, 8989);
	public static int hasSent = 0;
	private List<Vector3> vectors;
    public static int numConnections = 0;
    
    void Awake()
    {
        if (!created)
        {
            DontDestroyOnLoad(this.gameObject);
            created = true;
        }
        
        else
        {
            Destroy(this.gameObject);
        }
        /*GameObject[] objs = GameObject.FindGameObjectsWithTag("Network Tag");

        if (objs.Length > 1)
        {
            Destroy(this.gameObject);
            Debug.Log("THE GAME OBJECT IS: " + this.gameObject);
        }

        DontDestroyOnLoad(this.gameObject);*/
    }
    void Start()
    {   
        SetupServer();
        originalTime = DateTime.UtcNow.ToString();
        //var originalTime = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");

    }
    

    public void SetupServer()
    {
        numConnections++;
        Debug.Log("NUMCONNECTIONS: " + numConnections);
        try
        {
            _clientSocket.Connect(new IPEndPoint(ipAddress,8989));
        }
        catch(SocketException ex)
        {
            Debug.Log(ex.Message);
        }
        _clientSocket.BeginReceive(_recieveBuffer,0,_recieveBuffer.Length,SocketFlags.None,new AsyncCallback(ReceiveCallback),null);
    }
    private void Update(){       
    }
    private void FixedUpdate(){        
	
		if(Painter.planes.Count > iFixedUpdate/*Report_Manager.count >= (i * Report_Manager.REPORT_SIZE)*/){
            Upload();
            iFixedUpdate = Painter.planes.Count;          
			hasSent = iFixedUpdate; 
		}
        else{
        }
    }


    private void Upload()
    {
        collection = database.GetCollection<BsonDocument>("TestCases");

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
        AlternativeReport.altReport newestReport = AlternativeReport.makePlaneReport();
        string json = JsonConvert.SerializeObject(newestReport/*report*/,  new JsonSerializerSettings()
        { 
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        });
        //BsonDocument document = BsonDocument.Parse(json);
        byte[] myData = System.Text.Encoding.UTF8.GetBytes(json);
        Debug.Log("Length OF MY DATA:  " + myData.Length);

		SendData(myData);
    }

        public static bool ContainsInvalidCharacters(string name)
        {
            return name.IndexOfAny(new[]
            {
        '\u0000', '\u0002', '\u0003',
    }) != -1;
        }
    
     private void ReceiveCallback(IAsyncResult AR)
    {
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
        Painter.drawing testing = new Painter.drawing("drawingID", "geohash", "sessionID", /*filler*/"D",  "endOfDocument");
        Painter.drawing finalTest = new Painter.drawing("drawingID", "geohash", "sessionID", /*filler*/"E",  "endOfDocument");

        if(recievedSize > 0 && !ContainsInvalidCharacters(toDeserialize)){
            Debug.Log("ENTER IF");
           // StrtoDeserialize += Encoding.ASCII.GetString(recData,0,recievedSize); 
            toDeserialize = toDeserialize.Replace("$numberDouble", "");
            toDeserialize = toDeserialize.Replace("$numberLong", "");
            toDeserialize = toDeserialize.Replace("\\", "").Replace("\"", ""); //Gets rid of \"
           // Debug.Log("STRTODESERIALIZE " + StrtoDeserialize);
            massiveStringFromReports = massiveStringFromReports.Replace("$numberDouble", "");
            massiveStringFromReports = massiveStringFromReports.Replace("$numberLong", "");
            massiveStringFromReports = massiveStringFromReports.Replace("\\", "").Replace("\"", ""); //Gets rid of \"
  
      
        _clientSocket.BeginReceive(_recieveBuffer,0,_recieveBuffer.Length,SocketFlags.None,new AsyncCallback(ReceiveCallback),null);
        }
        
        else{
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
                Debug.Log("TRACKABLE ID S STRING: " + finalTest.trackableIdAsString);
                //Debug.Log("TRACKABLEID: " + finalTest.trackableIdOfPlane.subId1);
                Debug.Log("COLORLIST" + finalTest.paintColors);
                        string finalTestString = JsonConvert.SerializeObject(finalTest/*report*/,  new JsonSerializerSettings()
                    { 
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                    });
                    Debug.Log("HERE");
                    BsonDocument document2 = BsonDocument.Parse(finalTestString);
                            Debug.Log("FINALTESTSTRING: " + finalTestString);

                    collection.InsertOne(document2);
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
            json = JsonConvert.SerializeObject(testing/*report*/,  new JsonSerializerSettings()
            { 
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
            Debug.Log("HERE");
            BsonDocument document = BsonDocument.Parse(json);
            Debug.Log("THERE");
            TestCasesCollection.InsertOne(document);
        }
        catch(Exception e){
            Debug.Log("CATCH: " + e.ToString());
        }   
        Debug.Log("AFTER FUNCTION");
        iterations++;
}
    

    public string /*BsonDocument*/ setupDrawingObject(){
        int i = 1;
        string stringToDeserialize="";
        TestCasesCollection =  database.GetCollection<BsonDocument>("TestCases");
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
           //     massiveStringFromReports = massiveStringFromReports.Replace("}, normalized", "}, normalized");

        massiveStringFromReports = massiveStringFromReports.Replace(", geohash : ", "', 'geohash' : '");//.Replace("\\", "");
        massiveStringFromReports = massiveStringFromReports.Replace("drawingID : ", "'drawingID' : '");
        massiveStringFromReports = massiveStringFromReports.Replace(", sessionID : ", "', 'sessionID' : '");
        //NEW        
        massiveStringFromReports = massiveStringFromReports.Replace(", trackableIdAsString :", "', 'trackableIdAsString' : '");
        //MAYBE
        massiveStringFromReports = massiveStringFromReports.Replace("', 'trackableIdAsString' : ' " , "', 'trackableIdAsString' : '");

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

        while(massiveStringFromReports.Contains("drawingID")){
                    bool isDigit = char.IsDigit(massiveStringFromReports[0]);

            if(isDigit == true){
                Debug.Log("IS DIGIT IS TRUE");
                StringBuilder sb = new StringBuilder(massiveStringFromReports);
                sb.Remove(0,25);
                massiveStringFromReports = sb.ToString();
                Debug.Log("MAsSIVE AFTER SB:  " + massiveStringFromReports);
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
                Painter.drawing drawingss = new Painter.drawing("drawingID", "geohash", "sessionID", /*filler*/ "L",  "endOfDocument");
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

                BsonDocument document = BsonDocument.Parse(drawingString);
                Debug.Log("SIZE OF INSERTED DOC " + document.ToBson().Length);
                TestCasesCollection.InsertOne(document);
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
}