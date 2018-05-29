# E7ECS

Lightweight reactive systems and cool stuffs built **on top** of Unity's regular ECS. Centered on the idea of using **generic class parameter** and **overrides** instead of verbose inject declaration for your data requirement, while still not abstracting Unity ECS's concept too much.

Use `using E7.ECS;` to access them. **Requirement** : C#7.2 (Use Unity Incremental Compiler and you will have it)

## Reactive Systems

Contains new type of systems you could subclass from. The idea is using the generic parameter to influence the injected group inside them. (+ some additional helper methods to help you reacts to events)

**Reactive system definition** : All systems are based on the concept of creating an empty entity with "reactive component" (`IReactive`) to the world (this entity is "reactive entity"), just so that some system that targets them could pick them up and take action. All of the picked up reactive entities will be destroyed automatically.

A reactive component can optionally also contains data inside them to function like method arguments. All reactive entity must belong to an `IReactiveGroup`. A system will target the whole `IReactiveGroup`, not just one or some  `IReactive` reactions.

The act of creating a reactive entity is called `Issue`. You could do this in a job or in any `MonoBehaviour` script to release a reactive entity to the world waiting for some system to pick up.

### ReactiveCS\<ReactiveGroup>

A `ComponentSystem` which reacts to `ReactiveGroup` specified.
Override `OnReaction`, this method will run repeatedly equal to a number of reactions received in this frame.

This system use an `out` pattern in `OnReaction` : `if(ReactsTo(out YourIReactiveType rxComponent) { ... }` to take action based on a specific reactive type. Chain it with `else if` until you handle all of the possible types.

### ReactiveMonoCS\<ReactiveGroup, MonoComponent>

The same as `ReactiveCS` but additionally gives you a `MonoBehaviour` with `GameObjectEntity` attached.

Because `ComponentGroup` is always in the main thread, we can bridge back to mono world with this perfectly and do anything with them. (With reactive action that might came from ECS-world)

If you are confident there is only one such `MonoComponent` in your scene, use `FirstMono` to immediately access it.

### ReactiveJCS\<ReactiveGroup>

A system with different approach from `ReactiveCS` and `ReactiveMonoCS`. There are 2 more problems :
- In a `JobComponentSystem` we often want to prepare all the reactive components together and take them to the job, not reacting simply one by one like in the `-CS` systems.
- We could not destroy those captured reactive entities in the job without a command buffer. A command buffer requires a barrier which is not wise to include a barrier class in the library as it prohibits you from adding update order to the barrier.

Therefore this system has 3 changes in usage : 
1. Override `protected abstract ComponentType[] ReactsTo { get; }` and give it an array of all **type of** `IReactive` (not group, the group is in the class's declaration) that you would like to take action. It will create a separated inject group for each of them. Use `ComponenType.Create()` or `ComponentType.ReadOnly()` and form an array of types.
2. Override `protected abstract EntityCommandBuffer DestroyReactivesBuffer { get; }` and give it your own `barrier.CreateCommandBuffer()`. It will use the command buffer to destroy all reactive entities for you automatically.
3. In `OnReaction()` use `GetReactions<T>()` where `T` is one of the type in `ReactsTo`. You will get a `ComponentDataArray<T>` filled with reactive components of that type which you could take it to the job.

## Tag Response System

A simpler kind of reactive system. A "tag" is a struct with `ITag`. 

Unlike `IReactive`, `ITag` is meant to be attached to an existing entity and then that entity can be captured in tag response system. The entity is not automatically destroyed after the system, making it capture the entity repeatedly if you don't do something to it.

A tag does not belongs to any group. A tag response system directly specify which tag to response to in the generic.

These tag response is essentially a little template so you don't have to write the verbose `[Inject]` struct but instead construct the struct with generics from the class declaration. The super class can inherits the injected struct if it is not `private` (in this case, `private protected` is fine)

### TagResponseJCS\<TagComponent>

Captures all entities with `TagComponent`.

### TagResponseDataJCS\<TagComponent, DataComponent>

Captures all entities with `TagComponent` and also one `IComponentData` of your choice that is also in those entities.

## Interfaces

Contains `IReactive` and `ITag` you must use for your `IComponentData` to be able to work with the rest of the library. You don't have to implement `IComponentData` if you already chosen either of them.

## MonoECS

Many functions to interface easily with ECS world from `MonoBehaviour` world. One of which is `MonoECS.Issue` that you could use to trigger a reactive system from anywhere. Also some methods to add an `ITag`. It has some safeguards so that a duplicate tag additions will not results in an error.

## Entity Command Buffer Extensions

Added `Issue` command to the ECB so you could trigger more reactive systems after the barrier activates. Also some command to manually end a tag response system by removing a tag in command buffer.

## Math Extensions

Contains some missing functions I would like to use in a job, which are implemented with `Unity.Mathematics`.