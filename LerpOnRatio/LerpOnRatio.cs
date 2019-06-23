using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LerpOnRatio : MonoBehaviour {

#pragma warning disable 0649
    [Tooltip("Time axis is width/height ratio when on landscape orientation e.g. 4/3, 16/9, 2/1")]
    [SerializeField] private AnimationCurve lerpProgressionCurve;
#pragma warning restore 0649

    void Reset()
    {
        lerpProgressionCurve = new AnimationCurve(new Keyframe[]
        {
            new Keyframe( 4f/3f, 0,1.5f,1.5f),
            new Keyframe( 16f/9f, Mathf.InverseLerp(4/3f, 2/1f, 16/9f), 1.5f, 1.5f),
            new Keyframe( 2f/1f, 1, 1.5f,1.5f),
        });
    }

    public float Value
    {
        get
        {
            bool landscape = Screen.width > Screen.height;
            if (landscape)
            {
                return lerpProgressionCurve.Evaluate(Screen.width / (float)Screen.height);
            }
            else
            {
                return lerpProgressionCurve.Evaluate(Screen.height / (float)Screen.width);
            }
        }
    }

    public static implicit operator float(LerpOnRatio lor) => lor.Value;

}
