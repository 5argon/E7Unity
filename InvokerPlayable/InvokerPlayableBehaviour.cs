using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using System;

public class InvokerPlayableBehaviour : PlayableBehaviour
{
    public Action InvokeOnPlay { set; private get; }

	public override void OnBehaviourPlay(Playable playable, FrameData info) {
		//Debug.Log("Invoking...");
		if(Application.isPlaying)
		{
			InvokeOnPlay.Invoke();
		}
	}
}
