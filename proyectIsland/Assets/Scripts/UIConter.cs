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

    // Esta funcin ser llamada automticamente por el evento
    public void UpdateCounterText(int newCount)
    {
        int max = ChecklistManager.Instance ? ChecklistManager.Instance.totalPhotosNeeded : 6;
        counterText.text =  newCount + "/ " + max + " ";
    }

    private void OnDestroy()
    {
        // Buena pr�ctica: desconectar el listener cuando el objeto se destruye
        if (ChecklistManager.Instance != null)
        {
            ChecklistManager.Instance.OnPhotoCountChanged.RemoveListener(UpdateCounterText);
        }
    }
}