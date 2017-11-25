using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseTouch {

    private static Touch previousTouch = new Touch();
    private static Touch fakeTouch = new Touch();

    /// <summary>
    /// Consecutive calls needed to make dragging works.
    /// Check for phase Canceled for when not touching.
    /// </summary>
	public static Touch GetTouch()
	{
        fakeTouch.position = Input.mousePosition;
        if(Input.GetMouseButtonDown(0))
        {
            fakeTouch.phase = TouchPhase.Began;
        }
        else if(Input.GetMouseButtonUp(0))
        {
            fakeTouch.phase = TouchPhase.Ended;
        }
        else if (Input.GetMouseButton(0))
        {
            //moved or not?
            if(fakeTouch.position == previousTouch.position)
            {
                fakeTouch.phase = TouchPhase.Stationary;
                fakeTouch.deltaPosition = Vector2.zero;
            }
            else
            {
                fakeTouch.phase = TouchPhase.Moved;
                fakeTouch.deltaPosition = fakeTouch.position - previousTouch.position;
            }
        }
        else
        {
            fakeTouch.phase = TouchPhase.Canceled;
        }
        previousTouch = fakeTouch;
        //Debug.Log("MouseTouch : " + fakeTouch.position + " " + fakeTouch.deltaPosition + " " + fakeTouch.phase);
        return fakeTouch;
	}

}
