using UnityEngine;
using System.Collections;

public class FXStills : FXSpriteAnimation {

	[Header("- Stills Settings -")]
	[Range(0,3)]
	[ContextMenuItem("Match Tween Time","MatchTweenTime")]
	public float appearanceTime = 0.5f;

	private void MatchTweenTime()
	{
		appearanceTime = tweenTime;
	}

	protected override IEnumerator PlayRoutine()
	{
		foreach(SpriteRenderer sr in sprites)
		{
			sr.enabled = true;
		}
		yield return new WaitForSeconds(appearanceTime);
		foreach(SpriteRenderer sr in sprites)
		{
			sr.enabled = false;
		}
	}
}
