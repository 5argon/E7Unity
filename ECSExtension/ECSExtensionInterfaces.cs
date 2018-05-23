using Unity.Entities;

/// <summary>
/// The same as IReactiveInjectGroup but with additional data inside the entity that contains the reactive component.
/// It has effect on methods like commandBuffer.EndReactive where it will just remove the component, not destroying the whole entity.
/// </summary>
public interface IReactiveDataInjectGroup<ReactiveComponent, DataComponent> : IReactiveInjectGroup<ReactiveComponent> where DataComponent : struct, IComponentData where ReactiveComponent : struct, IComponentData, IReactive
{
    ComponentDataArray<DataComponent> data { get; }
}

/// <summary>
/// An inject struct that has a reactive component to be removed at the end of system function,
/// plus an injected array of entities so that we knows which one to remove a component from.
/// </summary>
public interface IReactiveInjectGroup<RxComponent> where RxComponent: struct, IComponentData, IReactive 
{
    ComponentDataArray<RxComponent> reactiveComponent { get; }
    EntityArray entities { get; }
}

/// <summary>
/// Use when an IComponentData is to be picked up by some system and optinally immediately remove it at the end.
/// </summary>
public interface IReactive{}

/// <summary>
/// Use when an IComponentData is to stick around and dictates behaviour, or use with `SubtractiveComponent` for example.
/// </summary>
public interface ITag{}