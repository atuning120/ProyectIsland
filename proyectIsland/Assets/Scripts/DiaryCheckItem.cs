using UnityEngine;
using UnityEngine.UI;

public class DiaryCheckItem : MonoBehaviour
{
    public string objectName;        // Debe coincidir con PhotographableObject.objectName
    public Image checkIcon;          // La imagen del check en tu UI

    void Start()
    {
        // Se esconde el check al empezar
        checkIcon.enabled = false;

        // Cada vez que el checklist cambie, revisa si este objeto debe activarse
        ChecklistManager.Instance.OnPhotoCountChanged.AddListener(OnChecklistUpdated);

        // Por si ya fue fotografiado antes de abrir el diario
        RefreshState();
    }

    void OnChecklistUpdated(int _)
    {
        RefreshState();
    }

    void RefreshState()
    {
        if (ChecklistManager.Instance.photoChecklist.ContainsKey(objectName))
        {
            bool photographed = ChecklistManager.Instance.photoChecklist[objectName];
            checkIcon.enabled = photographed;
        }
    }
}
