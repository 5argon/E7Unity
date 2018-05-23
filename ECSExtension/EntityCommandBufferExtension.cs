using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs;

public static class EntityCommandBufferExtension
{
    public static void Issue<T>(this EntityCommandBuffer ecb, T component)
    where T : struct, IComponentData, Reactive
    {
        ecb.AddComponent<T>(component);
    }

    public static void EndReactive<T>(this EntityCommandBuffer ecb, IReactiveInjectGroup<T> injectGroup, int entityArrayIndex)
    where T : struct, IComponentData, Reactive
    {
        ecb.RemoveComponent<T>(injectGroup.entities[entityArrayIndex]);
    }
}
