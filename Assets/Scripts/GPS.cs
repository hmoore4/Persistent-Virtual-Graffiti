using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GPS : MonoBehaviour
{
    public static float longitude;
    public static float latitude;
	public static float initial_longitude;
    public static float initial_latitude;
	public static float initial_heading;
    public static float altitude;
	public static int hasBeenInitialized =0;
    public static float heading;
    public static float heading_confidence;

    // a public object to use in other classes
    public static GPS instance;

    private void Awake(){


        if(instance == null){

            instance = this;
        }
        DontDestroyOnLoad(gameObject);
    }

    private void FixedUpdate(){
        if(Input.location.isEnabledByUser){
            latitude = Input.location.lastData.latitude;
            longitude = Input.location.lastData.longitude;
            altitude = Input.location.lastData.altitude;
            heading = Input.compass.trueHeading;
            heading_confidence = Input.compass.headingAccuracy;

        }


    }
    // Start is called before the first frame update
   IEnumerator Start()
    {
        // First, check if user has location service enabled
        if (!Input.location.isEnabledByUser)
            yield break;

        // Start service before querying location
        // First argument is how accurate within meters you want the GPS to be, second arg is updating when you move X meters.
        // Common wisdom is it somewhat breaks down going smaller than 5 meters
        Input.location.Start(5f,5f);
        Input.compass.enabled = true;
        Debug.Log("D");

        // Wait until service initializes
        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        // Service didn't initialize in 20 seconds
        if (maxWait < 1)
        {
            print("Timed out");
            yield break;
        }

        // Connection has failed
        if (Input.location.status == LocationServiceStatus.Failed)
        {
            print("Unable to determine device location");
            yield break;
        }
        else
        {
            // Access granted and location value could be retrieved
			if(hasBeenInitialized == 0){
				initial_latitude = Input.location.lastData.latitude;
				initial_longitude = Input.location.lastData.longitude;
				initial_heading = Input.compass.trueHeading;
				hasBeenInitialized =1;
			}
			print("Location: " + Input.location.lastData.latitude + " " + Input.location.lastData.longitude + " " + Input.location.lastData.altitude + " " + Input.location.lastData.horizontalAccuracy + " " + Input.location.lastData.timestamp);
        }

        // Stop service if there is no need to query location updates continuously
       // Input.location.Stop();
    }

    // Update is called once per frame
    void Update()
    {

    }

}
