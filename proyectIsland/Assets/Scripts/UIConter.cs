// UICounter.cs
using UnityEngine;
using TMPro; // Necesario para TextMeshPro

[RequireComponent(typeof(TextMeshProUGUI))]
public class UICounter : MonoBehaviour
{
    private TextMeshProUGUI counterText;

    void Start()
    {
        counterText = GetComponent<TextMeshProUGUI>();

        // Conectamos este script al evento del ChecklistManager
        if (ChecklistManager.Instance != null)
        {
            ChecklistManager.Instance.OnPhotoCountChanged.AddListener(UpdateCounterText);
            // Actualizamos el texto inicial
            UpdateCounterText(ChecklistManager.Instance.photographedCount);
        }
    }

    // Esta función será llamada automáticamente por el evento
    public void UpdateCounterText(int newCount)
    {
        counterText.text =  newCount + "/ 10 " ;
    }

    private void OnDestroy()
    {
        // Buena práctica: desconectar el listener cuando el objeto se destruye
        if (ChecklistManager.Instance != null)
        {
            ChecklistManager.Instance.OnPhotoCountChanged.RemoveListener(UpdateCounterText);
        }
    }
}