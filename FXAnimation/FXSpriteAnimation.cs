using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;

public abstract class FXSpriteAnimation : FXAnimation {
	protected IEnumerator tweenRoutineEnumerator;

	[Header("- Sprite Settings -")]
	public SpriteRenderer[] sprites;
	public Color colorTintOverride = Color.white;

	[Header("- Tween Settings -")]
	[Range(0,3)]
	public float tweenTime = 0.5f;
	[Range(0,10)]
	public int tweenSteps = 5;

	[Space(8)]

	[ContextMenuItem("Remove Tween","RemovePositionTween")]
	public Vector2 positionTween;
	public AnimationCurve positionCurve;

	[Space(8)]

	[ContextMenuItem("Remove Tween","RemoveRotationTween")]
	[Range(0,720)]
	public float rotationTween;
	public AnimationCurve rotationCurve;

	[Space(8)]

	[ContextMenuItem("Remove Tween","RemoveScaleTween")]
	public Vector2 scaleTween;
	public AnimationCurve scaleCurve;

	[Space(8)]

	public bool fadeOut;
	public AnimationCurve fadeCurve;

	[ContextMenu("Auto Fill Sprites")]
	protected void AutoFill()
	{
		SpriteRenderer[] allSr = GetComponentsInChildren<SpriteRenderer>();
		sprites = allSr;
		Debug.Log("Array filled with " + allSr.Length + " sprites.");
	}

	[ContextMenu("Set Effects Sorting Layer")]
	protected void SetEffectsSortingLayer()
	{
		SpriteRenderer[] allSr = GetComponentsInChildren<SpriteRenderer>();
		foreach(SpriteRenderer sr in allSr)
		{
			sr.sortingLayerName = "Effects";
		}
		Debug.Log(allSr.Length + " sprites set to Effects sorting layer.");
	}

	[ContextMenu("Set EffectsBehind Sorting Layer")]
	protected void SetEffectsBehindSortingLayer()
	{
		SpriteRenderer[] allSr = GetComponentsInChildren<SpriteRenderer>();
		foreach(SpriteRenderer sr in allSr)
		{
			sr.sortingLayerName = "EffectsBehind";
		}
		Debug.Log(allSr.Length + " sprites set to EffectsBehind sorting layer.");
	}	

	[ContextMenu("Set EffectsUI Sorting Layer")]
	protected void SetEffectsUISortingLayer()
	{
		SpriteRenderer[] allSr = GetComponentsInChildren<SpriteRenderer>();
		foreach(SpriteRenderer sr in allSr)
		{
			sr.sortingLayerName = "EffectsUI";
		}
		Debug.Log(allSr.Length + " sprites set to EffectsUI sorting layer.");
	}	

	protected void RemovePositionTween()
	{
		positionTween = Vector2.zero;
		positionCurve = new AnimationCurve();
	}

	protected void RemoveRotationTween()
	{
		rotationTween = 0;
		rotationCurve = new AnimationCurve();
	}

	protected void RemoveScaleTween()
	{
		scaleTween = Vector2.zero;
		scaleCurve = new AnimationCurve();
	}

	protected override void Awake()
	{
		foreach(SpriteRenderer sr in sprites)
		{
			sr.enabled = false;
			Color c = sr.color;
			if(colorTintOverride != Color.white)
			{
				c.r = colorTintOverride.r;
				c.g = colorTintOverride.g;
				c.b = colorTintOverride.b;
			}
			sr.color = c;
		}

		base.Awake();
	}

	public override void Stop()
	{
		if(tweenRoutineEnumerator != null)
		{
			StopCoroutine(tweenRoutineEnumerator);
		}
		foreach(SpriteRenderer sr in sprites)
		{
			sr.enabled = false;
		}
		base.Stop();
	}

	public override void Play(Vector3 playAtPosition,bool flipVertical,bool flipHorizontal,bool useLocalPosition,Vector3? specificScale = null  )
	{
		if(tweenRoutineEnumerator != null)
		{
			StopCoroutine(tweenRoutineEnumerator);
		}
		base.Play(playAtPosition,flipVertical,flipHorizontal,useLocalPosition);
	}

	protected override IEnumerator PrePlayRoutine()
	{
		//Workaround. Must call base actually.

		yield return new WaitForSeconds(delayBeforePlay+delayBeforePlayCoarse);
		tweenRoutineEnumerator = TweenRoutine();
		StartCoroutine(tweenRoutineEnumerator);
		playRoutineEnumerator = PlayRoutine();
		StartCoroutine(playRoutineEnumerator);
	}

	private IEnumerator TweenRoutine()
	{
		//Debug.Log("Tween " + t.position);
		if(tweenTime == 0 || tweenSteps == 0)
		{
			yield break;
		}
		float secondsPerFrame = tweenTime/tweenSteps;
		float tweenStepsFloat = (float)tweenSteps;

		//Do nothing on first frame.
		yield return new WaitForSeconds(secondsPerFrame);

		for(int i = 1 ; i <= tweenSteps ; i++)
		{
			//Debug.Log("Round "  + i);
			if(positionTween != Vector2.zero && positionCurve.length >= 2)
			{
				float positionProgress = positionCurve.Evaluate(i/tweenStepsFloat);
				float previousPositionProgress = positionCurve.Evaluate((i-1)/tweenStepsFloat);
				Vector2 addedPosition = positionTween * ( positionProgress - previousPositionProgress);
				t.localPosition += (Vector3)addedPosition;
			}

			if(rotationTween != 0 && rotationCurve.length >= 2)
			{
				float rotationProgress = rotationCurve.Evaluate(i/tweenStepsFloat);
				float previousRotationProgress = rotationCurve.Evaluate((i-1)/tweenStepsFloat);
				float addedRotation = rotationTween * (rotationProgress - previousRotationProgress);
				t.Rotate(new Vector3(0,0,addedRotation));
			}

			if(scaleTween != Vector2.zero && scaleCurve.length >= 2)
			{
				float scaleProgress = scaleCurve.Evaluate(i/tweenStepsFloat);
				float previousScaleProgress = scaleCurve.Evaluate((i-1)/tweenStepsFloat);
				Vector2 addedScale = scaleTween * (scaleProgress - previousScaleProgress);
				t.localScale += (Vector3)addedScale;
			}

			if(fadeOut && fadeCurve.length >= 2)
			{
				float fadeProgress = fadeCurve.Evaluate(i/tweenStepsFloat);
				//float previousFadeProgress = fadeCurve.Evaluate((i-1)/tweenStepsFloat);
				float addedFade =  1-fadeProgress;
				foreach(SpriteRenderer sr in sprites)
				{
					Color c = sr.color;
					c.a = addedFade;
					sr.color = c;
				}
			}
			else
			{
				// foreach(SpriteRenderer sr in sprites)
				// {
				// 	Color c = sr.color;
				// 	c.a = 1;
				// 	sr.color = c;
				// }
			}

			yield return new WaitForSeconds(secondsPerFrame);
		}

	}

	void OnDrawGizmosSelected()
	{
#if UNITY_EDITOR
		if(UnityEditor.Selection.activeGameObject != gameObject)
		{
			Color c = Color.red;
			c.a = 0.2f;
			Gizmos.color = c;
		}
		else
		{
			Gizmos.color = Color.red;
		}
#endif
		Vector3 dest = transform.position + (Vector3)positionTween;
		Gizmos.DrawLine(transform.position,dest);
		if(positionTween != Vector2.zero)
		{
			Gizmos.DrawSphere(dest,0.08f);
		}
		Gizmos.color = Color.white;
	}

}
