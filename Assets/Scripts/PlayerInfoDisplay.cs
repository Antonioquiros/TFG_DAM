using TMPro;
using UnityEngine;

public class PlayerInfoDisplay : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text userIdText;
    public TMP_Text usernameText;
    public TMP_Text registrationDateText;

    [Header("Referencia API")]
    [SerializeField] private APIClient apiClient; // Asigna esto desde el Inspector

    void Start()
    {
        // Si apiClient no está asignado, búscalo automáticamente
        if (apiClient == null)
            apiClient = FindObjectOfType<APIClient>();

        UpdateProfileInfo();
    }

    public void UpdateProfileInfo()
    {
        if (apiClient != null)
        {
            Usuario usuario = apiClient.GetUsuarioActual();

            if (usuario != null)
            {
                // Primero muestra la info local
                userIdText.text = $"ID: {usuario.id_usuario}";
                usernameText.text = $"Usuario: {usuario.nombre_usuario}";
                registrationDateText.text = $"Fecha de registro: {FormatDate(usuario.fecha_registro)}";

                // Luego refresca desde la API para obtener datos actualizados
                StartCoroutine(apiClient.RefrescarUsuarioActual(usuario.id_usuario, (success) =>
                {
                    if (success)
                    {
                        Usuario actualizado = apiClient.GetUsuarioActual();
                        userIdText.text = $"ID: {actualizado.id_usuario}";
                        usernameText.text = $"Usuario: {actualizado.nombre_usuario}";
                        registrationDateText.text = $"Fecha de registro: {FormatDate(actualizado.fecha_registro)}";
                    }
                    else
                    {
                        Debug.LogWarning("No se pudo actualizar la info del usuario");
                    }
                }));
            }
            else
            {
                Debug.LogWarning("No hay usuario logueado");
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
