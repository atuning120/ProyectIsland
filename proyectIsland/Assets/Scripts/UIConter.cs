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
            // Actualizamos el texto inicial al arrancar
            UpdateCounterText(ChecklistManager.Instance.photographedCount);
        }
    }

    // Esta función será llamada automáticamente por el evento
    public void UpdateCounterText(int newCount)
    {
        // Obtenemos el máximo desde el Manager. Si no existe el manager por error, usamos 6 por defecto.
        int max = ChecklistManager.Instance ? ChecklistManager.Instance.totalPhotosNeeded : 6;

        // Formato: "1 / 6"
        counterText.text = newCount + " / " + max;
    }

    private void OnDestroy()
    {
        // Buena práctica: desconectar el listener cuando el objeto se destruye para evitar errores
        if (ChecklistManager.Instance != null)
        {
            ChecklistManager.Instance.OnPhotoCountChanged.RemoveListener(UpdateCounterText);
        }
    }
}