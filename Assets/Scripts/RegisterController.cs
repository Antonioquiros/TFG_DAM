using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class RegisterController : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;
    public TMP_InputField confirmPasswordInput;
    public Button registerButton;
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
        registerButton.onClick.AddListener(OnRegisterClicked);
        backButton.onClick.AddListener(OnBackClicked);
    }

    public void OnRegisterClicked()
    {
        if (isProcessing) return;

        string username = usernameInput.text.Trim();
        string password = passwordInput.text;
        string confirmPassword = confirmPasswordInput.text;

        if (ValidateInputs(username, password, confirmPassword))
        {
            isProcessing = true;
            registerButton.interactable = false;

            // IMPORTANTE: Eliminar cualquier listener anterior para evitar duplicados
            apiClient.OnRegisterSuccess.RemoveAllListeners();
            apiClient.OnRegisterFailed.RemoveAllListeners();

            apiClient.OnRegisterSuccess.AddListener(HandleRegisterSuccess);
            apiClient.OnRegisterFailed.AddListener(HandleRegisterFailed);

            Debug.Log("Intentando registrar usuario: " + username);
            apiClient.RegistrarUsuario(username, password);
        }
    }

    private bool ValidateInputs(string username, string password, string confirmPassword)
    {
        if (string.IsNullOrEmpty(username))
        {
            popupController.ShowPopup("El nombre de usuario no puede estar vacio");
            return false;
        }

        if (string.IsNullOrEmpty(password))
        {
            popupController.ShowPopup("La contrasena no puede estar vacia");
            return false;
        }

        if (password != confirmPassword)
        {
            popupController.ShowPopup("Las contrasenas no coinciden");
            return false;
        }

        return true;
    }

    private void HandleRegisterSuccess(Usuario usuario)
    {
        Debug.Log("Registro exitoso: " + usuario.nombre_usuario);

        // Limpiar listeners
        apiClient.OnRegisterSuccess.RemoveListener(HandleRegisterSuccess);
        apiClient.OnRegisterFailed.RemoveListener(HandleRegisterFailed);

        isProcessing = false;
        registerButton.interactable = true;

        // Mostrar mensaje de éxito y volver al login
        PlayerPrefs.SetString("RegistrationSuccess", "¡Registro exitoso! Por favor inicia sesión");
        SceneManager.LoadScene("LogScene");
    }

    private void HandleRegisterFailed(string error)
    {
        Debug.LogError("Error de registro: " + error);

        // Limpiar listeners
        apiClient.OnRegisterSuccess.RemoveListener(HandleRegisterSuccess);
        apiClient.OnRegisterFailed.RemoveListener(HandleRegisterFailed);

        isProcessing = false;
        registerButton.interactable = true;

        if (error.Contains("already exists"))
        {
            popupController.ShowPopup("El nombre de usuario ya esta en uso");
        }
        else
        {
            popupController.ShowPopup("Error en el registro");
        }
    }

    private void OnBackClicked()
    {
        SceneManager.LoadScene("InitialScene");
    }

    private void OnDestroy()
    {
        if (apiClient != null)
        {
            apiClient.OnRegisterSuccess.RemoveListener(HandleRegisterSuccess);
            apiClient.OnRegisterFailed.RemoveListener(HandleRegisterFailed);
        }
    }
}