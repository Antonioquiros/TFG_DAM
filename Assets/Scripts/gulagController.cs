using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class gulagController : MonoBehaviour
{
    public GameObject enemigoGulag;
    private ControlJugador jugador;

    void Start()
    {
        jugador = FindObjectOfType<ControlJugador>();

        // Asegurar que el jugador tenga 1 vida en el Gulag
        if (jugador != null)
        {
            jugador.numVidas = 1f;
            jugador.hud.setVidasTxt(jugador.numVidas);
        }
    }

    void Update()
    {
        if (enemigoGulag == null) // Si el enemigo fue derrotado
        {
            GanarGulag();
        }
        else if (jugador != null && jugador.numVidas <= 0) // Si el jugador muere
        {
            PerderGulag();
        }
    }

    private void GanarGulag()
    {
        if (jugador != null)
        {
            jugador.GanarGulag();
        }
        else
        {
            // Fallback por si no encuentra el jugador
            SceneManager.LoadScene("VolverAlNivelScene");
        }
    }

    private void PerderGulag()
    {
        if (jugador != null)
        {
            jugador.PerderGulag();
        }
        else
        {
            // Fallback por si no encuentra el jugador
            SceneManager.LoadScene("FinJuegoScene");
        }
    }
}
