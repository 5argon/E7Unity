using UnityEngine;
using System.Collections;

public class FXSequence : FXSpriteAnimation {

	[Header("- Sequence Settings -")]
	[Range(0,0.5f)]
	public float secondsPerFrame = 0.2f;
	public AnimationCurve timeCurve;
	public bool freezeOnLastFrame;
	public bool loopSequence;

	protected override IEnumerator PlayRoutine()
	{
		//Debug.Log("Play Start");
		foreach(SpriteRenderer sr in sprites)
		{
			sr.enabled = false;
		}

		for(int i = 0 ; i < sprites.Length ; i++)
		{
			if(i > 0)
			{
				sprites[i-1].enabled = false;
			}
			sprites[i].enabled = true;
			yield return new WaitForSeconds(CalculateFrameTime(i,sprites.Length,secondsPerFrame,timeCurve));
			if(i == sprites.Length-1 && loopSequence)
			{
				i = -1;
				foreach(SpriteRenderer sr in sprites)
				{
					sr.enabled = false;
				}
			}
		}
		if(sprites.Length > 0 && !freezeOnLastFrame)
		{
			sprites[sprites.Length-1].enabled = false;
		}
		//Debug.Log("Play End");
	}

}
