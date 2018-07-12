using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

[System.Serializable]
#if ODIN_INSPECTOR
[InlineProperty(LabelWidth = 5)]
#endif
public class LegacyAnimatorNode {

#if ODIN_INSPECTOR
[HideLabel][HorizontalGroup(Width=0.2f)]
#endif
	[SerializeField] string trigger;

#if ODIN_INSPECTOR
[HideLabel][HorizontalGroup(Width=0.43f)]
#endif
	[SerializeField] AnimationClip animationClip;

#if ODIN_INSPECTOR
[LabelText("Spd.")][HorizontalGroup(Width= 0.16f, LabelWidth=30)]
#endif
	[Tooltip("0 means speed 1. (no adjustment)")]
	[SerializeField] float speedAdjust;
#if ODIN_INSPECTOR
[LabelText("Lyr.")][HorizontalGroup(LabelWidth =30)]
#endif
	[Tooltip("Run this on a separate layer than those unchecked.")]
	[SerializeField] bool secondLayer;

	public string Trigger => trigger;
	public AnimationClip AnimationClip => animationClip;
	public string ClipName => animationClip.name;
	public float SpeedAdjust => speedAdjust;
	public bool SecondLayer => secondLayer;
	
}
