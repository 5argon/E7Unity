using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DigitSpriteEach : MonoBehaviour
{

    public bool imageMode;
    [Space]
    public SpriteRenderer spriteRenderer;
    public Image image;
	[Space]
	[SerializeField] private Animator animator;

    public virtual void SetTriggerToAnimator(string trigger)
    {
        animator.SetTrigger(trigger);
    }

    public Sprite Sprite
    {
        set 
        {
            if(imageMode)
            {
                image.sprite = value;
            }
            else
            {
                spriteRenderer.sprite = value;
            }
        }
    }

    public Color Color 
    {
        set 
        {
            if(imageMode)
            {
                image.color = value;
            }
            else
            {
                spriteRenderer.color = value;
            }
        }
    }

}
