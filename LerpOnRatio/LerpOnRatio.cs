using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LerpOnRatio : MonoBehaviour {

    [SerializeField] private float value43;
    [SerializeField] private float value169;

    public float Value 
    {
        get
        {
            return Mathf.Lerp(value43, value169, Mathf.InverseLerp(4 / 3f, 16 / 9f, Screen.width/(float)Screen.height));
        }
    }

}
