using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointTracker {

	private List<Vector2> registeredPoints;
	private Dictionary<Vector2,bool> registeredStates;

	public PointTracker()
	{
		registeredPoints = new List<Vector2>();
        registeredStates = new Dictionary<Vector2, bool>();
	}

    /// <summary>
    /// All points in this list are currently "down". Not array for performance reason so don't modify the list! Just read it!
    /// </summary>
	public List<Vector2> CurrentPoints => registeredPoints;

    /// <summary>
    /// You can keep whatever state you want with a bool per point.
    /// </summary>
    public bool StateOfPoint(Vector2 point)
    {
        bool ret;
        if (registeredStates.TryGetValue(point, out ret))
        {
            //Debug.Log($"State of {x} {y} is {ret}");
            return ret;
        }
        else
        {
            //Debug.Log($"State of {x} {y} not found");
            return false;
        }
    }

    public void Reset()
    {
        registeredPoints.Clear();
    }

	public void Down(Vector2 pointDown)
	{
        //Debug.Log($"Down {x} {y}");
		registeredPoints.Add(pointDown);
        registeredStates.Add(pointDown,false);
	}

    public void SetState(Vector2 pointNow, bool toState)
    {
        if(registeredPoints.Contains(pointNow) && registeredStates.ContainsKey(pointNow))
        {
            //Debug.Log($"Set state OK {x} {y} {toState}");
            registeredStates.Remove(pointNow);
            registeredStates.Add(pointNow, toState);
        }
        //else
        //{
            //Debug.Log($"Set state fail {x} {y} {toState}");
        //}
    }

	public bool Move(Vector2 pointNow, Vector2 pointPrevious)
    {
        //Debug.Log($"Move {x} {y} {previousX} {previousY}");
        if(pointNow == pointPrevious)
        {
            //This weird bug iOS reports happen after an errornous Up.. we interpret this as Down.

            //Debug.Log($"Error Move!! {x} {y} {previousX} {previousY}");
            Down(pointNow);
            return true;
        }

        bool state;

        if (registeredPoints.Contains(pointPrevious) && registeredStates.TryGetValue(pointPrevious, out state))
        {
			registeredPoints.Remove(pointPrevious);
			registeredStates.Remove(pointPrevious);
			registeredPoints.Add(pointNow);
            if(!registeredStates.ContainsKey(pointNow)) //somehow ArgumentException crash happen below!!
            {
                registeredStates.Add(pointNow, state); //copy state
            }
            return true;
        }
		else
		{
            //Debug.LogError($"No such previous point! (move) {previousX} x {previousY}");
            return false;
		}
    }

    public bool Up(Vector2 pointUp, Vector2 pointPrevious)
	{
        //Debug.Log($"Up {x} {y} {previousX} {previousY}");

        if (registeredPoints.Contains(pointPrevious) && registeredStates.ContainsKey(pointPrevious))
        {
			registeredPoints.Remove(pointPrevious);
            registeredStates.Remove(pointPrevious);
            return true;
        }
        else if (registeredPoints.Contains(pointUp) && registeredStates.ContainsKey(pointUp))
        {
			registeredPoints.Remove(pointUp);
            registeredStates.Remove(pointUp);
            return true;
        }
		else
		{
            //Debug.LogError($"No such previous point! (up) {previousX} x {previousY}");
            return false;
		}
	}

}
