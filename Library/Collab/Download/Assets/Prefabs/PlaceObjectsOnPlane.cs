using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class PlaceObjectsOnPlane : MonoBehaviour
{
   [SerializeField]
   ARRaycastManager  m_RaycastManager;
   static List<ARRaycastHit> s_Hits = new List<ARRaycastHit>();
   [SerializeField]
   GameObject m_ObjectToPlace;


    // Update is called once per frame
    void Update()
    {
          if(Input.touchCount > 0){
            Touch touch = Input.GetTouch(0);
            if(touch.phase == TouchPhase.Began){
              if(m_RaycastManager.Raycast(touch.position,s_Hits, TrackableType.PlaneWithinPolygon)){
                Pose hitPose = s_Hits[0].pose;
                Instantiate(m_ObjectToPlace, hitPose.position,hitPose.rotation);
              }
            }
          }
    }
 }
