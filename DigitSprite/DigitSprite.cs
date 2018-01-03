using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Want to draw digits? Have sprites? Here you go!
/// It's not stupid as it sounds. Sometimes you just can't have a font. Sometimes you want to use the same atlas as much as possible.
/// Everything are monospaced. And also, you can animate each digits freely.
/// </summary>
public class DigitSprite : MonoBehaviour {

	public Sprite[] digits;
	public Sprite plusSign;

	[SerializeField] private DigitSpriteEach[] digitSpriteEach;
	public DigitSpriteEach[] DigitSpriteEach => digitSpriteEach;

    public virtual void Display(string toDisplay)
    {
        for (int i = 0; i < DigitSpriteEach.Length; i++)
        {
            bool isDisplay = i < toDisplay.Length;
            DigitSpriteEach[i].gameObject.SetActive(isDisplay);
            if (isDisplay)
            {
                int parsed = 0;
                if (int.TryParse(toDisplay[i].ToString(), out parsed))
                {
                    DigitSpriteEach[i].Sprite = digits[parsed];
                }
                else
                {
                    if (toDisplay[i] == '+')
                    {
                        DigitSpriteEach[i].Sprite = plusSign;
                    }
                }
            }
        }
    }


    public void SetColor(Color color)
	{
        for (int i = 0; i < DigitSpriteEach.Length; i++)
        {
			DigitSpriteEach[i].Color = color;
		}
	}

	IEnumerator waitCoroutine;
	public void SetTrigger(string trigger, float delayEach, float delayBefore)
	{
        waitCoroutine = SetTriggerRoutine(trigger, delayEach == 0 ? null : new WaitForSeconds(delayEach), delayBefore == 0 ? null : new WaitForSeconds(delayBefore));
		StartCoroutine(waitCoroutine);
	}

	/// <summary>
	/// Potentially causes a lag! Animator.Rebind() is an expensive operation.
	/// </summary>
	public void Hide()
	{
		if(waitCoroutine != null)
		{
			StopCoroutine(waitCoroutine);
		}
		foreach(DigitSpriteEach dse in DigitSpriteEach)
		{
			if(dse.gameObject.activeSelf)
			{
				dse.ResetAnimator();
			}
		}
	}

	protected virtual IEnumerator SetTriggerRoutine(string trigger, WaitForSeconds delayEach = null,WaitForSeconds delayBefore = null) 
	{
		if(delayBefore != null)
		{
			yield return delayBefore;
		}
		foreach(DigitSpriteEach dse in DigitSpriteEach)
		{
			if(dse.gameObject.activeSelf)
			{
				dse.SetTriggerToAnimator(trigger);
				if(delayEach != null)
				{
					yield return delayEach;
				}
			}
		}
		yield break;
	}

}
