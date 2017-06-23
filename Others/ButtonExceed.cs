using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class ButtonExceed : Selectable , IPointerDownHandler, IPointerClickHandler ,IPointerUpHandler
{
    public Animator buttonAnimator;
    public UnityEvent down;
    public UnityEvent up;


    public override void OnPointerDown(PointerEventData eventData)
    {
        if(buttonAnimator != null)
        {
            buttonAnimator.SetTrigger("Press");
        }
        down.Invoke();
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        if(buttonAnimator != null)
        {
            buttonAnimator.SetTrigger("Normal");
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if(buttonAnimator != null)
        {
            buttonAnimator.SetTrigger("Normal");
        }
        up.Invoke();
    }
}
