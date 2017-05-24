using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
Attach this to camera and it will adjust the rendering area to your desired ratio on awake.

If you put 16 and 9 for example, on iPad you will get a top and bottom letterbox. and the rest will
looks just like iPhone 5 size. Use this if it is difficult to make your UI responsive.
 */
public class RectCam : MonoBehaviour {

    public Camera cameraComponent;
    public int widthRatio;
    public int heightRatio;
    public float aspect;
    public bool aspectUpdate;

    public void ResetAspect()
    {
        cameraComponent.ResetAspect();
    }

    void Awake()
    {
        Update();
    }

	void Update () {
        if(aspectUpdate)
        {
            cameraComponent.aspect = aspect;
        }
        else
        {
            cameraComponent.ResetAspect();
            float heightPixel = (float)Screen.width * heightRatio / widthRatio;
            float letterboxHeightSum = Screen.height - heightPixel;
            float letterboxHalfNormalized = (letterboxHeightSum /2) / Screen.height;
            Rect renderRect = cameraComponent.rect;
            renderRect.y = letterboxHalfNormalized;
            renderRect.height = 1- (letterboxHalfNormalized * 2);
            cameraComponent.rect = renderRect;
        }
		
	}
}
