using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs;
using System.Collections.Generic;
using UnityEngine;

namespace E7.ECS
{
    public static class ActiveWorld
    {
        public static void Issue<ReactiveComponent,ReactiveGroup>()
        where ReactiveComponent : struct, IReactive
        where ReactiveGroup : struct, IReactiveGroup
        => World.Active.GetExistingManager<EntityManager>().Issue<ReactiveComponent, ReactiveGroup>();
    }
}