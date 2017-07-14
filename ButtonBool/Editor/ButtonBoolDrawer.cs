using UnityEngine;
using System;
using UnityEditor;

[CustomPropertyDrawer (typeof(ButtonBool))]
public class ButtonBoolDrawer : PropertyDrawer 
{
	public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) {

		SerializedProperty pressed =  property.FindPropertyRelative("_pressed");
		if(GUI.Button(position,property.displayName))
		{
			pressed.boolValue = true;
		}
	}
}
