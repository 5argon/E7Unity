using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;

//Animation event can only use the component that is attached to the same place.
//So we can forward it to call other things with this.

public class AnimationEventForwarder : MonoBehaviour {

	public UnityEvent[] forwards;

	public void Forward(int forwardIndex)
	{
		if(forwards.Length > forwardIndex)
		{
			forwards[forwardIndex].Invoke();
		}
		else
		{
			throw new Exception("Forward index out of range!");
		}
	}
}
