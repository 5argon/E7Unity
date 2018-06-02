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

        // /// <summary>
        // /// Destroys the entity, not just removing a component. Use with `Issue` because that creates a new entity.
        // /// Just use the IReactiveInjectGroup and it knows what to do.
        // /// </summary>
        // public static void EndReactive<T>(this EntityCommandBuffer ecb, IReactiveInjectGroup<T> injectGroup, int entityArrayIndex)
        // where T : struct, IComponentData, IReactive
        // {
        //     ecb.DestroyEntity(injectGroup.entities[entityArrayIndex]);
        // }
    }
}