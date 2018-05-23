using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs;

/// <summary>
/// When you want to make a reactive system that removes that component at the end, this is a nice start.
/// You can send the whole InjectGroup into the job with [ReadOnly]
/// Use `InjectedGroup` to get the data.
/// </summary>
public abstract class ReactiveJCS<ReactiveComponent> : JobComponentSystem
where ReactiveComponent : struct, IComponentData, IReactive
{
    protected struct InjectGroup : IReactiveInjectGroup<ReactiveComponent>
    {
        public ComponentDataArray<ReactiveComponent> reactiveComponent { get; }
        public EntityArray entities { get; }
        public int Length;
    }
    [Inject] protected InjectGroup injectedGroup;
    protected InjectGroup InjectedGroup => injectedGroup;
}

/// <summary>
/// When you want to make a reactive system with additional data on that entity.
/// Take the content out before sending them to the job so that `data` can be written to.
/// Use `InjectedGroup` to get the data.
/// </summary>
public abstract class ReactiveDataJCS<ReactiveComponent, DataComponent> : JobComponentSystem
where ReactiveComponent : struct, IComponentData, IReactive
where DataComponent : struct, IComponentData
{
    protected struct InjectGroup : IReactiveDataInjectGroup<ReactiveComponent, DataComponent>
    {
        public ComponentDataArray<ReactiveComponent> reactiveComponent { get; }
        public EntityArray entities { get; }
        public ComponentDataArray<DataComponent> data { get; }
        public int Length;
    }
    [Inject] protected InjectGroup injectedGroup;
    protected InjectGroup InjectedGroup => injectedGroup;
}