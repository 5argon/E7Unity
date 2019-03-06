using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Clamp the aspect making a letterbox if it is too squared or too squashed.
/// </summary>
[ExecuteInEditMode]
public class RectCam : MonoBehaviour {

#pragma warning disable 0649
    [SerializeField] private Camera cameraComponent;
    [Space]
    [SerializeField] private bool enableSquashBound;
    [SerializeField] private bool enableSquareBound;
    [SerializeField] private float squashBound;
    [SerializeField] private float squareBound;
    [Space]
    [SerializeField] private bool setAspectOnAwake;
#pragma warning restore 0649

    void Awake()
    {
        if(setAspectOnAwake)
        {
            SetAspect();
        }
    }

#if UNITY_EDITOR
    void Update()
    {
        if(!Application.isPlaying)
        {
            SetAspect();
        }
    }
#endif

    /// <summary>
    /// You can use this to stop using RectCam for the camera.
    /// </summary>
    [ContextMenu(nameof(ResetAspect))]
    public void ResetAspect()
    {
        cameraComponent.rect = new Rect(0,0,1,1);
    }

    [ContextMenu(nameof(SetAspect))]
    public void SetAspect()
    {
        ResetAspect();

        int width = cameraComponent.pixelWidth;
        int height = cameraComponent.pixelHeight;

        float screenRatio = width / (float)height;
        //Lower number = more square
        if (enableSquareBound && screenRatio < squareBound)
        {
            //The screen too square, we calculate the height that should be lost
            var correctHeight = width / squareBound;
            var lostHeight = height - correctHeight;
            var lostHeightNormalized = lostHeight / height;
            cameraComponent.rect = new Rect(0, lostHeightNormalized / 2, 1, (1 - lostHeightNormalized));
        }
        else if (enableSquashBound && screenRatio > squashBound)
        {
            var correctWidth = height * squashBound;
            var lostWidth = width - correctWidth;
            var lostWidthNormalized = lostWidth / width;
            cameraComponent.rect = new Rect(lostWidthNormalized / 2, 0, 1 - lostWidthNormalized, 1);
        }
	}
}
