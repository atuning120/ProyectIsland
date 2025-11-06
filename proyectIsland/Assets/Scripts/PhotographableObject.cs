// PhotographableObject.cs
using UnityEngine;

public class PhotographableObject : MonoBehaviour
{
    public string objectName = "Tigre"; // Nombre para mostrar en el checklist
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
}