using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using UnityEngine;
using Unity.Entities;

[BurstCompile][UpdateBefore(typeof(SpriteSheetRenderer))]
public partial class SpriteSheetAnimator : SystemBase
{
    [BurstCompile]
    public partial struct AnimationJob : IJobEntity
    {
        public float deltaTime;
        private float UV_width, UV_height, UV_offsetX, UV_offsetY;

        public void Execute(ref SpriteSheetComponent spriteSheetComponent)
        {
            spriteSheetComponent.frameTimer += deltaTime;
            spriteSheetComponent.frameTimerMax = 1f / spriteSheetComponent.framesPerSecond;

            while (spriteSheetComponent.frameTimer >= spriteSheetComponent.frameTimerMax) {
                spriteSheetComponent.frameTimer -= spriteSheetComponent.frameTimerMax;
                spriteSheetComponent.currentFrame = (spriteSheetComponent.currentFrame + 1) % spriteSheetComponent.frameCount;
            }

            UV_width    = 1f / spriteSheetComponent.frameCount;
            UV_height   = 1f;
            UV_offsetX  = UV_width * spriteSheetComponent.currentFrame;
            UV_offsetY  = 0f;

            spriteSheetComponent.UV = new Vector4(UV_width, UV_height, UV_offsetX, UV_offsetY);
            spriteSheetComponent.UVf = spriteSheetComponent.UV;
        }
    }

    private AnimationJob animationJob;

    [BurstCompile]
    protected override void OnUpdate()
    {
        var m_deltaTime = SystemAPI.Time.DeltaTime;

        animationJob = new AnimationJob { deltaTime = m_deltaTime };
        animationJob.ScheduleParallel();
    }
}
