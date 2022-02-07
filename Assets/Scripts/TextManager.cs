using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextManager : MonoBehaviour
{
    public Text lat, lon, alt,head,head_con, query;
    public string querytest;
     /*    private static TextManager _Instance;
    public static TextManager Instance
    {
        get
        {
            if (!_Instance)
            {
                //new
                GameObject go = new GameObject();
                _Instance = go.AddComponent<TextManager>();
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
   
    }*/

    // Trying out calling this 3 times per frame
    void FixedUpdate()
    {
        lat.text = "LAT : " + GPS.latitude.ToString();
        lon.text = "LON : " +GPS.longitude.ToString();
        alt.text = "ALT : " +GPS.altitude.ToString();
        head.text = "HEADING : " +GPS.heading.ToString();
        head_con.text = "hasSent flag : " +Network_Manager.hasSent.ToString() ;
              //  querytest = GameObject.FindWithTag("Network Tag").GetComponent<Network_Manager>().queryTest();
              //  query.text = querytest;


    }
}
