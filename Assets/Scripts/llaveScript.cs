using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class llaveScript : MonoBehaviour
{
    public AudioClip recolectarFX;
    void Start()
    {
        // Asegurarse de que el collider está configurado como trigger
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null && !collider.isTrigger)
        {
            collider.isTrigger = true;
        }
    }

        private void OnTriggerEnter2D(Collider2D other)
    {
        // Verificar si el objeto que entra en contacto es el jugador
        if (other.CompareTag("Player"))
        {
            other.GetComponent<AudioSource>().PlayOneShot(recolectarFX);
            Debug.Log("Llave recogida por " + other.name);

            // Destruir la llave para simular la recolección
            Destroy(gameObject);
        }
    }
}
