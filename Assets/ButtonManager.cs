using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Drawing;
using System.IO;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System;

public class ButtonManager : MonoBehaviour
{
    public struct rect{
      public Vector2 upleft;
      public Vector2 lowright;
    }
    public Button button;
    public static Camera cam;
    public static byte[] toSEND;
    public static bool sendFlag;
    public static byte[] test;
    public List<Pose> poses;
    public static List<GameObject> objects;
    public float objectwidth, objectheight;
    public List<Vector2> paintedPoints = new List<Vector2>();

    private static ButtonManager _Instance;
    public static ButtonManager Instance
    {
        get
        {
            if (!_Instance)
            {
                //new
                GameObject go = new GameObject();
                _Instance = go.AddComponent<ButtonManager>();
               // _Instance = new GameObject().AddComponent<SingletonSimple>();
                // name it for easy recognition
                //_Instance.name = _Instance.GetType().ToString();
                // mark root as DontDestroyOnLoad();
                //DontDestroyOnLoad(_Instance.gameObject);
            }
            return _Instance;
        }
    }
      void Awake()
    {
        if (_Instance != null && _Instance != this)
    {
        Debug.Log("GG");
       Destroy(this.gameObject);
       return;//Avoid doing anything else
    }
 Debug.Log("BUTTON MANAGER HERE");
    _Instance = this;
    DontDestroyOnLoad( this.gameObject );
    //numNetworkManagers = FindObjectsOfType<Network_Manager>().Length;
    //    Debug.Log("numNetworks: " + numNetworkManagers);
    }
    // Start is called before the first frame update
    void Start(){
      button.onClick.AddListener(TaskOnClick);
      cam = GetComponent<Camera>();
    }

    void TaskOnClick(){
      Debug.Log("Clicked");
      GameObject.FindWithTag("Painter").GetComponent<Painter>().drawingUpload();

      poses = Painter.getPoses();
      objects = Painter.getObjects();

      //rect r = boundingBox(objects);
      //Debug.Log("Creating BoundingBox");
    //  string ret = "ret r: " + r.lowright.x + "b: " + r.lowright.y + "t: " + r.upleft.y + "l: " + r.upleft.y;
    //  Debug.Log(ret);
    //  toSEND = System.Text.Encoding.UTF8.GetBytes(ret);
      //get heading so that we dont randomly place drawing somewhere
      //setDimensions(Painter.getObjects());
      //Debug.Log("Creating Texture");
      //test = takeScreenShot(r);
      //sendFlag = true;
      //GameObject.FindWithTag("Network Tag").GetComponent<Network_Manager>().SendData(toSEND);

    }
    public static byte[] takeScreenShot(rect r){
      //double left = cam.ScreenToWorldPoint(r.upleft.x);
      //double up = r.upleft.y / 640.0 - 0.5 * 10.0;
      //double right = r.lowright.x / 640.0 - 0.5 * 10.0;
      //double low = r.lowright.y / 640.0 - 0.5 * 10.0;
      int width = (int)(r.upleft.x - r.lowright.x);
      int height =(int)(r.upleft.y - r.lowright.y);
      Debug.Log("Pixel width: " + width + "height: " + height);
      Texture2D tex = new Texture2D(width,height, TextureFormat.RGBAFloat, false);
      prepareTex(tex);
      foreach(GameObject o in objects){
        //for loop circle and
        gettingCircles(o, tex);
        tex.SetPixel((int)o.transform.position.x, (int)o.transform.position.y,Color.white);
      }
      tex.Apply();
      SaveTexture(tex);
      byte[] xByte = tex.EncodeToPNG();
      return xByte;
    }
    public static bool checkFlag(){
      return sendFlag;
    }
    public static byte[] getImage(){
      return toSEND;
    }
    public rect boundingBox(List<GameObject> obj){
      rect r;
      //Vector2 upleft;
      //Vector2 lowright;
      r.upleft.x = (left(obj).x);
      r.upleft.y = (top(obj).y);
      r.lowright.x =(right(obj).x);
      r.lowright.y = (bot(obj).y);
      return r;
    }
    public Vector3 left(List<GameObject> obj){
      Vector3 kp;
      kp.x = obj[0].transform.position.x;
      kp.y = obj[0].transform.position.y;
      kp.z = obj[0].transform.position.z;
      foreach(GameObject o in obj){
        if(o.transform.position.x < kp.x){
          kp.x =o.transform.position.x;
          kp.y = o.transform.position.y;
          kp.z = o.transform.position.z;
        }
      }
      Debug.Log("Left");
      Debug.Log(cam.WorldToScreenPoint(kp));
      return cam.WorldToScreenPoint(kp);
      //return transform.TransformPoint(kp);
    }
    public Vector3 top(List<GameObject> obj){
      Vector3 kp;
      kp.x = obj[0].transform.position.x;
      kp.y = obj[0].transform.position.y;
      kp.z = obj[0].transform.position.z;
      foreach(GameObject o in obj){
        if(o.transform.position.y < kp.y){
          kp.x = o.transform.position.x;
          kp.y = o.transform.position.y;
          kp.z = o.transform.position.z;
        }
      }
      return cam.WorldToScreenPoint(kp);
      //return transform.TransformPoint(kp);
    }
    public Vector3 right(List<GameObject> obj){
      Vector3 kp;
      kp.x = obj[0].transform.position.x;
      kp.y = obj[0].transform.position.y;
      kp.z = obj[0].transform.position.z;
      foreach(GameObject o in obj){
        if(o.transform.position.x > kp.x){
          kp.x = o.transform.position.x;
          kp.y = o.transform.position.y;
          kp.z = o.transform.position.z;
        }
      }
      return cam.WorldToScreenPoint(kp);
      //return transform.TransformPoint(kp);;
    }
    public Vector3 bot(List<GameObject> obj){
      Vector3 kp;
      kp.x = obj[0].transform.position.x;
      kp.y = obj[0].transform.position.y;
      kp.z = obj[0].transform.position.z;
      foreach(GameObject o in obj){
        if(o.transform.position.y > kp.y){
          kp.x = o.transform.position.x;
          kp.y = o.transform.position.y;
          kp.z = o.transform.position.z;
        }
      }
      return cam.WorldToScreenPoint(kp);
      //return transform.TransformPoint(kp);
    }
    public void setDimensions(Vector3 size){
      objectwidth = size.x;
      objectheight = size.y;
    }
    public static IEnumerator SaveTexture(Texture2D texture)
     {
         string fileName = "Photo";
         string screenshotFilename;
         string path;

         string date = System.DateTime.Now.ToString("ddMMyyHHmmss");
         screenshotFilename = fileName + "_" + date + ".png";

         if (Application.platform == RuntimePlatform.IPhonePlayer)
         {
             string androidPath = "/private/var/mobile/Media/DCIM/" + screenshotFilename;

             path = Application.persistentDataPath + androidPath;
             string pathonly = Path.GetDirectoryName(path);

             if (!Directory.Exists(pathonly))
             {
                 Directory.CreateDirectory(pathonly);
             }

         }
         else if (Application.platform == RuntimePlatform.IPhonePlayer)
         {
             Debug.Log("made it ios path");
             string iosPath = Application.persistentDataPath + "/" + screenshotFilename;
             path = iosPath;
         }
         else
         {
             string otherPath = screenshotFilename;
             path = otherPath;
         }
         byte[] bytes = texture.EncodeToJPG();
         File.WriteAllBytes(path, bytes);
         Debug.Log("Photo Saved");
         yield return new WaitForEndOfFrame();
     }
     public static int getRad(GameObject obj){
       int rad = (int)obj.GetComponent<SphereCollider>().radius;
       return rad;
     }
     public static void gettingCircles(GameObject o, Texture2D tex){
       int h = (int)o.transform.position.x;
       int k = (int)o.transform.position.y;
       int rad = getRad(o);
       int totalrad = rad*rad;
       int xpart, ypart;
       List<Vector2> border = new List<Vector2>();
       for(int x = 0; x <= (2*rad); x++){
         for(int y = 0; y <= (2*rad); y++){
         xpart = (x+h)*(x+h);
         ypart = (y+k)*(y+k);
         if((xpart+ypart) == totalrad){
           double newx = x / 640.0 - 0.5 * 10.0;
           double newy = y / 640 - 0.5 * 10;
           x = (int)newx;
           y = (int)newy;
           tex.SetPixel(x,y, Color.grey);
         }
         }
       }
     }
     public static void prepareTex(Texture2D tex){
       for (int y = 0; y < tex.height; y++)
        {
            for (int x = 0; x < tex.width; x++)
            {
                tex.SetPixel(x, y, Color.clear);
            }
        }
     }
}
