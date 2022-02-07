using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeviceManager{
  public int deviceId;
  public int drawingId;

  public DeviceManager(int deviceId, int drawingId){
    this.deviceId = deviceId;
    this.drawingId = drawingId;
  }

  public int getDeviceId(){
    return deviceId;
  }
  public void setDeviceId(int deviceId){
    this.deviceId = deviceId;
  }
  public int getDrawingId(){
    return drawingId;
  }
  public void setDrawingId(int drawingId){
    this.drawingId = drawingId;
  }
}
