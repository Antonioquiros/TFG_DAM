using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using System.Collections;
using System;
using UnityEngine.SceneManagement;

public class APIClient : MonoBehaviour
{
    // Eventos para notificar resultados
    public UnityEvent<Usuario> OnLoginSuccess = new UnityEvent<Usuario>();
    public UnityEvent<string> OnLoginFailed = new UnityEvent<string>();
    public UnityEvent<Usuario> OnRegisterSuccess = new UnityEvent<Usuario>();
    public UnityEvent<string> OnRegisterFailed = new UnityEvent<string>();
    public UnityEvent<bool> OnStatsUpdated = new UnityEvent<bool>();
    public UnityEvent OnLogoutSuccess = new UnityEvent();

    private string apiUrl = "http://localhost:3000";
    private int usuarioId; // ID del usuario logueado/registrado
    public Usuario usuarioActual;

    public Usuario GetUsuarioActual()
    {
        return usuarioActual;
    }
    // M�todo para actualizar la info del usuario desde la API
    public IEnumerator RefrescarUsuarioActual(int idUsuario, Action<bool> callback)
    {
        string url = $"{apiUrl}/usuario/{idUsuario}";

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    string json = request.downloadHandler.text;
                    usuarioActual = JsonUtility.FromJson<Usuario>(json);
                    callback?.Invoke(true);
                }
                catch (Exception e)
                {
                    Debug.LogError("Error parseando usuario: " + e.Message);
                    callback?.Invoke(false);
                }
            }
            else
            {
                Debug.LogError("Error al obtener usuario: " + request.error);
                callback?.Invoke(false);
            }
        }
    }
    void Awake()
    {
        // Mant�n una �nica instancia entre escenas (si es necesario)
        DontDestroyOnLoad(gameObject);
    }

    // ==================== REGISTRO ====================
    public void RegistrarUsuario(string nombreUsuario, string contrase�a)
    {
        StartCoroutine(RegistroCoroutine(nombreUsuario, contrase�a));
    }

    private IEnumerator RegistroCoroutine(string nombreUsuario, string contrase�a)
    {
        string url = $"{apiUrl}/registro";

        UsuarioRegistroPayload payload = new UsuarioRegistroPayload
        {
            nombre_usuario = nombreUsuario,
            contrase�a = contrase�a
        };

        string jsonData = JsonUtility.ToJson(payload);
        byte[] rawData = System.Text.Encoding.UTF8.GetBytes(jsonData);

        using (UnityWebRequest webRequest = new UnityWebRequest(url, "POST"))
        {
            webRequest.uploadHandler = new UploadHandlerRaw(rawData);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");

            Debug.Log($"Enviando solicitud a {url} con datos: {jsonData}");
            yield return webRequest.SendWebRequest();

            // IMPORTANTE: Verificar realmente si la solicitud fue exitosa y el c�digo de respuesta
            Debug.Log($"C�digo de respuesta HTTP: {webRequest.responseCode}");
            if (webRequest.result == UnityWebRequest.Result.Success && webRequest.responseCode == 200)
            {
                string responseText = webRequest.downloadHandler.text;
                Debug.Log("Respuesta del servidor (registro): " + responseText);

                // Verificar si la respuesta contiene datos v�lidos
                if (!string.IsNullOrEmpty(responseText))
                {
                    try
                    {
                        Usuario usuario = JsonUtility.FromJson<Usuario>(responseText);

                        // Verificar que el usuario tiene un ID v�lido
                        if (usuario != null && usuario.id_usuario > 0)
                        {
                            usuarioId = usuario.id_usuario;
                            Debug.Log($"Registro exitoso! ID: {usuarioId}, Usuario: {usuario.nombre_usuario}");
                            Debug.Log($"Fecha registro: {usuario.fecha_registro}, �ltima partida: {usuario.ultima_partida}");
                            OnRegisterSuccess.Invoke(usuario);
                            yield break; // Salir del coroutine
                        }
                        else
                        {
                            Debug.LogError("El servidor devolvi� un usuario sin ID v�lido");
                            OnRegisterFailed.Invoke("Error en el formato de respuesta del servidor");
                        }
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError($"Error al procesar la respuesta JSON: {e.Message}");
                        OnRegisterFailed.Invoke("Error al procesar la respuesta del servidor");
                    }
                }
                else
                {
                    Debug.LogError("El servidor devolvi� una respuesta vac�a");
                    OnRegisterFailed.Invoke("El servidor devolvi� una respuesta vac�a");
                }
            }
            else
            {
                Debug.LogError($"Error en la solicitud: {webRequest.error}");
                OnRegisterFailed.Invoke(webRequest.error);
            }
        }
    }

    // ==================== LOGIN ====================
    public void IniciarSesion(string nombreUsuario, string contrase�a)
    {
        StartCoroutine(LoginCoroutine(nombreUsuario, contrase�a));
    }

    private IEnumerator LoginCoroutine(string nombreUsuario, string contrase�a)
    {
        string url = $"{apiUrl}/login";

        UsuarioLoginPayload payload = new UsuarioLoginPayload
        {
            nombre_usuario = nombreUsuario,
            contrase�a = contrase�a
        };

        string jsonData = JsonUtility.ToJson(payload);
        byte[] rawData = System.Text.Encoding.UTF8.GetBytes(jsonData);

        using (UnityWebRequest webRequest = new UnityWebRequest(url, "POST"))
        {
            webRequest.uploadHandler = new UploadHandlerRaw(rawData);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");

            yield return webRequest.SendWebRequest();

            // IMPORTANTE: Verificar realmente si la solicitud fue exitosa
            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                string responseText = webRequest.downloadHandler.text;
                Debug.Log("Respuesta del servidor (login): " + responseText);

                // Verificar si la respuesta contiene datos v�lidos
                if (!string.IsNullOrEmpty(responseText))
                {
                    try
                    {
                        Usuario usuario = JsonUtility.FromJson<Usuario>(responseText);

                        // Verificar que el usuario tiene un ID v�lido
                        if (usuario != null && usuario.id_usuario > 0)
                        {
                            usuarioId = usuario.id_usuario;
                            usuarioActual = usuario; // �Aqu� se guardan TODOS los datos!

                            Debug.Log($"Bienvenido: {usuario.nombre_usuario} (ID: {usuarioId})");
                            OnLoginSuccess.Invoke(usuario);

                            // Guardar en PlayerPrefs (opcional)
                            PlayerPrefs.SetInt("UserId", usuarioId);
                            PlayerPrefs.Save();

                            yield break;
                        }
                        else
                        {
                            Debug.LogError("El servidor devolvi� un usuario sin ID v�lido");
                            OnLoginFailed.Invoke("Credenciales incorrectas o usuario no encontrado");
                        }
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError($"Error al procesar la respuesta JSON: {e.Message}");
                        OnLoginFailed.Invoke("Error al procesar la respuesta del servidor");
                    }
                }
                else
                {
                    Debug.LogError("El servidor devolvi� una respuesta vac�a");
                    OnLoginFailed.Invoke("El servidor devolvi� una respuesta vac�a");
                }
            }
            else
            {
                Debug.LogError($"Error en la solicitud: {webRequest.error}");
                OnLoginFailed.Invoke(webRequest.error);
            }
        }
    }

    // ==================== LOGOUT  ====================
    public void CerrarSesion()
    {
        // Verificaci�n adicional
        if (usuarioActual == null && usuarioId == 0)
        {
            Debug.LogWarning("No hay sesi�n activa para cerrar.");
            return;
        }

        // Limpiar datos locales
        usuarioActual = null;
        usuarioId = 0;
        PlayerPrefs.DeleteKey("UserId");
        PlayerPrefs.Save();

        // Notificar que el logout fue exitoso
        OnLogoutSuccess.Invoke();

        Debug.Log("Sesi�n cerrada localmente");
        // Cargar escena inicial
        SceneManager.LoadScene("InitialScene");
    }

    // ==================== ACTUALIZAR ESTAD�STICAS ====================
    public void IncrementarEnemigosEliminados()
    {
        StartCoroutine(UpdateEnemigosEliminadosCoroutine());
    }

    private IEnumerator UpdateEnemigosEliminadosCoroutine()
    {
        string url = $"{apiUrl}/usuario/{usuarioId}/incrementar-enemigos-eliminados";

        if (usuarioId <= 0)
        {
            Debug.LogError("No hay usuario logueado para actualizar enemigos eliminados");
            OnStatsUpdated.Invoke(false);
            yield break;
        }

        using (UnityWebRequest webRequest = new UnityWebRequest(url, "POST"))
        {
            webRequest.SetRequestHeader("Content-Type", "application/json");
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Enemigos eliminados incrementados!");
                OnStatsUpdated.Invoke(true);

                if (usuarioActual != null)
                {
                    usuarioActual.enemigos_eliminados += 1;
                }
            }
            else
            {
                Debug.LogError($"Error: {webRequest.error}");
                OnStatsUpdated.Invoke(false);
            }
        }
    }

    public void IncrementarDerrotas()
    {
        StartCoroutine(UpdateDerrotasCoroutine());
    }

    private IEnumerator UpdateDerrotasCoroutine()
    {
        string url = $"{apiUrl}/usuario/{usuarioId}/incrementar-derrotas";

        if (usuarioId <= 0)
        {
            Debug.LogError("No hay usuario logueado para incrementar derrotas");
            OnStatsUpdated.Invoke(false);
            yield break;
        }

        using (UnityWebRequest webRequest = new UnityWebRequest(url, "POST"))
        {
            webRequest.SetRequestHeader("Content-Type", "application/json");
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Derrotas incrementadas!");
                OnStatsUpdated.Invoke(true);

                if (usuarioActual != null)
                {
                    usuarioActual.derrotas += 1;
                }
            }
            else
            {
                Debug.LogError($"Error: {webRequest.error}");
                OnStatsUpdated.Invoke(false);
            }
        }
    }

    public void IncrementarTiempoJugado(int segundos)
    {
        StartCoroutine(UpdateTiempoJugadoCoroutine(segundos));
    }

    private IEnumerator UpdateTiempoJugadoCoroutine(int segundos)
    {
        string url = $"{apiUrl}/usuario/{usuarioId}/incrementar-tiempo-jugado";

        if (usuarioId <= 0)
        {
            Debug.LogError("No hay usuario logueado para actualizar tiempo jugado");
            OnStatsUpdated.Invoke(false);
            yield break;
        }

        // Crear el payload con el tiempo en segundos
        TiempoJugadoPayload payload = new TiempoJugadoPayload { tiempo_jugado = segundos };
        string jsonData = JsonUtility.ToJson(payload);
        byte[] rawData = System.Text.Encoding.UTF8.GetBytes(jsonData);

        using (UnityWebRequest webRequest = new UnityWebRequest(url, "POST"))
        {
            webRequest.uploadHandler = new UploadHandlerRaw(rawData);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");

            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                string responseText = webRequest.downloadHandler.text;
                Debug.Log("Tiempo jugado incrementado! Respuesta: " + responseText);
                OnStatsUpdated.Invoke(true);

                // Actualizar tiempo jugado formateado localmente
                if (usuarioActual != null)
                {
                    usuarioActual.AgregarTiempoJugado(segundos);
                }
            }
            else
            {
                Debug.LogError($"Error: {webRequest.error}");
                OnStatsUpdated.Invoke(false);
            }
        }
    }

    public void IncrementarVecesGanadas()
    {
        StartCoroutine(UpdateVecesGanadasCoroutine());
    }

    private IEnumerator UpdateVecesGanadasCoroutine()
    {
        string url = $"{apiUrl}/usuario/{usuarioId}/incrementar-veces-ganadas";

        if (usuarioId <= 0)
        {
            Debug.LogError("No hay usuario logueado para incrementar veces ganadas");
            OnStatsUpdated.Invoke(false);
            yield break;
        }

        using (UnityWebRequest webRequest = new UnityWebRequest(url, "POST"))
        {
            webRequest.SetRequestHeader("Content-Type", "application/json");
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Veces ganadas incrementadas!");
                OnStatsUpdated.Invoke(true);

                if (usuarioActual != null)
                {
                    usuarioActual.veces_ganadas += 1;
                }
            }
            else
            {
                Debug.LogError($"Error: {webRequest.error}");
                OnStatsUpdated.Invoke(false);
            }
        }
    }

    public void ActualizarUltimaPartida(string fecha)
    {
        StartCoroutine(ActualizarUltimaPartidaCoroutine(fecha));
    }

    private IEnumerator ActualizarUltimaPartidaCoroutine(string fecha)
    {
        string url = $"{apiUrl}/usuario/{usuarioId}/ultima-conexion";

        if (usuarioId <= 0)
        {
            Debug.LogError("No hay usuario logueado para actualizar la �ltima partida");
            yield break;
        }

        string jsonData = JsonUtility.ToJson(new UltimaPartidaPayload { ultima_partida = fecha });
        byte[] rawData = System.Text.Encoding.UTF8.GetBytes(jsonData);

        using (UnityWebRequest webRequest = new UnityWebRequest(url, "PUT"))
        {
            webRequest.uploadHandler = new UploadHandlerRaw(rawData);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                // Mostrar la respuesta del servidor
                string responseText = webRequest.downloadHandler.text;
                Debug.Log($"Respuesta del servidor (actualizaci�n de �ltima partida): {responseText}");
            }
            else
            {
                Debug.LogError($"Error al actualizar �ltima partida: {webRequest.error}");
                Debug.LogError($"C�digo de respuesta HTTP: {webRequest.responseCode}");
            }
        }

    }


    // ==================== CLASES AUXILIARES ====================
    [System.Serializable]
    private class UsuarioRegistroPayload
    {
        public string nombre_usuario;
        public string contrase�a;
    }

    [System.Serializable]
    private class TiempoJugadoPayload
    {
        public int tiempo_jugado;
    }

    [System.Serializable]
    private class UsuarioLoginPayload
    {
        public string nombre_usuario;
        public string contrase�a;
    }

    [System.Serializable]
    private class UltimaPartidaPayload
    {
        public string ultima_partida;
    }

    // M�todo para obtener el ID del usuario actual
    public int GetUsuarioId()
    {
        return usuarioId;
    }
}