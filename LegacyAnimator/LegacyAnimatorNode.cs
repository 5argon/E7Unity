using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LegacyAnimatorNode {

	[SerializeField] string trigger;
	[SerializeField] AnimationClip animationClip;

	public string Trigger => trigger;
	public AnimationClip AnimationClip => animationClip;
	public string ClipName => animationClip.name;
	

}
