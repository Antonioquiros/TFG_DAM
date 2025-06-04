using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PopupController : MonoBehaviour
{
    public GameObject popupPanel;
    public TextMeshProUGUI popupText;
    public Button closeButton;

    private void Start()
    {
        popupPanel.SetActive(false);
        closeButton.onClick.AddListener(() => popupPanel.SetActive(false));
    }

    public void ShowPopup(string message)
    {
        popupText.text = message;
        popupPanel.SetActive(true);
    }
}
