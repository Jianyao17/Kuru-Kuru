using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HertaManager : MonoBehaviour
{
    [Header ("Herta Properties")]
    public GameObject hertaPrefabs;
    public bool randomSize, randomSpeed, dontDestroy;
    [Range(2f, 4f)] public float Radius;
    [Range(5f, 15f)] public float Speed;
    [Range(3f, 10f)] public float Lifetime;
    public AudioClip[] hertaSounds;
    
    public int HertaCount { 
        get { return hertaList.Count; }
    }

    public static Bounds moveArea;
    private static List<Herta> hertaList;
    private static Queue<Herta> destroyQueue;
    private static Camera _camera;

    private AudioSource _audioSource;
    private Vector2 _tempScreenSize = Vector2.one;

    void Awake() 
    {
        _camera = Camera.main;
        hertaList = new List<Herta>();
        destroyQueue = new Queue<Herta>();
        _audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        // Reclculate Bounds if screen changing
        if (_tempScreenSize != _camera.pixelRect.size) RecalculateBounds();

        foreach (var herta in hertaList)
        {
            herta.UpdateHertaMovement(dontDestroy);
            if (herta.lifeTime <= 0) destroyQueue.Enqueue(herta);
        }
        DestroyHertaInstances(dontDestroy);
    }

    private void DestroyHertaInstances(bool dontDestroy)
    {
        if (destroyQueue.Count <= 0 || dontDestroy) return;

        foreach (var herta in destroyQueue)
        {
            hertaList.Remove(herta);
            Destroy(herta.gameObject);
        }
        destroyQueue.Clear();
    }

    // Calculate herta Bounds area in world point
    private void RecalculateBounds()
    {
        _tempScreenSize = _camera.pixelRect.size;

        Vector2 minPos = _camera.ScreenToWorldPoint(new Vector2(0, 78)),
                maxPos = _camera.ScreenToWorldPoint(_camera.pixelRect.size);

        moveArea.min = minPos;
        moveArea.max = maxPos;
    }

    public void SpawnHerta()
    {
        // Random some value if true
        float radius = randomSize == true ? UnityEngine.Random.Range(2f, 4f) : Radius,
              speed = randomSpeed == true ? UnityEngine.Random.Range(5f, 15f) : Speed,
              lifetime = Lifetime;

        // Spawn herta mechanics
        Herta herta = Instantiate(hertaPrefabs, transform).GetComponent<Herta>();
        herta.Initialize(radius, speed, lifetime);

        hertaList.Add(herta);

        // Play SFX
        _audioSource.Stop();
        _audioSource.PlayOneShot(RandomHertaClip());
    }

    public void ClearHertaList()
    {
        // Delete all herta
        hertaList.ForEach(herta => {
            Destroy(herta.gameObject);
        });
        hertaList.Clear();
    }

    private AudioClip RandomHertaClip()
    {
        return hertaSounds[UnityEngine.Random.Range(0, hertaSounds.Length)];
    }
}
