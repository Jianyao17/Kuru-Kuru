using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

//[UpdateAfter(typeof(SpriteSheetAnimator))]
public partial class SpriteSheetRenderer : SystemBase
{
    public struct MeshProperties
    {
        public Matrix4x4 mat;
        public float4 sprite_UV;

        public static int Size() {
            return sizeof(float) * 4 * 4
                 + sizeof(float) * 4;
        }

    }

    private int entityCount;
    private Camera mainCam;
    private Mesh hertaMesh;
    private Material hertaMaterial;

    private NativeArray<Matrix4x4> Matrix_List;
    private NativeArray<Vector4> UV_List;

    private ComputeBuffer meshPropertiesBuffer;
    private ComputeBuffer argsBuffer;
    private RenderParams renderParams;

    private GraphicsBuffer commandBuf;
    private GraphicsBuffer.IndirectDrawIndexedArgs[] commandData;
    private int commandCount = 1;

    protected override void OnStartRunning()
    {
        mainCam = Camera.main;
        hertaMesh = ECS_HertaManager.HertaMesh;
        hertaMaterial = ECS_HertaManager.HertaMaterial;

        renderParams = new RenderParams(hertaMaterial);
        renderParams.worldBounds = new Bounds(Vector3.zero, Vector3.one * 100);

        commandBuf = new GraphicsBuffer(GraphicsBuffer.Target.IndirectArguments, commandCount, GraphicsBuffer.IndirectDrawIndexedArgs.size);
        commandData = new GraphicsBuffer.IndirectDrawIndexedArgs[commandCount];
    }
    protected override void OnUpdate()
    {
        //RenderMeshIndirect();
        RenderMeshInstanced();
        //DrawMeshInstanced();
        //DrawMeshInstancedIndirect();
    }

    private void RenderMeshIndirect()
    {
        EntityQuery entityQuery = GetEntityQuery(typeof(HertaComponent));
        entityCount = entityQuery.CalculateEntityCount(); // get herta entity count

        if (entityCount <= 0) return;

        var meshPropertiesArray = new NativeArray<MeshProperties>(entityCount, Allocator.TempJob);
        MeshPropertiesJob meshPropertiesJob = new MeshPropertiesJob()
        {
            meshProperties = meshPropertiesArray
        };

        var jobHandle = meshPropertiesJob.ScheduleParallel(this.Dependency);
        jobHandle.Complete();

        meshPropertiesBuffer = new ComputeBuffer(entityCount, MeshProperties.Size());
        meshPropertiesBuffer.SetData(meshPropertiesArray);
        renderParams.matProps = new MaterialPropertyBlock();
        renderParams.matProps.SetBuffer(Shader.PropertyToID("_Properties"), meshPropertiesBuffer);

        commandData[0].indexCountPerInstance = hertaMesh.GetIndexCount(0);
        commandData[0].instanceCount = (uint) entityCount;
        commandBuf.SetData(commandData);

        Graphics.RenderMeshIndirect(renderParams, hertaMesh, commandBuf, commandCount);

        meshPropertiesArray.Dispose();
    }

    private void RenderMeshInstanced()
    {
        EntityQuery entityQuery = GetEntityQuery(typeof(HertaComponent));
        entityCount = entityQuery.CalculateEntityCount(); // get herta entity count

        if (entityCount <= 0) return;

        Matrix_List = new NativeArray<Matrix4x4>(entityCount, Allocator.TempJob);
        UV_List = new NativeArray<Vector4>(entityCount, Allocator.TempJob);

        GetEntityDataJob getEntityDataJob = new GetEntityDataJob()
        {
            matrixList = Matrix_List,
            UVList = UV_List
        };

        // Complete job immediately
        JobHandle jobHandle = getEntityDataJob.ScheduleParallel(this.Dependency);
        jobHandle.Complete();

        renderParams.matProps = new MaterialPropertyBlock();
        renderParams.matProps.SetVectorArray("_MainTex_UV", UV_List.ToArray());
        // Render All the Mesh
        Graphics.RenderMeshInstanced(renderParams, hertaMesh, 0, Matrix_List);

        Matrix_List.Dispose();
        UV_List.Dispose();
    }

    private void DrawMeshInstanced()
    {
        MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
        EntityQuery entityQuery = GetEntityQuery(typeof(SpriteSheetComponent), typeof(HertaComponent));
        entityCount = entityQuery.CalculateEntityCount(); // get herta entity count

        Matrix_List = new NativeArray<Matrix4x4>(entityCount, Allocator.TempJob);
        UV_List = new NativeArray<Vector4>(entityCount, Allocator.TempJob);

        GetEntityDataJob getEntityDataJob = new GetEntityDataJob()
        {
            matrixList = Matrix_List,
            UVList = UV_List
        };

        // Complete job immediately
        JobHandle jobHandle = getEntityDataJob.ScheduleParallel(this.Dependency);
        jobHandle.Complete();

        //Render all the mesh when render target is exist
        if (entityCount > 0)
        {
            materialPropertyBlock.SetVectorArray("_MainTex_UV", UV_List.ToArray());
            Graphics.DrawMeshInstanced(hertaMesh, 0, hertaMaterial, Matrix_List.ToArray(), entityCount, materialPropertyBlock);
        }

        Matrix_List.Dispose();
        UV_List.Dispose();
    }

    private void DrawMeshInstancedIndirect()
    {
        EntityQuery entityQuery = GetEntityQuery(typeof(HertaComponent));
        entityCount = entityQuery.CalculateEntityCount(); // get herta entity count

        if (entityCount <= 0) return;
        
        UpdateBuffers();

        var bounds = new Bounds(new Vector3(0, 0, mainCam.nearClipPlane), Vector3.one * 1000);
        Graphics.DrawMeshInstancedIndirect(hertaMesh, 0, hertaMaterial, bounds, argsBuffer);
    }

    private void UpdateBuffers()
    {
        // Argument buffer used by DrawMeshInstancedIndirect.
        uint[] args = new uint[5] { 0, 0, 0, 0, 0 };
        // Arguments for drawing mesh.
        // 0 == number of triangle indices, 1 == population, others are only relevant if drawing submeshes.
        args[0] = (uint)hertaMesh.GetIndexCount(0);
        args[1] = (uint)entityCount;
        args[2] = (uint)hertaMesh.GetIndexStart(0);
        args[3] = (uint)hertaMesh.GetBaseVertex(0);
        argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        argsBuffer.SetData(args);

        var meshPropertiesArray = new NativeArray<MeshProperties>(entityCount, Allocator.TempJob);
        MeshPropertiesJob meshPropertiesJob = new MeshPropertiesJob()
        {
            meshProperties = meshPropertiesArray
        };

        var jobHandle = meshPropertiesJob.ScheduleParallel(this.Dependency);
        jobHandle.Complete();

        meshPropertiesBuffer = new ComputeBuffer(entityCount, MeshProperties.Size());
        meshPropertiesBuffer.SetData(meshPropertiesArray);
        hertaMaterial.SetBuffer("_Properties", meshPropertiesBuffer);

        meshPropertiesArray.Dispose();
    }

    protected override void OnStopRunning()
    {
        argsBuffer?.Release();
        argsBuffer = null;

        meshPropertiesBuffer?.Release();
        meshPropertiesBuffer = null;

        commandBuf?.Release();
        commandBuf = null;
    }

    [BurstCompile]
    public partial struct GetEntityDataJob : IJobEntity
    {
        [NativeDisableParallelForRestriction] public NativeArray<Matrix4x4> matrixList;
        [NativeDisableParallelForRestriction] public NativeArray<Vector4> UVList;

        void Execute([EntityIndexInQuery] int index, ref SpriteSheetComponent spriteSheetComp, ref HertaComponent hertaComp) 
        {
            UVList[index] = spriteSheetComp.UV;
            matrixList[index] = hertaComp.matrix;
        }
    }

    [BurstCompile]
    public partial struct MeshPropertiesJob : IJobEntity
    {
        [NativeDisableParallelForRestriction] public NativeArray<MeshProperties> meshProperties;

        void Execute([EntityIndexInQuery] int i, ref SpriteSheetComponent spriteSheetComp, ref HertaComponent hertaComp)
        {
            MeshProperties properties = new MeshProperties()
            {
                mat = hertaComp.matrix,
                sprite_UV = spriteSheetComp.UVf
            };
            meshProperties[i] = properties;
        }
    }
}
