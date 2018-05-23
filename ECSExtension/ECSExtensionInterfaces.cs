using Unity.Entities;

public interface IReactiveInjectGroup<ReactiveComponent> where ReactiveComponent : struct, IComponentData
{
    ComponentDataArray<ReactiveComponent> reactiveComponent { get; }
    EntityArray entities { get; }
}

public interface Reactive{}

public interface Tag{}