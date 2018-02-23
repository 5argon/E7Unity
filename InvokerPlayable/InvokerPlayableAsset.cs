using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;
using System;

public interface INamedInvoker
{
    string SelectedMethodToString();
}

[System.Serializable]
public abstract class InvokerPlayableAsset<T> : PlayableAsset, INamedInvoker where T : UnityEngine.Object
{
    [SerializeField] private ExposedReference<T> invokeTarget;

    public string SelectedMethodToString() => selectedMethod.ToString();

    protected abstract Enum selectedMethod { get; }
    protected abstract Dictionary<Enum, Action> MethodPairings(T t);

    public override Playable CreatePlayable(PlayableGraph graph, GameObject go)
    {
        T resolvedInvokeTarget = invokeTarget.Resolve(graph.GetResolver());
        if (resolvedInvokeTarget != null)
        {
            InvokerPlayableBehaviour invoker = new InvokerPlayableBehaviour();
            Action action;
            if (MethodPairings(resolvedInvokeTarget).TryGetValue(selectedMethod, out action))
            {
                invoker.InvokeOnPlay = action;
                return ScriptPlayable<InvokerPlayableBehaviour>.Create(graph, invoker);
            }
            else
            {
                Debug.LogError($"The enum {selectedMethod} is not paired with any delegate.");
            }
        }
        return Playable.Create(graph);
    }
}

