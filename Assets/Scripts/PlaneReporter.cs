/*﻿using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
public class PlaneReporter : MonoBehaviour
{
    public ARPlaneManager planeManager;
    public Camera cam;
    List<ARPlane> planes = new List<ARPlane>();
    NativeArray<UnityEngine.XR.ARFoundation.ARRaycastHit> hits = new NativeArray<UnityEngine.XR.ARFoundation.ARRaycastHit>();
    // Start is called before the first frame update
    void Start()
    {
      Camera cam = new Camera();
    }

    // Update is called once per frame
    void Update()
    {
      //Allocator a = new Allocator();
      Ray r = new Ray(transform.position, transform.TransformDirection(Vector3.down));
      hits = planeManager.Raycast(r,TrackableType.PlaneWithinPolygon,Allocator.Persistent);
      foreach(var plane in raycastManager.trackables){
        plane.boundary.get();
      }
      planes.Add();
    }
}*/
