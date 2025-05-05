using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class controlBotones : MonoBehaviour
{
    public void onBotonJugar() {
        SceneManager.LoadScene("Nivel1.1");
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

    public void onBotonVamos2()
    {
        SceneManager.LoadScene("Nivel1Boss");// Cambiar al nivel 2
    }

    public void OnBotonGulag()
    {
        SceneManager.LoadScene("Gulag");
    }

    public void OnVolverNivel() {
        string nivelOriginal = PlayerPrefs.GetString("UltimoNivel", "Nivel1.1");
        SceneManager.LoadScene(nivelOriginal);
    }
    public void onBotonSalir()
    {
        Application.Quit();
    }
}
