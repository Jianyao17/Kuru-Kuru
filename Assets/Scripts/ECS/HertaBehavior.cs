using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[BurstCompile][UpdateBefore(typeof(SpriteSheetRenderer))]
public partial class HertaBehavior : SystemBase
{
    [BurstCompile]
    public partial struct HertaBehaviorJob : IJobEntity
    {
        public float deltaTime, nearClipPlane;
        public Bounds moveArea;

        void Execute(ref HertaComponent hertaComponent, ref LocalTransform transform)
        {
            // Calculate move area based on screen boundaries
            float2  minPos = new float2(moveArea.min.x, moveArea.min.y) + hertaComponent.radius,
                    maxPos = new float2(moveArea.max.x, moveArea.max.y) - hertaComponent.radius;

            // Inverse movement if touching boundaries
            if (transform.Position.x <= minPos.x || transform.Position.x >= maxPos.x)
            {
                hertaComponent.direction.x *= -1;
            }
            if (transform.Position.y <= minPos.y || transform.Position.y >= maxPos.y)
            {
                hertaComponent.direction.y *= -1;
            }

            // Move herta
            transform.Position += new float3(hertaComponent.direction.x, hertaComponent.direction.y, 0)
                                            * hertaComponent.speed * deltaTime;
            hertaComponent.matrix =
                Matrix4x4.TRS(transform.Position, Quaternion.identity, Vector3.one * hertaComponent.radius * 2f);

            hertaComponent.matrixf =
                float4x4.TRS(transform.Position, Quaternion.identity, Vector3.one * hertaComponent.radius * 2f);

            // reduce lifetime each second
            hertaComponent.lifeTime -= deltaTime;
        }
    }

    private HertaBehaviorJob hertaBehaviorJob;

    [BurstCompile]
    protected override void OnUpdate()
    {
        var m_deltaTime = SystemAPI.Time.DeltaTime;
        var m_nearClipPlane = Camera.main.nearClipPlane;
        var m_moveArea = ECS_HertaManager.MoveArea;

        hertaBehaviorJob = new HertaBehaviorJob
        {
            deltaTime = m_deltaTime,
            nearClipPlane = m_nearClipPlane,
            moveArea = m_moveArea
        };

        hertaBehaviorJob.ScheduleParallel();
    }
}
