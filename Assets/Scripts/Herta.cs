using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Herta : MonoBehaviour
{
    public float radius, speed, lifeTime;
    public Vector2 direction;
    
    private float alphaFade = 0.0f;
    private SpriteRenderer spriteRenderer;

    public void Initialize(float radius, float speed, float lifeTime)
    {
        this.speed = speed;
        this.radius = radius;
        this.lifeTime = lifeTime;
        direction = new Vector2();

        spriteRenderer.size = Vector2.one * (radius * 2);
        RandomizeHertaPosAndDir(HertaManager.moveArea);
    }

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void RandomizeHertaPosAndDir(Bounds moveArea)
    {
        Vector2 minPos = moveArea.min + (Vector3.one * radius),
                maxPos = moveArea.max - (Vector3.one * radius);

        // random direction magic
        direction = Random.insideUnitCircle.normalized;

        transform.position = new Vector2(
            Random.Range(minPos.x, maxPos.x),
            Random.Range(minPos.y, maxPos.y)
        );
    }

    public void UpdateHertaMovement(bool dontDestroy)
    {
        Vector2 minPos = HertaManager.moveArea.min + (Vector3.one * radius), 
                maxPos = HertaManager.moveArea.max - (Vector3.one * radius);

        if (transform.position.x <= minPos.x || transform.position.x >= maxPos.x)
        {
            direction.x *= -1;
        }
        if (transform.position.y <= minPos.y || transform.position.y >= maxPos.y)
        {
            direction.y *= -1;
        }

        // move herta
        transform.position += new Vector3(direction.x, direction.y) * speed * Time.deltaTime;
        // reduce lifetime each second
        lifeTime = dontDestroy ? lifeTime : lifeTime - Time.deltaTime;
    }
}
