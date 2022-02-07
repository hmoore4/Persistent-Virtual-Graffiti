using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NewButtonManager : MonoBehaviour
{    public Button button;
private static NewButtonManager _Instance;
    public static NewButtonManager Instance
    {
        get
        {
            if (!_Instance)
            {
                //new
                GameObject go = new GameObject();
                _Instance = go.AddComponent<NewButtonManager>();
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
 Debug.Log("HERE");
    _Instance = this;
    DontDestroyOnLoad( this.gameObject );
   
    }

    // Start is called before the first frame update
    void Start()
    {
                Debug.Log("NEW BUTTON MANAGER TEST HERE");

              button.onClick.AddListener(TaskOnClick);
                 //  button.onClick.AddListener(PainterCreateSphere);
//370

    }
    void TaskOnClick(){
        Debug.Log("FUNCTIONAL BUTTON");
        GameObject.FindWithTag("Painter").GetComponent<Painter>().drawingUpload();

     /* poses = Painter.getPoses();
      objects = Painter.getObjects();

      rect r = boundingBox(objects);
      Debug.Log("Creating BoundingBox");
      string ret = "ret r: " + r.lowright.x + "b: " + r.lowright.y + "t: " + r.upleft.y + "l: " + r.upleft.y;
      Debug.Log(ret);
      toSEND = System.Text.Encoding.UTF8.GetBytes(ret);
      //get heading so that we dont randomly place drawing somewhere
      //setDimensions(Painter.getObjects());
      Debug.Log("Creating Texture");
      //test = takeScreenShot(r);
      sendFlag = true;
      GameObject.FindWithTag("Network Tag").GetComponent<Network_Manager>().SendData(toSEND);*/
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
