using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class PanelPlaceholder : MonoBehaviour {

	private RectTransform rectTransform;
	public bool overrideScale;

	public void ReplaceWith(RectTransform panel)
	{
		rectTransform = GetComponent<RectTransform>();

		panel.transform.SetParent(gameObject.transform.parent,true);
        panel.transform.SetSiblingIndex(gameObject.transform.GetSiblingIndex());
		if(overrideScale)
		{
			panel.localScale = rectTransform.localScale;
		}
		panel.anchorMax = rectTransform.anchorMax;
		panel.anchorMin = rectTransform.anchorMin;
		panel.localPosition = rectTransform.localPosition;
		panel.offsetMin = rectTransform.offsetMin;
		panel.offsetMax = rectTransform.offsetMax;
		Destroy(gameObject);
	}

}
