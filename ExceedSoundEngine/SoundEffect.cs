using UnityEngine;
using System.Collections;

[System.Serializable]
public class SoundEffect {

	public AudioClip audioClip;
	[Range(0f,1f)]
	public float volume = 1;

    // You can override this for more complex function.
    // Such as randomized walk sound, in that case Get should random from the list of SoundEffects.
    public virtual SoundEffect Get
    {
        get
        {
            return this;
        }
    }

}
