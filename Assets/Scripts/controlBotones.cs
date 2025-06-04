using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class controlBotones : MonoBehaviour
{
    APIClient client;
    void Start()
    {
        if (client == null)
            client = FindObjectOfType<APIClient>();
    }

    public void onBotonJugar() {
        SceneManager.LoadScene("Nivel1.1");
    }

    public void onBotonControler()
    {
        SceneManager.LoadScene("ControlesScene");
    }
    public void onBotonHistoria()
    {
        SceneManager.LoadScene("Historia");
    }
    public void onBotonMenu()
    {
        SceneManager.LoadScene("Menu");
    }
    public void onBotonVamos() {
        SceneManager.LoadScene("Nivel1Boss");
    }

    public void onBotonBoss2()
    {
        SceneManager.LoadScene("Nivel2Boss");
    }

    public void onBotonVamos2()
    {
        SceneManager.LoadScene("Nivel2.1");
    }

    public void OnBotonGulag()
    {
        SceneManager.LoadScene("Gulag");
    }
    public void OnBotonInfo() {
        SceneManager.LoadScene("InfoScene");
    }
    public void OnBotonStats() {
        SceneManager.LoadScene("StatsScene");
    }

    public void OnVolverNivel1() {
        SceneManager.LoadScene("Nivel1.1");
    }
    public void onBotonSalir()
    {
        Application.Quit();
    }
    public void GoToLoginScene() {
        SceneManager.LoadScene("LogScene");
    }
    public void GoToRegisterScene()
    {
        SceneManager.LoadScene("RegistroScene");
    }
    public void onVolverLog() {
        SceneManager.LoadScene("InitialScene");
    }
    public void LogOut()
    {
        if (client == null)
            client = FindObjectOfType<APIClient>();

        if (client != null)
        {
            client.CerrarSesion(); 
        }
        else
        {
            Debug.LogError("APIClient no encontrado al cerrar sesión.");
        }
    }

}
