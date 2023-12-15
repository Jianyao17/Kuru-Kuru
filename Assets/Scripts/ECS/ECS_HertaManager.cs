using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.UIElements;

public class ECS_HertaManager : MonoBehaviour
{
    [Header("Herta Entity")]
    [SerializeField] private Mesh hertaMesh;
    [SerializeField] private Material hertaMaterial;
    [SerializeField] private int hertaEntityCount = 0;
    public bool SpawnUsingCursor;

    [Header("Herta Animation")]
    public int HertaFrameCount;
    public int FramesPerSecond;

    [Header("Herta Properties")]
    [Range(2f, 4f)] public float HertaRadius = 2;
    [Range(5f, 15f)] public float HertaSpeed = 12;
    [Range(3f, 10f)] public float HertaLifetime = 9;
    public bool randomSize, randomSpeed, dontDestroy;

    [Header("Herta Sounds")]
    public AudioClip[] hertaSounds;

    public int HertaCount {
        get { return HertaEntities.Count; }
        private set {
            if (value > HertaEntities.Count) SpawnHertaEntity(value - HertaEntities.Count);
            else if (value < HertaEntities.Count) RemoveHertaEntity(HertaEntities.Count - value);
            _tempHertaCount = HertaEntities.Count;
        }
    }

    public static Mesh HertaMesh; // static variable for easy access
    public static Material HertaMaterial; // static variable for easy access
    public static List<Entity> HertaEntities; // static variable for easy access

    private static Camera _camera; // static variable for easy access
    public static Bounds MoveArea; // static variable for easy access
    public static bool DontDestroyEntity; // static variable for easy access

    private AudioSource _audioSource;
    //private Vector2 _tempScreenSize = Vector2.one;
    private int _tempHertaCount = 0;

    private EntityManager _entityManager;
    private EntityArchetype _hertaEntityArchetype;

    private bool PLATFORM_WIN, PLATFORM_ANDROID;

    void Awake()
    {
#if UNITY_STANDALONE_WIN
        PLATFORM_WIN = true;
        PLATFORM_ANDROID = false;
#elif UNITY_ANDROID
        PLATFORM_ANDROID = true;
        PLATFORM_WIN = false;
#endif

        _camera = Camera.main;
        _audioSource = GetComponent<AudioSource>();
        HertaEntities = new List<Entity>();
        _tempHertaCount = HertaCount;

        // Setting herta mesh & material
        HertaMaterial = hertaMaterial;
        HertaMesh = hertaMesh;

        // Make herta entity archtype
        _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        _hertaEntityArchetype = _entityManager.CreateArchetype(
            typeof(SpriteSheetComponent),
            typeof(LocalTransform),
            typeof(HertaComponent)
        );
    }

    // Register Callback Reclculate Bounds if screen changing
    private void Start() => UIManager.panelElement.RegisterCallback<GeometryChangedEvent>(RecalculateBounds);

    private void Update()
    {
        // Update DontDestroyEntity variable
        DontDestroyEntity = dontDestroy; 

        // If Entity List Count changing outsite HertaCount Properties 
        // Update hertaEntityCount variable
        if (_tempHertaCount != HertaCount) hertaEntityCount = HertaCount;
        HertaCount = hertaEntityCount;

        //Debug.Log($"HertaCount : {HertaCount} " +
        //          $"Entity Count : {_entityManager.Debug.EntityCount - 1}");
    }

    private void FixedUpdate()
    {
        Touch touch = Input.touchCount > 0 ? Input.GetTouch(0) : default(Touch);

        // Get the point position then convert to world position
        var pos = PLATFORM_WIN ? Input.mousePosition : PLATFORM_ANDROID ? touch.position : Vector3.zero;
        Vector3 point = _camera.ScreenToWorldPoint(pos);

        var holdClick = Input.GetMouseButton(0) || (PLATFORM_ANDROID && (touch.phase == TouchPhase.Began || touch.phase == TouchPhase.Moved));
        if (SpawnUsingCursor && holdClick) 
            SpawnHertaEntity(point);
    }

    public void SpawnHertaEntity()
    {
        Entity entity = _entityManager.CreateEntity(_hertaEntityArchetype);

        Vector2 minPos = MoveArea.min + (Vector3.one * HertaRadius),
                maxPos = MoveArea.max - (Vector3.one * HertaRadius);

        _entityManager.SetComponentData(entity,
            new LocalTransform
            {   // Random Spawn Position
                Position = new float3(
                    UnityEngine.Random.Range( minPos.x, maxPos.x),
                    UnityEngine.Random.Range(minPos.y, maxPos.y),
                    _camera.nearClipPlane)
            }
        );

        _entityManager.SetComponentData(entity,
            new SpriteSheetComponent
            {
                frameCount = HertaFrameCount,
                framesPerSecond = FramesPerSecond
            }
        );

        _entityManager.SetComponentData(entity,
            new HertaComponent
            {
                lifeTime = HertaLifetime,
                speed = randomSpeed ? UnityEngine.Random.Range(5f, 15f) : HertaSpeed,
                radius = randomSize ? UnityEngine.Random.Range(2f, 4f) : HertaRadius,
                direction = UnityEngine.Random.insideUnitCircle.normalized
            }
        );
        
        HertaEntities.Add(entity);

        // Play SFX
        _audioSource.Stop();
        _audioSource.PlayOneShot(RandomHertaClip());
    }

    public void SpawnHertaEntity(Vector2 worldPos)
    {
        Entity entity = _entityManager.CreateEntity(_hertaEntityArchetype);

        if (!MoveArea.Contains(worldPos)) return;

        _entityManager.SetComponentData(entity,
            new LocalTransform
            { 
                Position = new float3(worldPos.x, worldPos.y, _camera.nearClipPlane)
            }
        );

        _entityManager.SetComponentData(entity,
            new SpriteSheetComponent
            {
                frameCount = HertaFrameCount,
                framesPerSecond = FramesPerSecond
            }
        );

        _entityManager.SetComponentData(entity,
            new HertaComponent
            {
                lifeTime = HertaLifetime,
                speed = randomSpeed ? UnityEngine.Random.Range(5f, 15f) : HertaSpeed,
                radius = randomSize ? UnityEngine.Random.Range(2f, 4f) : HertaRadius,
                direction = UnityEngine.Random.insideUnitCircle.normalized
            }
        );

        HertaEntities.Add(entity);

        // Play SFX
        _audioSource.Stop();
        _audioSource.PlayOneShot(RandomHertaClip());
    }

    public void SpawnHertaEntity(int amount)
    {
        NativeArray<Entity> entityArray = _entityManager.CreateEntity(_hertaEntityArchetype, amount, Allocator.Temp);

        Vector2 minPos = MoveArea.min + (Vector3.one * HertaRadius),
                maxPos = MoveArea.max - (Vector3.one * HertaRadius);

        foreach (var entity in entityArray)
        {
            _entityManager.SetComponentData(entity,
                new LocalTransform
                {
                    Position = new float3(
                        UnityEngine.Random.Range(minPos.x, maxPos.x),
                        UnityEngine.Random.Range(minPos.y, maxPos.y),
                        _camera.nearClipPlane)
                }
            );

            _entityManager.SetComponentData(entity,
                new SpriteSheetComponent 
                {
                    frameCount = HertaFrameCount,
                    framesPerSecond = FramesPerSecond
                }
            );

            _entityManager.SetComponentData(entity,
                new HertaComponent
                {
                    lifeTime = HertaLifetime,
                    speed = randomSpeed ? UnityEngine.Random.Range(5f, 15f) : HertaSpeed,
                    radius = randomSize ? UnityEngine.Random.Range(2f, 4f) : HertaRadius,
                    direction = UnityEngine.Random.insideUnitCircle.normalized
                }
            );
        }

        HertaEntities.AddRange( entityArray );
        entityArray.Dispose();

        // Play SFX
        _audioSource.Stop();
        _audioSource.PlayOneShot(RandomHertaClip());
    }

    public void ClearHertaEntity()
    {
        _entityManager.DestroyEntity(HertaEntities.ToNativeArray(Allocator.Temp));
        HertaEntities.Clear();
    }

    public void RemoveHertaEntity(Entity entity) 
    { 
        _entityManager.DestroyEntity(entity);
        HertaEntities.Remove(entity); 
    }
    public void RemoveHertaEntity(int amount) 
    {
        _entityManager.DestroyEntity(HertaEntities.GetRange(HertaCount - amount, amount).ToNativeArray(Allocator.Temp));
        HertaEntities.RemoveRange(HertaCount - amount, amount); 
    }

    // Calculate herta Bounds area in world point
    private void RecalculateBounds(GeometryChangedEvent evt = null)
    {
        //_tempScreenSize = _camera.pixelRect.size;

        float height = UIManager.PanelHeight == float.NaN ? 80f : UIManager.PanelHeight;
        Vector2 minPos = _camera.ScreenToWorldPoint(new Vector2(0, height)),
                maxPos = _camera.ScreenToWorldPoint(_camera.pixelRect.size);

        MoveArea.min = minPos;
        MoveArea.max = maxPos;
    }

    private AudioClip RandomHertaClip()
    {
        return hertaSounds[UnityEngine.Random.Range(0, hertaSounds.Length)];
    }
}
