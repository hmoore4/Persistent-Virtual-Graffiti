using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.UI;

public class PlacePaint : MonoBehaviour
{

    public ARRaycastManager raycastManager;
    public ARPlaneManager arPlaneManager;
    public GameObject toPlace;
    public static int contentPlaced = 0;
    public static List<ARRaycastHit> rayHits = new List<ARRaycastHit>();
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
     Vector2 screenCenter = new Vector2(Screen.width / 2, Screen.height / 2);
      if(contentPlaced < 8){
        if(raycastManager.Raycast(screenCenter, rayHits,TrackableType.PlaneWithinPolygon)){
        placeContent(rayHits[0]);
        contentPlaced++;

        }
      }
    }
    public void placeContent(ARRaycastHit hit){
      Pose hitPose = hit.pose;
      TrackableId planeToPlace = hit.trackableId;
      Debug.Log("attempting to place on: " + planeToPlace);

      Vector3 newPos = hitPose.position + hitPose.up * 0.01f;
      toPlace.transform.position = Vector3.Lerp(toPlace.transform.position, newPos, Time.deltaTime * 6.0f);
      ARPlane plane = arPlaneManager.GetPlane(planeToPlace);
      if(Application.platform == RuntimePlatform.IPhonePlayer){
        if(System.Math.Abs(plane.transform.eulerAngles.z - 180 ) < 0.1){
          toPlace.transform.eulerAngles = new Vector3(0, plane.transform.eulerAngles.y - 180, 0);
        }
        else{
          toPlace.transform.eulerAngles = new Vector3(0, plane.transform.eulerAngles.y,0);
        }
      }
      Instantiate(toPlace,hitPose.position,hitPose.rotation);
    }

}
