using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LegacyAnimatorNode {

	[SerializeField] string trigger;
	[SerializeField] AnimationClip animationClip;
	[Tooltip("0 means speed 1. (no adjustment)")]
	[SerializeField] float speedAdjust;

	public string Trigger => trigger;
	public AnimationClip AnimationClip => animationClip;
	public string ClipName => animationClip.name;
	public float SpeedAdjust => speedAdjust;
	
}
