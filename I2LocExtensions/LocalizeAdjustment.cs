using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using I2.Loc;

[System.Serializable]
public class LocalizeAdjustmentValue
{
	public string languageCode; //like "en"
	public int x,y,w,h = 0; //adds to the inital anchoredPosition and sizeDelta 
	public Vector3 scale = Vector3.zero; //adds to the initial scale
}

[RequireComponent(typeof(RectTransform))]
public class LocalizeAdjustment : MonoBehaviour {

	public List<LocalizeAdjustmentValue> adjustmentValues;
	private Vector2 initialAnchoredPosition;
	private Vector2 initialSizeDelta;
	private Vector3 initialScale;
	private RectTransform rect;

	public void Awake()
	{
		rect = GetComponent<RectTransform>();
		initialAnchoredPosition = rect.anchoredPosition;
		initialSizeDelta = rect.sizeDelta;
		initialScale = rect.localScale;
	}

	public void Adjust()
	{
		if(rect == null)
		{
			return;
		}

        LocalizeAdjustmentValue adjustmentValue = adjustmentValues.Find(value => value.languageCode == LocalizationManager.CurrentLanguageCode);

		if(adjustmentValue != null)
		{
			rect.anchoredPosition = initialAnchoredPosition + new Vector2(adjustmentValue.x,adjustmentValue.y);
			rect.sizeDelta = initialSizeDelta + new Vector2(adjustmentValue.w,adjustmentValue.h);
			rect.localScale = initialScale + adjustmentValue.scale;
		}
		else
		{
			rect.anchoredPosition = initialAnchoredPosition;
			rect.sizeDelta = initialSizeDelta;
			rect.localScale = initialScale;
		}
	}

}
