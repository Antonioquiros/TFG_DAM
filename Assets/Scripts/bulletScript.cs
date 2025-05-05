using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class bulletScript : MonoBehaviour
{
    public float speed;
    public float maxDistance = 5f; // Distancia máxima que puede recorrer la bala
    public string playerTag = "Player"; // Tag del jugador para ignorar colisiones

    private Rigidbody2D Rigidbody2D;
    private Vector2 Direction;
    private Vector3 initialPosition; // Posición inicial (posición del jugador)
    private GameObject shooter; // Referencia al objeto que disparó esta bala


    void Start()
    {
        Rigidbody2D = GetComponent<Rigidbody2D>();
        initialPosition = transform.position; // Guardar la posición inicial
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        Rigidbody2D.velocity = Direction * speed;

        // Comprobar si la bala ha recorrido la distancia máxima
        float distanceTraveled = Vector3.Distance(transform.position, initialPosition);
        if (distanceTraveled >= maxDistance)
        {
            DestroyBullet();
        }
    }

    public void setDirection(Vector3 direction)
    {
        Direction = direction;

        // Rotar la bala según la dirección
        if (direction == Vector3.left)
        {
            // Si va hacia la izquierda, rotar 180 grados
            transform.rotation = Quaternion.Euler(0, 0, 180);
        }
        else
        {
            // Si va hacia la derecha, mantener rotación normal
            transform.rotation = Quaternion.identity;
        }
    }
    // Establecer quién disparó esta bala
    public void SetShooter(GameObject player)
    {
        shooter = player;

        // Desactivar las colisiones entre la bala y el jugador
        Physics2D.IgnoreCollision(GetComponent<Collider2D>(), player.GetComponent<Collider2D>(), true);
    }
    // Detectar colisiones con otros objetos
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Verificar si la colisión es con el jugador que disparó
        if (collision.gameObject.CompareTag(playerTag) && collision.gameObject == shooter)
        {
            // No hacer nada si colisiona con el jugador que la disparó
            return;
        }

        // Destruir la bala si colisiona con cualquier otro objeto
        DestroyBullet();
    }

    // También podemos usar OnTriggerEnter2D si la bala tiene un Collider configurado como Trigger
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Verificar si la colisión es con el jugador que disparó
        if (collision.CompareTag(playerTag) && collision.gameObject == shooter)
        {
            // No hacer nada si colisiona con el jugador que la disparó
            return;
        }

        // Si la bala choca con una llave, no se destruye
        if (collision.CompareTag("llave1") || collision.CompareTag("llave2"))
        {
            return;
        }

        // Si la bala choca con cualquier otra cosa, se destruye
        DestroyBullet();
    }

    // Método para destruir la bala
    private void DestroyBullet()
    {
        Destroy(gameObject);
    }
}
