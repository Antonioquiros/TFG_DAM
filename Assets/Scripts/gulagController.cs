using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class gulagController : MonoBehaviour
{
    public GameObject enemigoGulag; // El enemigo en el Gulag
    public GameObject jugador; // El jugador que entra en el Gulag
    private ControlJugador controlJugador; // Referencia al script de control del jugador
    private bool jugadorGanoGulag = false;

    void Start()
    {
        // Buscar el controlador del jugador en la escena
        // Verificamos que tengamos una referencia al jugador
        if (jugador == null)
        {
            jugador = GameObject.FindGameObjectWithTag("Player");
        }

        if (jugador != null)
        {
            controlJugador = jugador.GetComponent<ControlJugador>();
            // Asegurarse de que el jugador comience con 1 vida en el Gulag
            if (controlJugador != null)
            {
                controlJugador.numVidas = 1f;
                if (controlJugador.hud != null)
                {
                    controlJugador.hud.setVidasTxt(controlJugador.numVidas);
                }
            }
        }
    }

    void Update()
    {
        // Verificamos que tengamos referencias válidas
        if (controlJugador == null || jugadorGanoGulag) return;

        if (controlJugador.numVidas <= 0)
        {
            // Si el jugador muere en el Gulag, perderá el juego
            PerderGulag();
        }
        else if (enemigoGulag == null)
        {
            // Si el enemigo es derrotado (null), el jugador ha ganado
            GanarGulag();
        }
    }

    private void GanarGulag()
    {
        jugadorGanoGulag = true;

        // Informar al controlador del jugador que ganó
        if (controlJugador != null)
        {
            controlJugador.GanarGulag();
        }
        else
        {
            // Si no hay referencia al controlador, configurar para volver al nivel
            PlayerPrefs.SetInt("VueltaDeGulag", 1);
            PlayerPrefs.Save();

            // Cargar la escena con el mensaje de victoria
            SceneManager.LoadScene("VolverAlNivelScene");
        }
    }

    private void PerderGulag()
    {
        // El jugador ha perdido, se termina el juego
        if (controlJugador != null)
        {
            controlJugador.PerderGulag();
        }
        else
        {
            // Si no hay referencia al controlador, cargamos la escena de fin directamente
            SceneManager.LoadScene("FinJuegoScene");
        }
    }
}
