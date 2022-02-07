using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class ButtonHold :  MonoBehaviour,  IPointerDownHandler, IPointerUpHandler{

  public void OnPointerDown(PointerEventData eventData){
    Painter.spray = true;
    Debug.Log("SPRAYING");
  }
  public void OnPointerUp(PointerEventData eventData){
    Painter.spray = false;
  }
}
