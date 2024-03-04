using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using UnityEngine;

[UpdateInGroup(typeof(InitializationSystemGroup))]
public partial class HertaLifetimeSystem : SystemBase
{
    // Make Entity Command Buffer
    private EndInitializationEntityCommandBufferSystem _endInitializationEntityCommandBufferSystem;
    protected override void OnCreate()
    {
        // Reference World Entity Command Buffer
        _endInitializationEntityCommandBufferSystem = World.GetOrCreateSystemManaged<EndInitializationEntityCommandBufferSystem>();
    }
    protected override void OnUpdate()
    {
        // Make Command Buffer as Parallel Writer (For Multi-Thread Job)
        var ecb = _endInitializationEntityCommandBufferSystem.CreateCommandBuffer().AsParallelWriter();
        var hertaEntities = ECS_HertaManager.HertaEntities;
        var DontDestroy = ECS_HertaManager.DontDestroyEntity;

        if (DontDestroy == true) return; // Return if dont destroy

        Entities.ForEach((Entity entity, int entityInQueryIndex, in HertaComponent hertaComponent) => {
            if (hertaComponent.lifeTime <= 0f) 
            {
                // Destroy Entity & remove from list if Lifetime <= 0 and DontDestroyEntity = false
                hertaEntities.Remove(entity);
                ecb.DestroyEntity(entityInQueryIndex, entity);
            }
        }).WithoutBurst().Run();
        _endInitializationEntityCommandBufferSystem.AddJobHandleForProducer(this.Dependency);
    }
}
