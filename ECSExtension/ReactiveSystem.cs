using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace E7.Entities
{
    public abstract class ReactiveCSBase<ReactiveComponent> : ComponentSystem
    where ReactiveComponent : struct, IComponentData, IReactive
    {
        /// <summary>
        /// Because ComponentSystem is on the main thread we have the class context and know which one is the reactive component, this can work.
        /// Or you could call base.OnUpdate() on your OnUpdate() to remove all reactive components captured in this update.
        /// </summary>
        // protected void EndReactive(int entityIndex) => PostUpdateCommands.EndReactive<ReactiveComponent>(ReactiveGroup.entities, entityIndex);

        /// <summary>
        /// You sure there's only one of them?
        /// </summary>
        //protected void EndReactive() => EndReactive(0);

        private protected abstract IReactiveInjectGroup<ReactiveComponent> ReactiveGroup { get; }

        protected abstract void OnReaction();
        protected override void OnUpdate()
        {
            OnReaction();
            for (int i = 0; i < ReactiveGroup.entities.Length; i++)
            {
                PostUpdateCommands.EndReactive<ReactiveComponent>(ReactiveGroup.entities[i]);
            }
        }
    }

    /// <summary>
    /// When you want to make a reactive system that removes that component at the end, this is a nice start.
    /// You can send the whole InjectGroup into the job with [ReadOnly]
    /// Use `InjectedGroup` to get the data.
    /// </summary>
    public abstract class ReactiveCS<ReactiveComponent> : ReactiveCSBase<ReactiveComponent>
    where ReactiveComponent : struct, IComponentData, IReactive
    {
        protected struct InjectGroup : IReactiveInjectGroup<ReactiveComponent>
        {
            public ComponentDataArray<ReactiveComponent> reactiveComponent { get; }
            public EntityArray entities { get; }
            public int Length;
        }
        [Inject] private protected InjectGroup injectedGroup;
        protected InjectGroup InjectedGroup => injectedGroup;

        private protected override IReactiveInjectGroup<ReactiveComponent> ReactiveGroup => injectedGroup;
    }

    /// <summary>
    /// Works with mono things that you have attached `GameObjectEntity` to.
    /// Use the `IReactive` as a way to simulate "method call" to that object.
    /// </summary>
    public abstract class ReactiveMonoCS<ReactiveComponent, MonoComponent> : ReactiveCSBase<ReactiveComponent>
    where ReactiveComponent : struct, IComponentData, IReactive
    where MonoComponent : Component
    {
        protected struct InjectGroup : IReactiveMonoInjectGroup<ReactiveComponent, MonoComponent>
        {
            public ComponentDataArray<ReactiveComponent> reactiveComponent { get; }
            public ComponentArray<MonoComponent> monoComponent { get; }
            public EntityArray entities { get; }
            public int Length;
        }
        [Inject] private protected InjectGroup injectedGroup;
        protected InjectGroup InjectedGroup => injectedGroup;

        private protected override IReactiveInjectGroup<ReactiveComponent> ReactiveGroup => injectedGroup;
    }

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
        [Inject] private protected InjectGroup injectedGroup;
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
        [Inject] private protected InjectGroup injectedGroup;
        protected InjectGroup InjectedGroup => injectedGroup;
    }
}