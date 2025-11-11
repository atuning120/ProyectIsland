// PhotographableObject.cs
using UnityEngine;

public class PhotographableObject : MonoBehaviour
{
    public string objectName = ""; // Nombre para mostrar en el checklist
    public bool isPhotographed = false;

    // Esta función será llamada por la cámara cuando se tome la foto
    public void OnPhotograph()
    {
        if (!isPhotographed)
        {
            isPhotographed = true;
            Debug.Log("¡Has fotografiado a: " + objectName + "!");

            // Aquí notificamos al gestor del checklist
            ChecklistManager.Instance.MarkAsPhotographed(objectName);
        }
    }

    // NUEVO: Método para resetear el estado del objeto
    public void ResetPhotographState()
    {
        isPhotographed = false;
        Debug.Log(objectName + " ha sido reseteado y ya no está fotografiado.");
    }
}