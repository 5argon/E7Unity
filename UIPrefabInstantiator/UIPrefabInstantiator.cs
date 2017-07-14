using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Instantiate UI prefab and stretch the rect to the current rect transform
/// Use the toggle when working on the game and turn it off when we are applying outer prefab to avoid nested prefab problem.
/// </summary>
[RequireComponent(typeof(RectTransform))]
[ExecuteInEditMode]
public class UIPrefabInstantiator : MonoBehaviour {

	public ButtonBool togglePrefab;
	public ButtonBool addPrefab;
	public ButtonBool removePrefab;
	[Space]
	public GameObject prefab;
	private GameObject instantiatedPrefab;

    public void Update()
    {
        if (Application.isEditor)
        {
            if (togglePrefab.Pressed)
            {
                if (IsInstantiated)
                {
                    Destroy();
                }
                else
                {
                    Instantiate();
                }
            }
            else if (addPrefab.Pressed)
            {
                Instantiate();
            }
            else if (removePrefab.Pressed)
            {
                Destroy();
            }
        }
    }

    private bool IsInstantiated
    {
        get
        {
            if (gameObject.transform.childCount > 0 && instantiatedPrefab != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

	public T GetComponentOfInstantiated<T>()
	{
		return instantiatedPrefab.GetComponent<T>();
	}

	public T Instantiate<T>()
	{
		Instantiate();
		return GetComponentOfInstantiated<T>();
	}

    [ContextMenu("Instantiate")]
    public void Instantiate()
    {
        if (Application.isEditor)
        {
#if UNITY_EDITOR
            instantiatedPrefab = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
#endif
        }
        else
        {
            instantiatedPrefab = Instantiate(prefab);
        }
        instantiatedPrefab.transform.SetParent(gameObject.transform);
        instantiatedPrefab.transform.localScale = Vector3.one;
		RectTransform rect = instantiatedPrefab.GetComponent<RectTransform>();
        rect.position = Vector3.zero;
		rect.anchorMin = Vector2.zero;
		rect.anchorMax = Vector2.one;
		rect.offsetMax = Vector2.zero;
		rect.offsetMin = Vector2.zero;
	}

	[ContextMenu("Destroy")]
	public void Destroy()
	{
		if(IsInstantiated || gameObject.transform.childCount > 0)
		{
			DestroyImmediate(gameObject.transform.GetChild(0).gameObject);
			instantiatedPrefab = null;
		}
	}

}
