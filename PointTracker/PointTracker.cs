using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct IntVector2
{
    /// <summary>
    /// The value of x and y should not exceed this value. Increase as needed.
    /// This is used to create a non-conflicting determinant for IntVector2
    /// </summary>
    private const int maxBound = 50000;
    public int x { get; }
    public int y { get; }

    public IntVector2(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public int Determinant => (x * maxBound) + y;
    public Vector2 Vector2 => new Vector2(x, y);
}

public class PointTracker {

	private Dictionary<int,IntVector2> registeredPoints;
	private Dictionary<int,bool> registeredStates;

	public PointTracker()
	{
		registeredPoints = new Dictionary<int,IntVector2>();
		registeredStates = new Dictionary<int,bool>();
	}

	public Dictionary<int,IntVector2>.ValueCollection CurrentPoints => registeredPoints.Values;

    /// <summary>
    /// You can keep whatever state you want with a bool per point.
    /// </summary>
    public bool StateOfPoint(int x, int y)
    {
        IntVector2 point = new IntVector2(x, y);
        bool ret;
        if (registeredStates.TryGetValue(point.Determinant, out ret))
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

	public void Down(int x, int y)
	{
        //Debug.Log($"Down {x} {y}");
        IntVector2 pointDown = new IntVector2(x, y);
		registeredPoints.Add(pointDown.Determinant,pointDown);
        registeredStates.Add(pointDown.Determinant,false);
	}

    public void SetState(int x, int y, bool toState)
    {
        IntVector2 pointNow = new IntVector2(x, y);
		int det = pointNow.Determinant;
        if(registeredPoints.ContainsKey(det) && registeredStates.ContainsKey(det))
        {
            //Debug.Log($"Set state OK {x} {y} {toState}");
            registeredStates.Remove(det);
            registeredStates.Add(det, toState);
        }
        else
        {
            //Debug.Log($"Set state fail {x} {y} {toState}");
        }
    }

	public bool Move(int x, int y, int previousX, int previousY)
    {
        //Debug.Log($"Move {x} {y} {previousX} {previousY}");
        if(x == previousX && y == previousY)
        {
            //This weird bug iOS reports happen after an errornous Up.. we interpret this as Down.
            //Debug.Log($"Error Move!! {x} {y} {previousX} {previousY}");
            Down(x, y);
            return true;
        }

        IntVector2 pointPrevious = new IntVector2(previousX, previousY);
        IntVector2 pointNow = new IntVector2(x, y);
		int det = pointPrevious.Determinant;
        bool state;

        if (registeredPoints.ContainsKey(det) && registeredStates.TryGetValue(det, out state))
        {
            int detNow = pointNow.Determinant;
			registeredPoints.Remove(det);
			registeredStates.Remove(det);
			registeredPoints.Add(detNow, pointNow);
            if(!registeredStates.ContainsKey(detNow)) //somehow ArgumentException crash happen below!!
            {
                registeredStates.Add(detNow, state); //copy state
            }
            return true;
        }
		else
		{
            //Debug.LogError($"No such previous point! (move) {previousX} x {previousY}");
            return false;
		}
    }

    public bool Up(int x, int y, int previousX, int previousY)
	{
        //Debug.Log($"Up {x} {y} {previousX} {previousY}");
        IntVector2 pointPrevious = new IntVector2(previousX, previousY);
        IntVector2 pointUp = new IntVector2(x, y);
		int detPrevious = pointPrevious.Determinant;
		int detCurrent = pointUp.Determinant;

        if (registeredPoints.ContainsKey(detPrevious) && registeredStates.ContainsKey(detPrevious))
        {
			registeredPoints.Remove(detPrevious);
            registeredStates.Remove(detPrevious);
            return true;
        }
        else if (registeredPoints.ContainsKey(detCurrent) && registeredStates.ContainsKey(detCurrent))
        {
			registeredPoints.Remove(detCurrent);
            registeredStates.Remove(detCurrent);
            return true;
        }
		else
		{
            //Debug.LogError($"No such previous point! (up) {previousX} x {previousY}");
            return false;
		}
	}

}
