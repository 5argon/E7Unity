using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor;

/// <summary>
/// For when you want your child RectTransform to follow the parent scaling as if it is the same piece of image.
/// Set the bound as you like, then right click on `RectTransform` to use the context menu that make the anchor points works that way.
/// </summary>
public static class FollowScaleAnchor
{
    [MenuItem("CONTEXT/RectTransform/Set Anchors to Follow Parent Scaling", false)]
    public static void SetFollowScaleAnchor()
    {
        try
        {
            var rt = Selection.activeGameObject.GetComponent<RectTransform>();
            var parentRt = rt.parent.GetComponent<RectTransform>();

            Undo.RegisterCompleteObjectUndo(rt, nameof(SetFollowScaleAnchor));
            var bound = RectTransformUtility.CalculateRelativeRectTransformBounds(parentRt.transform, rt.transform);

            var minX = ((parentRt.rect.width / 2) + bound.center.x - bound.extents.x) / parentRt.rect.width;
            var maxX = ((parentRt.rect.width / 2) + bound.center.x + bound.extents.x) / parentRt.rect.width;
            var minY = ((parentRt.rect.height / 2) + bound.center.y - bound.extents.y) / parentRt.rect.height;
            var maxY = ((parentRt.rect.height / 2) + bound.center.y + bound.extents.y) / parentRt.rect.height;

            var anchorMin = new Vector2(minX, minY);
            var anchorMax = new Vector2(maxX, maxY);

            //By setting this the position will move.
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;

            //Counter the position to be at the same place.
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, bound.extents.x * 2);
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, bound.extents.y * 2);
            rt.anchoredPosition = Vector2.zero;
        }
        catch (MissingComponentException)
        {
            Debug.LogError("No RectTransform in the parent!");
            return;
        }
    }

}
