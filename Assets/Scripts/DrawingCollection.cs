using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public struct PseudoHash{
  public Vector3 center;
  public float radius;
}
public struct GlobalPositionOrigin{
  public float lat;
  public float lon;
  public float heading;
}
public struct shipable{
  public static int id;
  public static GlobalPositionOrigin initialGPSWithHeading;
  public static List<PseudoHash> phash;
}
public class DrawingCollection{

  private int id; //Eventually using a gps ID or independent drawID
  private List<Pose> drawing = new List<Pose>(); //Get this once create mode finshed and shipped from PlaceObjectsOnPlane;
  private GlobalPositionOrigin initialGPSWithHeading; // Get gps so we can do operations on the centers of the gameObjects
  private List<PseudoHash> painting = new List<PseudoHash>();
  private float rad;



  public DrawingCollection(List<Pose> drawing){
    this.drawing = drawing;
  }
  /*public void setRad(GameObject g){
    rad = g.GetComponent<Sphere>.radius;
  }*/
  public void setId(int id){
    this.id = id;
  }

  public void setPseudoHash(){
    foreach(Pose p in drawing){
      PseudoHash ph = new PseudoHash();
      Vector3 temp = p.position;
      ph.center = temp;
      ph.radius = rad;
      painting.Add(ph);
    }
  }
  public List<PseudoHash> getPsuedoHash(){
    return painting;
  }
  public void setInitialGPSHeading(float lat, float lon, float heading){
    GlobalPositionOrigin gpo = new GlobalPositionOrigin();
    gpo.lat = lat;
    gpo.lon = lon;
    gpo.heading = heading;
  }
  public GlobalPositionOrigin getInitialGPSHeading(){
    return initialGPSWithHeading;
  }
  public void setShipable(int id, GlobalPositionOrigin gpo, List<PseudoHash> phash){
    shipable.id = id;
    shipable.initialGPSWithHeading = gpo;
    shipable.phash = phash;
  }

}
