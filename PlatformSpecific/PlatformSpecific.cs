using UnityEngine;

/// <summary>
/// Ensure that the game object is active or not depending on listed platforms.
/// </summary>
public class PlatformSpecific : MonoBehaviour
{
    public RuntimePlatform[] platforms;

    void Awake()
    {
        foreach(var p in platforms)
        {
            if(Application.platform == p)
            {
                this.gameObject.SetActive(true);
                return;
            }
        }
        this.gameObject.SetActive(false);
    }
}