//Sirawat Pitaksarit / 5argon - Exceed7 Experiments

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Easily make a button on inspector, check with .Pressed
/// </summary>
[System.Serializable]
public class ButtonBool 
{
    public bool _pressed;
    public bool Pressed
    {
        get { 
			if(_pressed)
			{
				_pressed = false;
				return true;
			}
			return false;
		}
    }

}
