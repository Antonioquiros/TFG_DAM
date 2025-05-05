using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class llaveScript : MonoBehaviour
{
    void Start()
    {
        // Asegurarse de que el collider está configurado como trigger
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null && !collider.isTrigger)
        {
            collider.isTrigger = true;
            Debug.Log("Se cambió automáticamente el collider a trigger en " + gameObject.name);
        }
    }

        private void OnTriggerEnter2D(Collider2D other)
    {
        // Verificar si el objeto que entra en contacto es el jugador
        if (other.CompareTag("Player"))
        {
            Debug.Log("Llave recogida por " + other.name);

            // Destruir la llave para simular la recolección
            Destroy(gameObject);
        }
    }
}
