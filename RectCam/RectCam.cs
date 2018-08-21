using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// If you put 16 and 9 for example, on iPad you will get a top and bottom letterbox. and the rest will
/// looks just like iPhone 5 size. Use this if it is difficult to make your UI responsive.
/// </summary>
public class RectCam : MonoBehaviour {

    [SerializeField] private Camera cameraComponent;
    [SerializeField] private int widthRatio;
    [SerializeField] private int heightRatio;
    [SerializeField] private bool setAspectOnAwake;

    void Awake()
    {
        if(setAspectOnAwake)
        {
            SetAspect();
        }
    }

    /// <summary>
    /// You can use this to stop using RectCam for the camera.
    /// </summary>
    public void ResetAspect()
    {
        cameraComponent.ResetAspect();
    }

    [ContextMenu(nameof(SetAspect))]
    public void SetAspect()
    {
        cameraComponent.ResetAspect();
        float heightPixel = (float)Screen.width * heightRatio / widthRatio;
        float letterboxHeightSum = Screen.height - heightPixel;
        float letterboxHalfNormalized = (letterboxHeightSum / 2) / Screen.height;
        Rect renderRect = cameraComponent.rect;
        renderRect.y = letterboxHalfNormalized;
        renderRect.height = 1 - (letterboxHalfNormalized * 2);
        cameraComponent.rect = renderRect;
	}
}
