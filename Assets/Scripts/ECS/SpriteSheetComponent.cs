using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public struct SpriteSheetComponent : IComponentData
{
    public int frameCount; // Spritesheet frame count
    public int framesPerSecond; // frames per second

    public int currentFrame; // current frame rendering
    public float frameTimer; // time elapsed in seconds
    public float frameTimerMax; // time elapsed per one frame

    public Vector4 UV;
    public float4 UVf;
}
