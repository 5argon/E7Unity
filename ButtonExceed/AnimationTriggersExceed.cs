using UnityEngine;

[System.Serializable]
/// <summary>
/// Adds additional trigger to Unity's AnimationTriggers.
/// </summary>
public class AnimationTriggersExceed
{
    private const string kDefaultUpAnimName = "Up";

#pragma warning disable 0649
    [SerializeField]
    private string m_UpTrigger;
#pragma warning restore 0649

    public string upTrigger => m_UpTrigger;
}