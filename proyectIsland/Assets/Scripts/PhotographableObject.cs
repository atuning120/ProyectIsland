using UnityEngine;

public class PhotographableObject : MonoBehaviour
{
    public string objectName = "";
    public bool isPhotographed = false;

    // Guardamos los valores originales para poder resetear el juego si hace falta
    private int originalLayer;
    private string originalTag;

    private void Start()
    {
        // Recordamos qué capa y tag tenían al principio (ej: "Photographable")
        originalLayer = gameObject.layer;
        originalTag = gameObject.tag;

        // Verificación de seguridad por si olvidaste poner el nombre en el Inspector
        if (string.IsNullOrEmpty(objectName))
        {
            Debug.LogWarning("¡ATENCIÓN! El objeto " + gameObject.name + " no tiene 'objectName' configurado en el script.");
        }
    }

    public void OnPhotograph()
    {
        // Doble seguridad: Si el grupo ya está completo, no hacemos nada.
        if (ChecklistManager.Instance.IsGroupComplete(objectName))
        {
            return;
        }

        if (!isPhotographed)
        {
            isPhotographed = true;
            Debug.Log("¡Has fotografiado a: " + objectName + "!");
            ChecklistManager.Instance.MarkAsPhotographed(objectName);
        }
    }

    // --- NUEVA FUNCIÓN: Cambia la capa para que la cámara no lo detecte ---
    public void SetPhotographableState(bool canBePhotographed)
    {
        if (canBePhotographed)
        {
            // Restauramos su estado original (para reiniciar el juego)
            gameObject.layer = originalLayer;
            gameObject.tag = originalTag;
            isPhotographed = false;
        }
        else
        {
            // Lo cambiamos a la capa "Default" (generalmente la 0) y quitamos el Tag.
            // La mayoría de cámaras VR ignoran la capa Default o los objetos sin Tag.
            gameObject.layer = 0; // Capa "Default"
            gameObject.tag = "Untagged";
        }
    }
}