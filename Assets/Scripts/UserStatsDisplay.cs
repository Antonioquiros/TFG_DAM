using System.Collections;
using TMPro;
using UnityEngine;

public class UserStatsDisplay : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text victoriesText;
    public TMP_Text defeatsText;
    public TMP_Text enemiesText;
    public TMP_Text playTimeText;
    public TMP_Text lastConnectionText;

    [Header("Referencia API")]
    [SerializeField] private APIClient apiClient;

    void Start()
    {
        if (apiClient == null)
            apiClient = FindObjectOfType<APIClient>();

        UpdateStatsInfo();
    }

    public void UpdateStatsInfo()
    {
        if (apiClient != null)
        {
            Usuario usuario = apiClient.GetUsuarioActual();

            if (usuario != null)
            {
                // Mostrar datos locales primero
                victoriesText.text = $"Victorias: {usuario.veces_ganadas}";
                defeatsText.text = $"Derrotas: {usuario.derrotas}";
                enemiesText.text = $"Enemigos derrotados: {usuario.enemigos_eliminados}";
                playTimeText.text = $"Tiempo jugado: {usuario.tiempo_jugado}";
                lastConnectionText.text = $"Última conexión: {FormatDate(usuario.ultima_partida)}";

                // Refrescar desde API
                StartCoroutine(apiClient.RefrescarUsuarioActual(usuario.id_usuario, (success) =>
                {
                    if (success)
                    {
                        Usuario actualizado = apiClient.GetUsuarioActual();
                        victoriesText.text = $"Victorias: {actualizado.veces_ganadas}";
                        defeatsText.text = $"Derrotas: {actualizado.derrotas}";
                        enemiesText.text = $"Enemigos derrotados: {actualizado.enemigos_eliminados}";
                        playTimeText.text = $"Tiempo jugado: {actualizado.tiempo_jugado}";
                        lastConnectionText.text = $"Última conexión: {FormatDate(actualizado.ultima_partida)}";
                    }
                    else
                    {
                        Debug.LogWarning("No se pudo actualizar las estadísticas del usuario.");
                    }
                }));
            }
            else
            {
                Debug.LogWarning("No hay usuario logueado.");
            }
        }
    }

    private string FormatDate(string rawDate)
    {
        if (System.DateTime.TryParse(rawDate, out var date))
        {
            return date.ToString("dd/MM/yyyy");
        }
        return rawDate;
    }
}
