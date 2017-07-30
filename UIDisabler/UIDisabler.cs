using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIDisabler : MonoBehaviour {

	public Selectable[] uis;
	public bool alsoDisableAnimator;

	public void Disable()
	{
		foreach(Selectable s in uis)
		{
			s.interactable = false;
			SetupComponents(s,false);
		}
	}

	public void Enable()
	{
		foreach(Selectable s in uis)
		{
			s.interactable = true;
			SetupComponents(s,true);
		}
	}

	private void SetupComponents(Selectable s, bool to)
	{
		Animator a = s.GetComponent<Animator>();
		if(a != null)
		{
			if(alsoDisableAnimator)
			{
				a.enabled = to;
			}
		}

        //it is very likely that "Text" is for the empty area click trick
		Text t = s.GetComponent<Text>();
		//Button b = s.GetComponent<Button>();
		if( t != null)
		{
			t.enabled = to;
		}
	}

}
