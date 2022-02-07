using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GPS : MonoBehaviour
{
  public static GPS Instance{set; get;}
  public float latitude;
  public float longitude;
  public float altitude;
    // Start is called before the first frame update
    void Start()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
        StartCoroutine(StartLocationService());

    }
    private IEnumerator StartLocationService()
{
  if(!Input.location.isEnabledByUser){
    Debug.Log("User has not enabled GPS");
    yield break;
  }
  Input.location.Start();
  int maxWait = 20;
  while(Input.location.status == LocationServiceStatus.Initializing && maxWait > 0){
    yield return new WaitForSeconds(1);
    maxWait--;
  }
  if(maxWait <= 0){
    Debug.Log("Timed Out");
    yield break;
  }
  if(Input.location.status== LocationServiceStatus.Failed){
    Debug.Log("Unable to deermin device location");
    yield break;
  }
  latitude = Input.location.lastData.latitude;
  longitude = Input.location.lastData.longitude;
  altitude = Input.location.lastData.altitude;
  yield break;

} // End Start
  // Update is called once per frame
    void Update()
    {

    }
}
