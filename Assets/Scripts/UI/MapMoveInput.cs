using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MapMoveInput : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    Vector2 lastPos;
    public Vector2 deltaMove;
    public bool moving;

    int pointerID;

    private void Update()
    {
        if (moving)
        {
            if (pointerID == -1)
            {
                deltaMove = (Vector2)Input.mousePosition - lastPos;
                lastPos = Input.mousePosition;
            }
            else
            {
                deltaMove = Input.touches[pointerID].deltaPosition;
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        moving = true;
        pointerID = eventData.pointerId;

        if (pointerID == -1)
        {
            lastPos = Input.mousePosition;
        }

        Debug.Log($"OnPointerDown({pointerID})");
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        moving = false;
        pointerID = -1;
        deltaMove = Vector2.zero;
        lastPos = Vector2.zero;
    }
}
