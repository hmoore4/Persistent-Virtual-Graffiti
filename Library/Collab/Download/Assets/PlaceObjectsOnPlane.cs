 using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.Experimental.XR;

public class PlaceObjectsOnPlane : MonoBehaviour
{
   [SerializeField]
   ARRaycastManager  m_RaycastManager;
   static List<ARRaycastHit> s_Hits = new List<ARRaycastHit>();
   [SerializeField]
   GameObject m_ObjectToPlace;
    [SerializeField]
    ARPlaneManager planeManager;

    static List<Pose> poses = new List<Pose>();
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
         if (Input.touchCount > 0){
             Touch touch = Input.GetTouch(0);
             Ray ray = new Ray(transform.position, transform.TransformDirection(Vector3.down));
             if(touch.phase == TouchPhase.Began || touch.phase == TouchPhase.Moved){
               if(m_RaycastManager.Raycast(touch.position,s_Hits, TrackableType.PlaneWithinPolygon)){
                 Pose hitPose = s_Hits[0].pose;
                 poses.Add(hitPose);
                 Instantiate(m_ObjectToPlace, hitPose.position,hitPose.rotation);
                     Debug.Log("Hit At: " + hitPose.position.x + "," + hitPose.position.y + "," + hitPose.position.z);
               }
             }
           }
 }
  }
