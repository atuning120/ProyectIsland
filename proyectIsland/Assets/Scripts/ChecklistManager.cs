using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement; // Necesario para cargar escenas (V1)

public class ChecklistManager : MonoBehaviour
{
    public static ChecklistManager Instance { get; private set; }

    // Diccionario para controlar qué animales ya fueron fotografiados
    public Dictionary<string, bool> photoChecklist = new Dictionary<string, bool>();

    public int photographedCount { get; private set; } = 0;
    public int totalPhotosNeeded = 6; // Meta de fotos (V1)

    // Evento para actualizar la UI general (ej: "3/6")
    public UnityEvent<int> OnPhotoCountChanged;

    // Evento para avisar al Tutorial qué objeto específico se fotografió (V2)
    public UnityEvent<string> OnSpecificPhotoTaken;

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

            // Inicializamos eventos si están vacíos
            if (OnPhotoCountChanged == null) OnPhotoCountChanged = new UnityEvent<int>();
            if (OnSpecificPhotoTaken == null) OnSpecificPhotoTaken = new UnityEvent<string>();
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

    /// <summary>
    /// Marca un objeto como fotografiado.
    /// </summary>
    /// <param name="objectName">El nombre del animal (ej: "Tigre")</param>
    /// <param name="debeSumar">Si es TRUE, aumenta el contador (1/6). Si es FALSE, solo marca el check (para tutoriales).</param>
    public void MarkAsPhotographed(string objectName, bool debeSumar = true)
    {
        if (photoChecklist.ContainsKey(objectName) && !photoChecklist[objectName])
        {
            // 1. Marcamos internamente que este grupo ya está listo
            photoChecklist[objectName] = true;

            // 2. SOLO aumentamos el número si es un animal real (lógica de V2)
            if (debeSumar)
            {
                photographedCount++;
            }

            Debug.Log(objectName + " completado. ¿Suma puntos?: " + debeSumar);

            // 3. Avisamos a los eventos (UI y Tutorial)
            OnPhotoCountChanged?.Invoke(photographedCount);
            OnSpecificPhotoTaken?.Invoke(objectName);

            // 4. Desactivamos las físicas de todos los animales de ese tipo
            UpdateAllAnimalsPhysics(objectName, false);

            PrintChecklistStatus();

            // 5. Verificar condición de victoria (Lógica de V1)
            // Solo ganamos si estamos sumando puntos y alcanzamos la meta
            if (debeSumar && photographedCount >= totalPhotosNeeded)
            {
                Debug.Log("¡Todas las fotos completadas! Cambiando a escena Descanso...");
                SceneManager.LoadScene("Descanso");
            }
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
            if (obj.objectName == targetName)
            {
                obj.SetPhotographableState(canPhotograph);
            }
        }
    }

    void PrintChecklistStatus()
    {
        Debug.Log("--- ESTADO DEL CHECKLIST ---");
        Debug.Log("Total Fotografiado: " + photographedCount + "/" + (totalPhotosNeeded > 0 ? totalPhotosNeeded.ToString() : photoChecklist.Count.ToString()));
        foreach (var item in photoChecklist)
        {
            Debug.Log(item.Key + ": " + (item.Value ? "Fotografiado" : "Pendiente"));
        }
        Debug.Log("--------------------------");
    }
}