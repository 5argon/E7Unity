using UnityEngine;
using System.Collections;

public abstract class FXAnimation : MonoBehaviour {

	[Header("- Play Settings -")]
	[Range(0,0.2f)]
	public float delayBeforePlay = 0;
	[Range(0,5)]
	public float delayBeforePlayCoarse = 0;
	[Range(0,360)]
	public int randomRotation;
	public Vector2 randomPosition;
	public Vector2 offsetPosition;
	[Space(8)]
	public bool playOnAwake;

	private Vector3 rememberLocalPosition;
	private Quaternion rememberLocalRotation;
	private Vector3 rememberLocalScale;
	protected Transform t;
	protected IEnumerator playRoutineEnumerator;
	protected IEnumerator prePlayRoutineEnumerator;
	protected bool isPlaying;

	protected virtual void Awake()
	{
		t = transform;
		rememberLocalPosition = t.localPosition;
		rememberLocalRotation = t.localRotation;
		rememberLocalScale = t.localScale;
		if(playOnAwake)
		{
			Play(transform.position,false,false,false);
		}
	}

	public virtual void Stop()
	{
		if(playRoutineEnumerator != null)
		{
			StopCoroutine(playRoutineEnumerator);
		}
		if(prePlayRoutineEnumerator != null)
		{
			StopCoroutine(prePlayRoutineEnumerator);
		}
	}

	[ContextMenu("Play at current position")]
	public void PlayAtCurrentPosition()
	{
		Play(transform.position,false,false,false);
	}

	public void Play()
	{
		Play(rememberLocalPosition,false,false,true);
	}

	public virtual void Play(Vector3 playAtPosition,bool flipVertical,bool flipHorizontal,bool useLocalPosition,Vector3? specificScale = null )
	{
		//Debug.Log( gameObject.name +  " Play at " + playAtPosition + " " + flipVertical + " " + flipHorizontal + " " + useLocalPosition);

		if(playRoutineEnumerator != null)
		{
			StopCoroutine(playRoutineEnumerator);
		}
		if(prePlayRoutineEnumerator != null)
		{
			StopCoroutine(prePlayRoutineEnumerator);
		}

		int offsetFlip = flipVertical ? -1 : 1;
		int mirrorFlip = flipHorizontal ? -1 : 1;

		Vector3 flippedOffset = offsetPosition;
		flippedOffset.x *= mirrorFlip;
		flippedOffset *= offsetFlip;

		if(useLocalPosition)
		{
			t.localPosition = playAtPosition + (Vector3)flippedOffset;
			//Debug.Log("Local!" + t.localPosition);
		}
		else
		{
			t.position = playAtPosition + (Vector3)flippedOffset;
		}
		t.localRotation = rememberLocalRotation;
		if(specificScale.HasValue) {
			t.localScale = specificScale.Value;
		}
		else {
			t.localScale = rememberLocalScale;
		}

		if(flipVertical)
		{
			//Debug.Log("Flip");
			t.Rotate(0,0,180);
		}

		Vector3 sc = t.localScale;
		sc.x  *= mirrorFlip;
		t.localScale =  sc;

		float randomRot = Random.Range(-randomRotation,randomRotation);
		t.Rotate(0,0,randomRot);

		t.position += new Vector3(Random.Range(-randomPosition.x,randomPosition.x),Random.Range(-randomPosition.y, randomPosition.y),0);

		prePlayRoutineEnumerator = PrePlayRoutine();
		StartCoroutine(prePlayRoutineEnumerator);
	}

	protected virtual IEnumerator PrePlayRoutine()
	{
		yield return new WaitForSeconds(delayBeforePlay + delayBeforePlayCoarse);
		playRoutineEnumerator = PlayRoutine();
		StartCoroutine(playRoutineEnumerator);
	}

	protected abstract IEnumerator PlayRoutine();

	protected float CalculateFrameTime(int frame,int totalFrames,float secondsPerFrame,AnimationCurve curve)
	{
		if(!(curve != null && curve.length >= 2))
		{
			//Debug.Log("Default");
			return secondsPerFrame;
		}
		if(frame >= totalFrames || frame < 0)
		{
			Debug.LogError("Frame does not exist.");
			return 0;
		}
		frame = totalFrames - 1 - frame;
		float position = (float)frame/totalFrames;
		float nextPosition = (float)(frame+1)/totalFrames;
		float progression = curve.Evaluate(position);
		float nextProgression = curve.Evaluate(nextPosition);
		float totalTime = secondsPerFrame * totalFrames;
		float result = (nextProgression - progression)* totalTime;
		//Debug.Log(frame + " " + totalFrames + " " + position + " " + nextPosition + " " + progression + " " + nextProgression + " " + "Result : " + result);
		return result;
	}
}
