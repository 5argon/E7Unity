using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using System.Collections.Generic;

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

        public static void UpsertComponent<T>(this EntityManager em, Entity entity) where T : struct, IComponentData => UpsertComponent<T>(em, entity, default(T));

        public static void UpsertComponent<T>(this EntityManager em, Entity entity, T tagContent) where T : struct, IComponentData
        {
            if (em.HasComponent<T>(entity) == false)
            {
                em.AddComponentData<T>(entity, tagContent);
            }
            else
            {
                em.SetComponentData<T>(entity, tagContent);
            }
        }

        public static bool TryGetComponent<T>(this EntityManager em, Entity entity, out T componentData) where T : struct, IComponentData
        {
            if(em.HasComponent<T>(entity))
            {
                componentData = em.GetComponentData<T>(entity);
                return true;
            }
            else
            {
                componentData = default;
                return false;
            }
        }

        /// <summary>
        /// Removes a tag component if it is there.
        /// </summary>
        public static void RemoveTag<T>(this EntityManager em, Entity entity) where T : struct, IComponentData, ITag
        {
            if (em.HasComponent<T>(entity))
            {
                em.RemoveComponent<T>(entity);
            }
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

        public static void Issue<ReactiveComponent, ReactiveGroup>(this EntityCommandBuffer.Concurrent ecb)
        where ReactiveComponent : struct, IReactive
        where ReactiveGroup : struct, IReactiveGroup
        => Issue<ReactiveComponent, ReactiveGroup>(ecb, default, default);

        public static void Issue<ReactiveComponent, ReactiveGroup>(this EntityCommandBuffer.Concurrent ecb, ReactiveComponent rx)
        where ReactiveComponent : struct, IReactive
        where ReactiveGroup : struct, IReactiveGroup
        => Issue<ReactiveComponent, ReactiveGroup>(ecb, rx, default);

        public static void Issue<ReactiveComponent, ReactiveGroup>(this EntityCommandBuffer.Concurrent ecb, ReactiveComponent rx, ReactiveGroup rg)
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
        /// No upsert check!
        /// Be careful not to add duplicate tags!
        /// </summary>
        public static void AddTag<T>(this EntityCommandBuffer ecb, Entity addToEntity)
        where T : struct, IComponentData, ITag
        => AddTag<T>(ecb, addToEntity, default(T));

        public static void AddTag<T>(this EntityCommandBuffer ecb, Entity addToEntity, T data)
        where T : struct, IComponentData, ITag
        {
            ecb.AddComponent<T>(addToEntity, data);
        }

        public static void AddTag<T>(this EntityCommandBuffer.Concurrent ecb, Entity addToEntity)
        where T : struct, IComponentData, ITag
        => AddTag<T>(ecb, addToEntity, default(T));

        public static void AddTag<T>(this EntityCommandBuffer.Concurrent ecb, Entity addToEntity, T data)
        where T : struct, IComponentData, ITag
        {
            ecb.AddComponent<T>(addToEntity, data);
        }

        /// <summary>
        /// Determine whether it is an Add or Set command based on if it currently has a component at the time of calling this or not.
        /// </summary>
        public static void AddTag<T>(this EntityCommandBuffer ecb, Entity addToEntity, EntityManager em)
        where T : struct, IComponentData, ITag
        => AddTag<T>(ecb, addToEntity, default, em);

        /// <summary>
        /// Determine whether it is an Add or Set command based on if it currently has a component at the time of calling this or not.
        /// </summary>
        public static void AddTag<T>(this EntityCommandBuffer ecb, Entity addToEntity, T data, EntityManager em)
        where T : struct, IComponentData, ITag
        {
            //Debug.Log($"Adding tag " + typeof(T).Name);
            if (em.HasComponent<T>(addToEntity) == false)
            {
                //Debug.Log($"Choose to add {addToEntity.Index}");
                ecb.AddComponent<T>(addToEntity, data);
            }
            else
            {
                //Debug.Log($"Choose to set! {addToEntity.Index}");
                ecb.SetComponent<T>(addToEntity, data);
            }
        }

        /// <summary>
        /// An overload suitable to use with system with EntityManager.
        /// Contains HasComponent check.
        /// </summary>
        public static void RemoveTag<ReactiveComponent>(this EntityCommandBuffer ecb, Entity e, EntityManager em)
        where ReactiveComponent : struct, IComponentData, ITag
        {
            if (em.HasComponent<ReactiveComponent>(e))
            {
                RemoveTag<ReactiveComponent>(ecb, e);
            }
        }


        /// <summary>
        /// End a tag response routine by removing a component from an entity. You must specify a reactive component type manually.
        /// </summary>
        public static void RemoveTag<ReactiveComponent>(this EntityCommandBuffer ecb, Entity e)
        where ReactiveComponent : struct, IComponentData, ITag
        {
            ecb.RemoveComponent<ReactiveComponent>(e);
        }
    }

    public static class ComponentDataArrayExtension
    {
        public static List<T> CopyToList<T>(this ComponentDataArray<T> cda) where T : struct, IComponentData
        {
            using (var na = new NativeArray<T>(cda.Length, Allocator.Temp))
            {
                cda.CopyTo(na);
                List<T> list = new List<T>(na);
                return list;
            }
        }
    }
}