using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

public struct HertaComponent : IComponentData
{
    public float radius, speed, lifeTime;
    public float2 direction;

    public Matrix4x4 matrix;
    public float4x4 matrixf;
}
