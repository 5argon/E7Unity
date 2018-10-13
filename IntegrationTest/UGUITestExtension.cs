//When integration testing using "on device" button DEVELOPMENT_BUILD is automatically on.
#if UNITY_EDITOR || (DEVELOPMENT_BUILD && !UNITY_EDITOR)
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public static class UGUITestExtension
{
    /// <summary>
    /// Searches all layers of Animator of this GameObject does any of them at a specified state name?
    /// Specify layerIndex to look in only one layer.
    /// If you just issue a SetTrigger to change state for example, one frame is required to make it take effect.
    /// </summary>
    public static bool AnimatorAtState(this Component go, string stateName, int layerIndex = -1)
    {
        Animator ani = go.GetComponent<Animator>();
        return ani.AtState(stateName, layerIndex);
    }

    /// <summary>
    /// Searches all layers of Animator is any of them at a specified state name?
    /// Specify layerIndex to look in only one layer.
    /// If you just issue a SetTrigger to change state for example, one frame is required to make it take effect.
    /// </summary>
    public static bool AtState(this Animator ani, string stateName, int layerIndex = -1)
    {
        if (layerIndex == -1)
        {
            for (int i = 0; i < ani.layerCount; i++)
            {
                if (ani.GetCurrentAnimatorStateInfo(i).IsName(stateName))
                {
                    return true;
                }
            }
            return false;
        }
        else
        {
            return ani.GetCurrentAnimatorStateInfo(layerIndex).IsName(stateName);
        }
    }

    private static bool IsOutOfScreen(this Graphic graphic)
    {
        RectTransform rect = graphic.rectTransform;
        Vector3[] worldCorners = new Vector3[4];
        rect.GetWorldCorners(worldCorners); //This is already screen space not world! wtf!

        Camera activeCamera = Camera.main;
        Vector3 bottomLeft = worldCorners[0];
        Vector3 topLeft = worldCorners[1];
        //Vector3 topRight = worldCorners[2];
        Vector3 bottomRight = worldCorners[3];

        //Debug.Log($"{bottomLeft.x} < {Screen.width} && {bottomRight.x} > 0 && {topLeft.y} > 0 && {bottomRight.y} < {Screen.height}");

        if(bottomLeft.x < Screen.width && bottomRight.x > 0 && topLeft.y > 0 && bottomRight.y < Screen.height)
        {
            return false; //Rect overlaps, therefore it is not out of screen
        }
        else
        {
            return true;
        }
    }
    
    private static bool HasZeroRectSize(this Graphic graphic) => graphic.rectTransform.rect.width == 0 || graphic.rectTransform.rect.height == 0;
    private static bool HasZeroScale(this Graphic graphic) => graphic.transform.localScale.x == 0 || graphic.transform.localScale.y == 0;

    /// <summary>
    /// An extension method to check visually can we see the graphic or not.
    /// It does not check for null Sprite since that will be rendered as a white rectangle.
    /// It does not check for transparency resulting from parent CanvasGroup.
    /// For Text, it does not account for empty text or truncated text.
    /// </summary>
    public static bool GraphicVisible(this Graphic graphic)
    {
        //Debug.Log($"{graphic.IsOutOfScreen()} || {graphic.HasZeroRectSize()} || {graphic.HasZeroScale()} || {graphic.gameObject.activeInHierarchy == false} || {graphic.enabled == false} || {graphic.color.a == 0} || {ComponentInvisible(graphic)}");

        if (graphic.IsOutOfScreen() || graphic.HasZeroRectSize() || graphic.HasZeroScale() || graphic.gameObject.activeInHierarchy == false || graphic.enabled == false || graphic.color.a == 0 || ComponentInvisible(graphic))
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    /// <summary>
    /// In here we examine all other factors not common in Graphic if it makes the thing invisible or not.
    /// </summary>
    private static bool ComponentInvisible(Graphic graphic)
    {
        Text t  = graphic.GetComponent<Text>();
        if(t != null && t.text == "")
        {
            return true;
        }

        TextMeshProUGUI tmpro  = graphic.GetComponent<TextMeshProUGUI>();
        if(t != null && (t.text == "" || t.color.a == 0))
        {
            return true;
        }

        return false; //It's visible
    }

    private static Vector2 Center(this Graphic graphic) => InteHelper.CenterOfRectTransform(graphic.rectTransform);
    public static void ClickAtCenter(this Graphic graphic) => InteHelper.RaycastClick(graphic.Center());
}


#endif