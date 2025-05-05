using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlEnemigo : MonoBehaviour
{
    public float velocidad;
    public Vector3 posicionFin;
    public int numVidas = 0;

    private Vector3 posicionInicio;
    private bool moviendoAFin;
    void Start()
    {
        posicionInicio = transform.position;
        moviendoAFin = true;
    }

    void Update()
    {
        MoverEnemigo();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Si el objeto que ha colisionado con el enemigo es el jugador
        if (collision.gameObject.CompareTag("Player")) {
            collision.gameObject.GetComponent<ControlJugador>().QuitarVida(1);
        }
        // Si el objeto que ha colisionado es una bala
        else if (collision.gameObject.CompareTag("bullet"))
        {
            RecibirDaño(1); // Restar 1 vida al enemigo
            Destroy(collision.gameObject); // Destruir la bala tras impactar
        }
    }

    private void RecibirDaño(int daño)
    {
        numVidas -= daño;
        Debug.Log(gameObject.name + " recibió daño, vidas restantes: " + numVidas);

        if (numVidas <= 0)
        {
            DestruirEnemigo();
        }
    }

    private void DestruirEnemigo()
    {
        Debug.Log(gameObject.name + " ha sido eliminado.");
        Destroy(gameObject);
    }
    private void MoverEnemigo() {
        //1. Calcular la posicion de destino
        Vector3 posicionDestino = (moviendoAFin) ? posicionFin : posicionInicio;

        //2. Mover el enemigo 
        transform.position = Vector3.MoveTowards(transform.position, posicionDestino, velocidad * Time.deltaTime);

        //Cambio de direccion
        if (transform.position == posicionFin) moviendoAFin = false;
        if (transform.position == posicionInicio) moviendoAFin = true;
    }
}
