using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DigitSpriteEach : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField] private bool imageMode;

    [Space]

    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Image image;

	[Space]

	[SerializeField] private Animator animator;
#pragma warning restore 0649

    public virtual void SetTriggerToAnimator(string trigger)
    {
        animator.SetTrigger(trigger);
    }

    public virtual void ResetAnimator()
    {
        animator.Rebind(); //Causes a lot of lag!
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
