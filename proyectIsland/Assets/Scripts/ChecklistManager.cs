using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ChecklistManager : MonoBehaviour
{
    public static ChecklistManager Instance { get; private set; }
    public Dictionary<string, bool> photoChecklist = new Dictionary<string, bool>();
    public int photographedCount { get; private set; } = 0;
    public UnityEvent<int> OnPhotoCountChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            if (OnPhotoCountChanged == null) OnPhotoCountChanged = new UnityEvent<int>();
        }
        InitializeChecklist();
    }

    void InitializeChecklist()
    {
        PhotographableObject[] allObjects = FindObjectsOfType<PhotographableObject>();
        foreach (var obj in allObjects)
        {
            if (!string.IsNullOrEmpty(obj.objectName) && !photoChecklist.ContainsKey(obj.objectName))
            {
                photoChecklist.Add(obj.objectName, false);
            }
        }
        PrintChecklistStatus();
    }

    public bool IsGroupComplete(string objectName)
    {
        if (photoChecklist.ContainsKey(objectName)) return photoChecklist[objectName];
        return false;
    }

    public void MarkAsPhotographed(string objectName)
    {
        if (photoChecklist.ContainsKey(objectName) && !photoChecklist[objectName])
        {
            photoChecklist[objectName] = true;
            photographedCount++;
            Debug.Log(objectName + " completado. Bloqueando fotos para este grupo...");

            OnPhotoCountChanged?.Invoke(photographedCount);

            // --- AQUÍ ESTÁ LA MAGIA ---
            // Buscamos TODOS los animales y desactivamos a los de este grupo
            UpdateAllAnimalsPhysics(objectName, false);

            PrintChecklistStatus();
        }
    }

    public void ResetChecklist()
    {
        photographedCount = 0;
        var keys = new List<string>(photoChecklist.Keys);
        foreach (var key in keys) photoChecklist[key] = false;

        // Reactivamos TODOS los animales para que se puedan fotografiar de nuevo
        PhotographableObject[] allObjects = FindObjectsOfType<PhotographableObject>();
        foreach (var obj in allObjects)
        {
            obj.SetPhotographableState(true);
        }

        OnPhotoCountChanged?.Invoke(photographedCount);
        Debug.Log("¡CHECKLIST RESETEADO!");
    }

    // Función auxiliar para cambiar el estado físico de los animales
    void UpdateAllAnimalsPhysics(string targetName, bool canPhotograph)
    {
        PhotographableObject[] allObjects = FindObjectsOfType<PhotographableObject>();
        foreach (var obj in allObjects)
        {
            // Si el animal se llama igual al que acabamos de fotografiar (ej: "Tigre")
            if (obj.objectName == targetName)
            {
                // Le decimos que cambie su capa/tag
                obj.SetPhotographableState(canPhotograph);
            }
        }
    }
    void PrintChecklistStatus()
    {
        Debug.Log("--- ESTADO DEL CHECKLIST ---");
        Debug.Log("Total Fotografiado: " + photographedCount + "/" + photoChecklist.Count);
        foreach (var item in photoChecklist)
        {
            Debug.Log(item.Key + ": " + (item.Value ? "Fotografiado" : "Pendiente"));
        }
        Debug.Log("--------------------------");
    }
}