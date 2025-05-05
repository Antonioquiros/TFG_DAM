using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class flipEnemigo : MonoBehaviour
{
    private SpriteRenderer sprite;
    private float posicionXAnterior;

    private void Start()
    {
        posicionXAnterior = transform.parent.position.x;
        sprite = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        // moviendo hacia la derecha
        sprite.flipX = posicionXAnterior < transform.position.x;

        // Actualizar posicion anterior
        posicionXAnterior = transform.position.x;
    }
}
