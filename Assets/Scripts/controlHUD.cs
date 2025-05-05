using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class controlHUD : MonoBehaviour
{
    public TextMeshProUGUI vidasTxt;
    public TextMeshProUGUI tiempoTxt;
    public TextMeshProUGUI llavesTxt;

    public void setVidasTxt(float vidas) {
        vidasTxt.text = "Vidas: " + vidas;
    }

    public void setTiempoTxt(int tiempo)
    {
        int segundos = tiempo % 60;
        int minutos = tiempo / 60;
       tiempoTxt.text = minutos.ToString("00") + ":" + segundos.ToString("00");
    }

    public void setLlavesTxt(int llaves)
    {
        llavesTxt.text = "Llaves: "+llaves;
    }
}
