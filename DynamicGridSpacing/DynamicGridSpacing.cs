using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Change GridLayoutGroup's spacing dynamically based on aspect ratio breakpoints.
/// </summary>
[RequireComponent(typeof(GridLayoutGroup))]
[ExecuteInEditMode]
public class DynamicGridSpacing: MonoBehaviour
{
    public GridLayoutGroup gridLayoutGroup;
    public AnimationCurve spacingX;
    public AnimationCurve spacingY;

    [Tooltip("Even with this off, you can manually request the scaler change with UpdateSpacing()")]
    public bool alwaysUpdate;

    public void Awake()
    {
        UpdateSpacing();
    }

    public void Update()
    {
        if (alwaysUpdate || (Application.isEditor && !Application.isPlaying))
        {
            UpdateSpacing();
        }
    }

    public void UpdateSpacing()
    {
        if (gridLayoutGroup != null)
        {
            var ratio = Screen.width / (float)Screen.height;
            gridLayoutGroup.spacing = new Vector2(spacingX.Evaluate(ratio), spacingY.Evaluate(ratio));
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
