using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs;

public static class EntityCommandBufferExtension
{
    /// <summary>
    /// Use the system with IReactiveInjectGroup to pick it up and act.
    /// </summary>
    public static void Issue<T>(this EntityCommandBuffer ecb, T component)
    where T : struct, IComponentData, IReactive
    {
        ecb.AddComponent<T>(component);
    }

    /// <summary>
    /// Takes the whole IReactiveInjectGroup and it will remove the reactive component in that group for you at the index that you are currently iterating.
    /// </summary>
    public static void EndReactive<ReactiveComponent>(this EntityCommandBuffer ecb, EntityArray entityArray, int entityArrayIndex)
    where ReactiveComponent : struct, IComponentData, IReactive
    {
        ecb.RemoveComponent<ReactiveComponent>(entityArray[entityArrayIndex]);
    }

    /// <summary>
    /// Destroys the entity, not just removing a component. Use with `Issue` because that creates a new entity.
    /// </summary>
    public static void EndReactive<T>(this EntityCommandBuffer ecb, IReactiveInjectGroup<T> injectGroup, int entityArrayIndex)
    where T : struct, IComponentData, IReactive
    {
        ecb.DestroyEntity(injectGroup.entities[entityArrayIndex]);
    }
}
