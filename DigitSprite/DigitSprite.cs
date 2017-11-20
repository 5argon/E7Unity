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

	public DigitSpriteEach[] digitSpriteEach;


    public void Display(string toDisplay)
    {
        for (int i = 0; i < digitSpriteEach.Length; i++)
        {
            bool isDisplay = i < toDisplay.Length;
            digitSpriteEach[i].gameObject.SetActive(isDisplay);
            if (isDisplay)
            {
                int parsed = 0;
                if (int.TryParse(toDisplay[i].ToString(), out parsed))
                {
                    digitSpriteEach[i].Sprite = digits[parsed];
                }
                else
                {
                    if (toDisplay[i] == '+')
                    {
                        digitSpriteEach[i].Sprite = plusSign;
                    }
                }
            }
        }
    }

    public void SetColor(Color color)
	{
        for (int i = 0; i < digitSpriteEach.Length; i++)
        {
			digitSpriteEach[i].Color = color;
		}
	}

	public void SetTrigger(string trigger, float delayEach, float delayBefore)
	{
        StartCoroutine(SetTriggerRoutine(trigger, delayEach == 0 ? null : new WaitForSeconds(delayEach), delayBefore == 0 ? null : new WaitForSeconds(delayBefore)));
	}

	IEnumerator SetTriggerRoutine(string trigger, WaitForSeconds delayEach = null,WaitForSeconds delayBefore = null) 
	{
		if(delayBefore != null)
		{
			yield return delayBefore;
		}
		foreach(DigitSpriteEach dse in digitSpriteEach)
		{
			if(dse.gameObject.activeSelf)
			{
				dse.animator.SetTrigger(trigger);
				if(delayEach != null)
				{
					yield return delayEach;
				}
			}
		}
		yield break;
	}

}
