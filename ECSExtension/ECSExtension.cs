using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs;
using System.Collections.Generic;

public static class MonoECS 
{
    private static EntityManager me;
    private static EntityManager em
    {
        get
        {
            if (me == null)
            {
                me = World.Active.GetOrCreateManager<EntityManager>();
            }
            return me;
        }
    }

    /// <summary>
    /// A very inefficient and barbaric way of getting a filtered entities outside of ECS world.
    /// Useful for when you are in the middle of moving things to ECS. Runs on your active world's EntityManager.
    /// </summary>
    public static T[] Inject<T>() where T : struct, IComponentData
    {
        List<T> list = new List<T>();
        using (NativeArray<Entity> allEntities = em.GetAllEntities())
        {
            foreach (Entity e in allEntities)
            {
                if (em.HasComponent<T>(e))
                {
                    T componentData = em.GetComponentData<T>(e);
                    list.Add(componentData);
                }
            }
        }
        return list.ToArray();
    }

    /// <summary>
    /// A very inefficient and barbaric way of getting a filtered entities outside of ECS world.
    /// Useful for when you are in the middle of moving things to ECS. Runs on your active world's EntityManager.
    /// </summary>
    public static (T1,T2)[] Inject<T1,T2>() 
    where T1 : struct, IComponentData
    where T2 : struct, IComponentData
    {
        List<(T1,T2)> list = new List<(T1,T2)>();
        using (NativeArray<Entity> allEntities = em.GetAllEntities())
        {
            foreach (Entity e in allEntities)
            {
                if (em.HasComponent<T1>(e) && em.HasComponent<T2>(e))
                {
                    T1 componentData = em.GetComponentData<T1>(e);
                    T2 componentData2 = em.GetComponentData<T2>(e);
                    list.Add((componentData,componentData2));
                }
            }
        }
        return list.ToArray();
    }
    public static bool HasTag<T>(Entity entity) where T : struct, IComponentData
    {
        return em.HasComponent<T>(entity);
    }

    public static void AddTag<T>(Entity entity) where T : struct, IComponentData
    {
        if (em.HasComponent<T>(entity) == false)
        {
            em.AddComponentData<T>(entity, default(T));
        }
    }

    public static void RemoveTag<T>(Entity entity) where T : struct, IComponentData
    {
        if (em.HasComponent<T>(entity))
        {
            em.RemoveComponent<T>(entity);
        }
    }

    public static T GetComponentData<T>(Entity entity) where T : struct, IComponentData
    {
        return em.GetComponentData<T>(entity);
    }

    public static void SetComponentData<T>(Entity entity, T data) where T : struct, IComponentData
    {
        em.SetComponentData(entity, data);
    }
}