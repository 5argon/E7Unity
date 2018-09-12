using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Change CanvasScaler's match width/height dynamically based on aspect ratio breakpoints.
/// On a screen that is too long you may want to scale towards height, on squarer screen towards width. etc. 
/// </summary>
[RequireComponent(typeof(CanvasScaler))]
[ExecuteInEditMode]
public class DynamicCanvasScaler : MonoBehaviour
{
    public CanvasScaler canvasScaler;
    public AnimationCurve aspectCurve;

    [Tooltip("Even with this off, you can manually request the scaler change with UpdateScaler()")]
    public bool alwaysUpdate;

    public void Awake()
    {
        UpdateScaler();
    }

    public void Update()
    {
        if (alwaysUpdate || (Application.isEditor && !Application.isPlaying))
        {
            UpdateScaler();
        }
    }

    public void UpdateScaler()
    {
        if (canvasScaler != null)
        {
            canvasScaler.matchWidthOrHeight = aspectCurve.Evaluate(Screen.width / (float)Screen.height);
        }
    }

    /// <summary>
    /// Without this sometimes the layout does not update when switching around the aspect ratio
    /// </summary>
    public void OnEnable()
    {
        Update();
    }
}
