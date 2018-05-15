using UnityEngine;

 /*
     This is an example of how to extend SimpleSFXSystem.
     This is a special type of SoundEffect where each Play____ call results in random sounds from the pool plus small fixed pitch variation.

     Useful for repetitive sound like waking, character voice where you can design several sounds to use.
     It will take up more of your editor interface though.

     You cannot use automatic code generation since that will use normal SoundEffect.
     (More or less you have to manually design which files is in the same random group anyway, so I think coding is the best.)
 */

[System.Serializable]
public class SoundEffectSet : SoundEffect {

    public SoundEffect[] sfxPool;
    public bool pitchVariation;

    public override SoundEffect Get
    {
        get
        {
            return sfxPool[Random.Range(0,sfxPool.Length)];
        }
        
    }

}
