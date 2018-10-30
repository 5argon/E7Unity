using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class BlackHide : MonoBehaviour
{
    Image im;
    void Awake()
    {
        im = GetComponent<Image>();
        im.enabled = true;
    }

    public void Unhide() => im.enabled = false;

    /// <summary>
    /// Convenience method to show the scene quick.
    /// </summary>
    public static void Unblack()
    {
        GameObject.FindObjectOfType<BlackHide>().Unhide();
    }
}
