using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor;
using static UnityEngine.RectTransform;
using UnityEditor.ShortcutManagement;

/// <summary>
/// For when you want your child RectTransform to follow the parent scaling as if it is the same piece of image.
/// Set the bound as you like, then right click on `RectTransform` to use the context menu that make the anchor points works that way.
/// </summary>
public static class FollowScaleAnchor
{
    [MenuItem("CONTEXT/RectTransform/Set Anchors to Follow Parent Scaling", false)]
    [Shortcut("Scene View/Set Anchors to Follow Parent Scaling", KeyCode.F, ShortcutModifiers.Alt)]
    public static void SetFollowScaleAnchor()
    {
        try
        {
            var rt = Selection.activeGameObject.GetComponent<RectTransform>();
            var parentRt = rt.parent.GetComponent<RectTransform>();
            if (rt == null || parentRt == null) return;

            Undo.RegisterCompleteObjectUndo(rt, nameof(SetFollowScaleAnchor));

            var bound = CalculateRelativeRectTransformBounds(parentRt, rt);

            var minX = ((parentRt.rect.width * parentRt.pivot.x) + bound.center.x - bound.extents.x) / parentRt.rect.width;
            var maxX = ((parentRt.rect.width * parentRt.pivot.x) + bound.center.x + bound.extents.x) / parentRt.rect.width;
            var minY = ((parentRt.rect.height * parentRt.pivot.y) + bound.center.y - bound.extents.y) / parentRt.rect.height;
            var maxY = ((parentRt.rect.height * parentRt.pivot.y) + bound.center.y + bound.extents.y) / parentRt.rect.height;

            var anchorMin = new Vector2(minX, minY);
            var anchorMax = new Vector2(maxX, maxY);

            //By setting this the position will move.
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;

            //Counter the position to be at the same place.
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, bound.extents.x * 2);
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, bound.extents.y * 2);

            //Pivot 0.5 is neutral, while at either extreme end require a compensation move equal to its size.
            var sizeX = rt.rect.width;
            var sizeY = rt.rect.height;
            var lerpX = Mathf.LerpUnclamped(1, -1, rt.pivot.x) * sizeX;
            var lerpY = Mathf.LerpUnclamped(1, -1, rt.pivot.y) * sizeY;

            rt.anchoredPosition = new Vector2(
                0,0//lerpX, lerpY
            );

            rt.sizeDelta = Vector2.zero;
            // Debug.Log(bound);
            // Debug.Log(anchorMin);
            // Debug.Log(anchorMax);
        }
        catch (MissingComponentException)
        {
            Debug.LogError("No RectTransform in the parent!");
            return;
        }
    }

    private static readonly Vector3[] s_Corners = new Vector3[4];

    private static Bounds CalculateRelativeRectTransformBounds(RectTransform root, RectTransform child)
    {
        Vector3 vMin = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        Vector3 vMax = new Vector3(float.MinValue, float.MinValue, float.MinValue);

        Matrix4x4 toLocal = root.transform.worldToLocalMatrix;

        child.GetWorldCorners(s_Corners);
        for (int j = 0; j < 4; j++)
        {
            Vector3 v = toLocal.MultiplyPoint3x4(s_Corners[j]);
            vMin = Vector3.Min(v, vMin);
            vMax = Vector3.Max(v, vMax);
        }

        Bounds b = new Bounds(vMin, Vector3.zero);
        b.Encapsulate(vMax);
        return b;
    }

}
