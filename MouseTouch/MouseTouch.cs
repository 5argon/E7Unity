using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseTouch {

    private static Touch previousTouch = new Touch();
    private static Touch fakeTouch = new Touch();

    /// <summary>
    /// Consecutive calls needed to make dragging works.
    /// Check for phase Canceled for when not touching.
    /// 
    /// There is a chance that we don't get Up before the next down
    /// in the case that the previous frame is holding -> we up and down so fast that the next frame is down instead of up.
    /// Of course this is the behaviour of checking Input.GetMouse
    /// </summary>
	public static ref readonly Touch GetTouch()
	{
        fakeTouch.position = Input.mousePosition;
        if(Input.GetMouseButtonDown(0))
        {
            fakeTouch.phase = TouchPhase.Began;
        }
        else if(Input.GetMouseButtonUp(0))
        {
            fakeTouch.phase = TouchPhase.Ended;
            fakeTouch.deltaPosition = fakeTouch.position - previousTouch.position;
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
        return ref fakeTouch;
	}

}
