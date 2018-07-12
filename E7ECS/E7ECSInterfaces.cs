using Unity.Entities;
using UnityEngine;

namespace E7.ECS
{
    /// <summary>
    /// An inject struct that has a reactive component to be removed at the end of system function,
    /// plus an injected array of entities so that we knows which one to remove a component from.
    /// We need one more component data array with IReactive, but it is not enforced in this interface.
    /// </summary>
    public interface IReactiveInjectGroup<RxGroup>
    where RxGroup : struct, IReactiveGroup
    {
        SharedComponentDataArray<RxGroup> ReactiveGroups { get; }
        SharedComponentDataArray<DestroyReactivesSystem.ReactiveEntity> ReactiveEntityTag { get; }
        EntityArray Entities { get; }
    }

    public interface ITagResponseDataInjectGroup<ReactiveComponent, DataComponent> : ITagResponseInjectGroup<ReactiveComponent>
    where DataComponent : struct, IComponentData
    where ReactiveComponent : struct, IComponentData, ITag
    {
        ComponentDataArray<DataComponent> datas { get; }
    }

    public interface ITagResponseInjectGroup<RxComponent> 
    where RxComponent : struct, IComponentData, ITag
    {
        ComponentDataArray<RxComponent> ReactiveComponents { get; }
        EntityArray Entities { get; }
    }

    /// <summary>
    /// Use when an IComponentData is intended to be picked up by some system and immediately remove them without condition.
    /// </summary>
    public interface IReactive : ITag { }

/// <summary>
/// A E7ECS's reactive system will look for only one kind of reactive group, while that group could contains various reactions to perform.
/// </summary>
    public interface IReactiveGroup : ISharedComponentData { }

    /// <summary>
    /// Use when an `IComponentData` is to stick around and dictates behaviour, or use with `SubtractiveComponent` for example.
    /// Or use when a removal is optional unlike `IReactive`.
    /// </summary>

    //TODO : Make it a shared component data? So adding a tag/reactives is just a matter of be in a different chunk but still the same shape.
    public interface ITag : IComponentData { }
}