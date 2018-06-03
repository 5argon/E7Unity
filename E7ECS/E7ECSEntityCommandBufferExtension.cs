using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace E7.ECS
{
    public static class EntityManagerExtension 
    {
        /// <summary>
        /// If you just loop and destroy each one in EntityArray without a command buffer, it will cause problems mid-loop!
        /// </summary>
        public static void DestroyAllInEntityArray(this EntityManager em, EntityArray ea)
        {
            var na = new NativeArray<Entity>(ea.Length, Allocator.Temp);
            ea.CopyTo(na,0);
            for (int i = 0; i < na.Length; i++)
            {
                em.DestroyEntity(na[i]);
            }
            na.Dispose();
        }

        /// <summary>
        /// Make a new entity just for carrying the reactive component.
        /// A system like `ReactiveCS`, `ReactiveMonoCS`, or `ReactiveJCS` can pick it up,
        /// take action, and destroy them afterwards automatically.
        /// </summary>
        /// <typeparam name="ReactiveComponent">An `IReactive` which you can check in the receiving system what action to take.</typeparam>
        /// <typeparam name="ReactiveGroup">An `IReactiveGroup` that determines which system could pick up the reaction.</typeparam>
        public static void Issue<ReactiveComponent, ReactiveGroup>(this EntityManager ecb)
        where ReactiveComponent : struct, IReactive
        where ReactiveGroup : struct, IReactiveGroup
        => Issue<ReactiveComponent, ReactiveGroup>(ecb, default, default);

        /// <summary>
        /// Make a new entity just for carrying the reactive component.
        /// A system like `ReactiveCS`, `ReactiveMonoCS`, or `ReactiveJCS` can pick it up,
        /// take action, and destroy them afterwards automatically.
        /// </summary>
        /// <typeparam name="ReactiveComponent">An `IReactive` which you can check in the receiving system what action to take.</typeparam>
        /// <typeparam name="ReactiveGroup">An `IReactiveGroup` that determines which system could pick up the reaction.</typeparam>
        public static void Issue<ReactiveComponent, ReactiveGroup>(this EntityManager ecb, ReactiveComponent rx)
        where ReactiveComponent : struct, IReactive
        where ReactiveGroup : struct, IReactiveGroup
        => Issue<ReactiveComponent, ReactiveGroup>(ecb, rx, default);

        /// <summary>
        /// Make a new entity just for carrying the reactive component.
        /// A system like `ReactiveCS`, `ReactiveMonoCS`, or `ReactiveJCS` can pick it up,
        /// take action, and destroy them afterwards automatically.
        /// </summary>
        /// <typeparam name="ReactiveComponent">An `IReactive` which you can check in the receiving system what action to take.</typeparam>
        /// <typeparam name="ReactiveGroup">An `IReactiveGroup` that determines which system could pick up the reaction.</typeparam>
        public static void Issue<ReactiveComponent, ReactiveGroup>(this EntityManager ecb, ReactiveComponent rx, ReactiveGroup rg)
        where ReactiveComponent : struct, IReactive
        where ReactiveGroup : struct, IReactiveGroup
        {
            //Debug.Log($"Issuing {typeof(ReactiveComponent).Name} (ECB)");
            var e = ecb.CreateEntity();
            ecb.AddComponentData<ReactiveComponent>(e, rx);
            ecb.AddSharedComponentData<ReactiveGroup>(e, rg);
            //TODO : Create an archetype that has this because we always need this...
            ecb.AddSharedComponentData<DestroyReactivesSystem.ReactiveEntity>(e, default);
        }
    }

    public static class EntityCommandBufferExtension
    {
        public static void Issue<ReactiveComponent, ReactiveGroup>(this EntityCommandBuffer ecb)
        where ReactiveComponent : struct, IReactive
        where ReactiveGroup : struct, IReactiveGroup
        => Issue<ReactiveComponent, ReactiveGroup>(ecb, default, default);

        public static void Issue<ReactiveComponent, ReactiveGroup>(this EntityCommandBuffer ecb, ReactiveComponent rx)
        where ReactiveComponent : struct, IReactive
        where ReactiveGroup : struct, IReactiveGroup
        => Issue<ReactiveComponent, ReactiveGroup>(ecb, rx, default);

        public static void Issue<ReactiveComponent, ReactiveGroup>(this EntityCommandBuffer ecb, ReactiveComponent rx, ReactiveGroup rg)
        where ReactiveComponent : struct, IReactive
        where ReactiveGroup : struct, IReactiveGroup
        {
            //Debug.Log($"Issuing {typeof(ReactiveComponent).Name} (ECB)");
            ecb.CreateEntity();
            ecb.AddComponent<ReactiveComponent>(rx);
            ecb.AddSharedComponent<ReactiveGroup>(rg);
            //TODO : Create an archetype that has this because we always need this...
            ecb.AddSharedComponent<DestroyReactivesSystem.ReactiveEntity>(default);
        }

        /// <summary>
        /// Be careful not to add duplicate tags since command buffer
        /// cound not use `HasTag`!
        /// </summary>
        public static void AddTag<T>(this EntityCommandBuffer ecb)
        where T : struct, IComponentData, ITag
        {
            ecb.AddComponent<T>(default);
        }

        /// <summary>
        /// Be careful not to add duplicate tags since command buffer
        /// cound not use `HasTag`!
        /// </summary>
        public static void AddTag<T>(this EntityCommandBuffer ecb,Entity addToEntity)
        where T : struct, IComponentData, ITag
        {
            //Debug.Log($"Adding tag " + typeof(T).Name);
            ecb.AddComponent<T>(addToEntity, default);
        }

        /// <summary>
        /// End a tag response routine by removing a component from an entity. You must specify a reactive component type manually.
        /// </summary>
        public static void EndTagResponse<ReactiveComponent>(this EntityCommandBuffer ecb, EntityArray entityArray, int entityArrayIndex)
        where ReactiveComponent : struct, IComponentData, ITag
        {
            ecb.RemoveComponent<ReactiveComponent>(entityArray[entityArrayIndex]);
        }

    }
}