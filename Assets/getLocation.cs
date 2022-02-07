using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Android;



namespace WSGPS {



public class getLocation : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        #if iOS
                  if (!Permission.HasUserAuthorizedPermission (Permission.FineLocation))
                  {
                           Permission.RequestUserPermission (Permission.FineLocation);
                  }
         #elif Android
                  PlayerSettings.iOS.locationUsageDescription = "Details to use location";
         #endif
         StartCoroutine(StartLocationService());
    }

   private IEnumerator StartLocationService()
{
         if (!Input.location.isEnabledByUser)
        {
                 Debug.Log("User has not enabled location");
                  yield break;
         }
         Input.location.Start();
         while(Input.location.status == LocationServiceStatus.Initializing)
         {
                  yield return new WaitForSeconds(1);
         }
         if (Input.location.status == LocationServiceStatus.Failed)
         {
                  Debug.Log("Unable to determine device location");
                  yield break;
         }
         Debug.Log ("Latitude : " + Input.location.lastData.latitude);
         Debug.Log ("Longitude : " + Input.location.lastData.longitude);
         Debug.Log("Altitude : " + Input.location.lastData.altitude);
}
}

}

