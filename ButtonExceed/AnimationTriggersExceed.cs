using UnityEngine;

[System.Serializable]
/// <summary>
/// Adds additional trigger to Unity's AnimationTriggers.
/// </summary>
public class AnimationTriggersExceed
{
    private const string kDefaultUpAnimName = "Up";

    [SerializeField]
    private string m_UpTrigger;

    public string upTrigger => m_UpTrigger;
}