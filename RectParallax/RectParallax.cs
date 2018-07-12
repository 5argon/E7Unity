using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class RectParallax : MonoBehaviour {

	public float smoothTime = 0.3f;

	/// <summary>
	/// 1.00 move the rect to its rightmost edge.
	/// </summary>
	public void ParallaxTo(float percentage)
	{
		RectTransform rt = GetComponent<RectTransform>();
		Vector2 selfSize = rt.sizeDelta;
		Vector2 parentSize = rt.parent.GetComponent<RectTransform>().sizeDelta;
		float widthDifference = selfSize.x - parentSize.x;
		rt.pivot = new Vector2(0, rt.pivot.y);
        //rt.anchoredPosition = new Vector2(-(percentage * widthDifference), rt.anchoredPosition.y);
		if(parallaxRoutine != null)
		{
			StopCoroutine(parallaxRoutine);
		}
		parallaxRoutine = ParallaxRoutine(new Vector2(-(percentage * widthDifference),rt.anchoredPosition.y));
		StartCoroutine(parallaxRoutine);
	}

	private IEnumerator parallaxRoutine;
	private IEnumerator ParallaxRoutine(Vector2 goToPosition)
	{
		RectTransform currentRect = GetComponent<RectTransform>();
		Vector2 current = currentRect.anchoredPosition;
		Vector2 velocity = Vector2.zero;
		while(current != goToPosition)
		{
            current = Vector2.SmoothDamp(current, goToPosition, ref velocity, smoothTime, Mathf.Infinity, Time.deltaTime);
			currentRect.anchoredPosition = current;
			yield return null;
		}
	}

	[ContextMenu("Test 0")]
	public void TestParallax0()
	{
		ParallaxTo(0);
	}

	[ContextMenu("Test 0.3")]
	public void TestParallax30()
	{
		ParallaxTo(0.3f);
	}

	[ContextMenu("Test 1.0")]
	public void TestParallax100()
	{
		ParallaxTo(1.0f);
	}

}
