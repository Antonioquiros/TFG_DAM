using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerStats : MonoBehaviour
{
    private APIClient apiClient;
    private float tiempoInicioPartida;
    private int enemigosEliminados;
    private bool partidaActiva = false;

    void Start()
    {
        apiClient = FindObjectOfType<APIClient>();
        if (apiClient == null || apiClient.GetUsuarioId() <= 0)
        {
            Debug.LogError("Usuario no logueado, redirigiendo...");
            SceneManager.LoadScene("Login");
            return;
        }

        IniciarPartida();
    }

    public void IniciarPartida()
    {
        tiempoInicioPartida = (int)Time.time;
        partidaActiva = true;
    }

    public void EnemigoEliminado()
    {
        apiClient.IncrementarEnemigosEliminados();  // Actualizar enemigos eliminados en el servidor
    }

    public void PartidaGanada()
    {
        if (!partidaActiva) return;

        // Calcular tiempo jugado
        int tiempoSegundos = (int)(Time.time - tiempoInicioPartida);

        // Actualizar estadísticas: Tiempo jugado y veces ganadas
        apiClient.IncrementarTiempoJugado(tiempoSegundos);
        apiClient.IncrementarVecesGanadas();

        // Finalizar partida
        partidaActiva = false;
    }

    public void PartidaPerdida()
    {
        if (!partidaActiva) return;

        // Calcular tiempo jugado
        int tiempoSegundos = (int)(Time.time - tiempoInicioPartida);

        // Actualizar estadísticas: Tiempo jugado, derrotas y enemigos eliminados
        apiClient.IncrementarTiempoJugado(tiempoSegundos);
        apiClient.IncrementarDerrotas();

        // Finalizar partida
        partidaActiva = false;
    }
}
