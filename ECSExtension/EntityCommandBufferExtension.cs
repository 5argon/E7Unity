using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs;

namespace E7.Entities
{
    public static class EntityCommandBufferExtension
    {
        /// <summary>
        /// Make a new entity just for carrying the reactive component.
        /// A system like `ReactiveCS` or `ReactiveMonoCS` can pick it up,
        /// take action, and destroy them afterwards automatically.
        /// </summary>
        public static void Issue<T>(this EntityCommandBuffer ecb, T component)
        where T : struct, IComponentData, IReactive
        {
            ecb.CreateEntity();
            ecb.AddComponent<T>(component);
        }

        public static void AddTag<T>(this EntityCommandBuffer ecb,Entity addToEntity)
        where T : struct, IComponentData, ITag
        {
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