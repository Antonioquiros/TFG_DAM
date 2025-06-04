using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class LoginController : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;
    public Button loginButton;
    public Button backButton;

    public PopupController popupController;

    private APIClient apiClient;
    private bool isProcessing = false;

    private void Awake()
    {
        apiClient = FindObjectOfType<APIClient>();
        if (apiClient == null)
        {
            GameObject apiClientObj = new GameObject("APIClient");
            apiClient = apiClientObj.AddComponent<APIClient>();
            DontDestroyOnLoad(apiClientObj);
        }
    }

    private void Start()
    {
        loginButton.onClick.AddListener(OnLoginClicked);
        backButton.onClick.AddListener(OnBackClicked);
    }

    public void OnLoginClicked()
    {
        if (isProcessing) return;

        string username = usernameInput.text.Trim();
        string password = passwordInput.text;

        if (ValidateInputs(username, password))
        {
            isProcessing = true;;
            loginButton.interactable = false;

            apiClient.OnLoginSuccess.RemoveAllListeners();
            apiClient.OnLoginFailed.RemoveAllListeners();

            apiClient.OnLoginSuccess.AddListener(HandleLoginSuccess);
            apiClient.OnLoginFailed.AddListener(HandleLoginFailed);

            apiClient.IniciarSesion(username, password);
        }
    }

    private bool ValidateInputs(string username, string password)
    {
        if (string.IsNullOrEmpty(username))
        {
            popupController.ShowPopup("El nombre de usuario no puede estar vacio");
            return false;
        }
        if ( string.IsNullOrEmpty(password))
        {
            popupController.ShowPopup("La contrasena no puede estar vacia");
            return false;
        }

        return true;
    }

    private void HandleLoginSuccess(Usuario usuario)
    {
        Debug.Log("Login exitoso: " + usuario.nombre_usuario);

        // Limpiar listeners
        apiClient.OnLoginSuccess.RemoveListener(HandleLoginSuccess);
        apiClient.OnLoginFailed.RemoveListener(HandleLoginFailed);

        isProcessing = false;
        loginButton.interactable = true;

        // Guardar datos del usuario si es necesario
        PlayerPrefs.SetInt("UserId", usuario.id_usuario);
        PlayerPrefs.SetString("Username", usuario.nombre_usuario);
        PlayerPrefs.Save();

        // ACTUALIZAR CAMPO "ultima_partida"
        string fechaActual = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        usuario.ultima_partida = fechaActual;
        // Actualizar la última partida en el servidor
        apiClient.ActualizarUltimaPartida(fechaActual);  // Llamada al servidor para actualizar la fecha



        // Cargar escena principal del juego
        SceneManager.LoadScene("Menu");
    }

    private void HandleLoginFailed(string error)
    {
        Debug.LogError("Error de login: " + error);
        popupController.ShowPopup("Usuario o contrasena incorrectos");

        // Limpiar listeners
        apiClient.OnLoginSuccess.RemoveListener(HandleLoginSuccess);
        apiClient.OnLoginFailed.RemoveListener(HandleLoginFailed);

        isProcessing = false;
        loginButton.interactable = true;



        // NO cargar la escena del juego en caso de error
    }

    private void OnBackClicked()
    {
        SceneManager.LoadScene("InitialScene");
    }

    private void OnDestroy()
    {
        if (apiClient != null)
        {
            apiClient.OnLoginSuccess.RemoveListener(HandleLoginSuccess);
            apiClient.OnLoginFailed.RemoveListener(HandleLoginFailed);
        }
    }
}