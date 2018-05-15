using UnityEngine;
using System;
using UnityEditor;
using System.Reflection;

[CustomPropertyDrawer (typeof(SoundEffect))]
public class SoundEffectDrawer : PropertyDrawer 
{
	public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) {

		//Rect rectOnTheRightSide = EditorGUI.PrefixLabel(position, label);
		Rect rectOnTheRightSide = position; 
		rectOnTheRightSide.width *= 0.3f;
        EditorGUI.LabelField(rectOnTheRightSide,label);
		rectOnTheRightSide.x += rectOnTheRightSide.width;
		rectOnTheRightSide.width /= 0.3f;
		rectOnTheRightSide.width *= 0.2f;
		EditorGUI.PropertyField(rectOnTheRightSide,property.FindPropertyRelative("audioClip"),GUIContent.none);
		rectOnTheRightSide.x += rectOnTheRightSide.width;
		rectOnTheRightSide.width /= 0.2f;
		rectOnTheRightSide.width *= 0.45f;
		EditorGUI.Slider(rectOnTheRightSide,property.FindPropertyRelative("volume"),0,1,GUIContent.none);
		rectOnTheRightSide.x += rectOnTheRightSide.width;
		rectOnTheRightSide.width /= 0.45f;
		rectOnTheRightSide.width *= 0.05f;
		if(GUI.Button(rectOnTheRightSide,"►"))
		{
			//SoundEffectPlayer.Instance.PlaySoundEffect(property.objectReferenceValue as SoundEffect);
			PlayAudioInEditor(property.FindPropertyRelative("audioClip").objectReferenceValue as AudioClip);
		}
	}

    public static void PlaySoundEffectSetInEditor(SoundEffectSet sfxSet)
    {
        AudioClip randomizedClip = sfxSet.Get.audioClip;
        PlayAudioInEditor(randomizedClip);
    }

    public static void PlayAudioInEditor(AudioClip clip)
    {
        Assembly unityEditorAssembly = typeof(AudioImporter).Assembly;
        System.Type audioUtilClass = unityEditorAssembly.GetType("UnityEditor.AudioUtil");

        //TODO : directly modify the audio clip to be quieter for preview..

        MethodInfo method = audioUtilClass.GetMethod("PlayClip", BindingFlags.Static | BindingFlags.Public, null,
            new System.Type[] { typeof(AudioClip) }, null);

        method.Invoke(null, new object[] { clip });
    }
}
