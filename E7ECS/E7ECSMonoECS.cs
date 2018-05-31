using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs;
using System.Collections.Generic;
using UnityEngine;

namespace E7.ECS
{
    /// <summary>
    /// A slew of cheat functions to help you bridge `MonoBehaviour` world with ECS.
    /// Also useful as a band-aid solution while moving to ECS.
    /// </summary>
    public static class MonoECS
    {
        private static EntityManager cachedEntityManager;

        /// <summary>
        /// If you change the manager or world this will be broken...
        /// </summary>
        private static EntityManager em
        {
            get
            {
                if (cachedEntityManager == null)
                {
                    cachedEntityManager = World.Active.GetOrCreateManager<EntityManager>();
                }
                return cachedEntityManager;
            }
        }

        /// <summary>
        /// A very inefficient and barbaric way of getting a filtered entities outside of ECS world.
        /// Useful for when you are in the middle of moving things to ECS. Runs on your active world's EntityManager.
        /// </summary>
        public static (T component, Entity entity)[] Inject<T>() where T : struct, IComponentData
        {
            List<(T, Entity)> list = new List<(T, Entity)>();
            using (NativeArray<Entity> allEntities = em.GetAllEntities())
            {
                foreach (Entity e in allEntities)
                {
                    if (em.HasComponent<T>(e))
                    {
                        T componentData = em.GetComponentData<T>(e);
                        list.Add((componentData, e));
                    }
                }
            }
            return list.ToArray();
        }

        /// <summary>
        /// A very inefficient and barbaric way of getting a filtered entities outside of ECS world.
        /// Useful for when you are in the middle of moving things to ECS. Runs on your active world's EntityManager.
        /// </summary>
        public static (T1 component1, T2 component2, Entity entity)[] Inject<T1, T2>()
        where T1 : struct, IComponentData
        where T2 : struct, IComponentData
        {
            List<(T1, T2, Entity)> list = new List<(T1, T2, Entity)>();
            using (NativeArray<Entity> allEntities = em.GetAllEntities())
            {
                foreach (Entity e in allEntities)
                {
                    if (em.HasComponent<T1>(e) && em.HasComponent<T2>(e))
                    {
                        T1 componentData = em.GetComponentData<T1>(e);
                        T2 componentData2 = em.GetComponentData<T2>(e);
                        list.Add((componentData, componentData2, e));
                    }
                }
            }
            return list.ToArray();
        }

        /// <summary>
        /// Get all of your `MonoBehaviour` attached with `GameObjectEntity` outside of ECS.
        /// </summary>
        /// <returns>Contains both the component and its corresponding entity generated from `GameObjectEntity`.</returns>
        public static (T monoComponent, Entity entity)[] InjectMono<T>() where T : Component
        {
            List<(T, Entity)> list = new List<(T, Entity)>();
            using (NativeArray<Entity> allEntities = em.GetAllEntities())
            {
                foreach (Entity e in allEntities)
                {
                    if (em.HasComponent<T>(e))
                    {
                        T componentData = em.GetComponentObject<T>(e);
                        list.Add((componentData, e));
                    }
                }
            }
            return list.ToArray();
        }


        public static bool HasTag<T>(Entity entity) where T : struct, IComponentData, ITag
        {
            return em.HasComponent<T>(entity);
        }

        /// <summary>
        /// Adds a tag component if not already there, the content of component is its empty default because we assume it is just a "tag" anyways.
        /// </summary>
        public static void AddTag<T>(Entity entity) where T : struct, IComponentData, ITag => AddTag<T>(entity, default(T));

        /// <summary>
        /// Adds a tag component if not already there. 
        /// </summary>
        public static void AddTag<T>(Entity entity, T tagContent) where T : struct, IComponentData, ITag
        {
            if (em.HasComponent<T>(entity) == false)
            {
                em.AddComponentData<T>(entity, tagContent);
            }
        }

        /// <summary>
        /// Removes a tag component if it is there.
        /// </summary>
        public static void RemoveTag<T>(Entity entity) where T : struct, IComponentData, ITag
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
        public static void Issue<ReactiveComponent,ReactiveGroup>()
        where ReactiveComponent : struct, IReactive
        where ReactiveGroup : struct, IReactiveGroup
        => Issue<ReactiveComponent, ReactiveGroup>(default);

        /// <summary>
        /// Make a new entity just for carrying the reactive component.
        /// A system like `ReactiveCS`, `ReactiveMonoCS`, or `ReactiveJCS` can pick it up,
        /// take action, and destroy them afterwards automatically.
        /// </summary>
        /// <typeparam name="ReactiveComponent">An `IReactive` which you can check in the receiving system what action to take.</typeparam>
        /// <typeparam name="ReactiveGroup">An `IReactiveGroup` that determines which system could pick up the reaction.</typeparam>
        public static void Issue<ReactiveComponent, ReactiveGroup>(ReactiveComponent rx)
        where ReactiveComponent: struct, IReactive
        where ReactiveGroup : struct, IReactiveGroup
        {
            //Debug.Log($"Issuing command {typeof(ReactiveComponent).Name}");
            var e = em.CreateEntity();
            em.AddComponentData<ReactiveComponent>(e, rx);
            em.AddSharedComponentData<ReactiveGroup>(e, default);
            //TODO : Create an archetype that has this because we always need this...
            em.AddSharedComponentData<DestroyReactivesSystem.ReactiveEntity>(e, default);
        }

        /// <summary>
        /// Runs on the currently active world and EntityManager.
        /// </summary>
        public static T GetComponentData<T>(Entity entity) where T : struct, IComponentData
        {
            return em.GetComponentData<T>(entity);
        }

        /// <summary>
        /// Runs on the currently active world and EntityManager.
        /// </summary>
        public static void SetComponentData<T>(Entity entity, T data) where T : struct, IComponentData
        {
            em.SetComponentData(entity, data);
        }
    }
}