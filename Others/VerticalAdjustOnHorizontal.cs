using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VerticalAdjustOnHorizontal : MonoBehaviour {

	public RectTransform rect;
	public float maxAdjustment;

	void Awake() {
		float adjustment = maxAdjustment * Mathf.InverseLerp(16/9f, 4/3f, Screen.width/(float)Screen.height);
		rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, rect.anchoredPosition.y + adjustment);
	}
	
}
