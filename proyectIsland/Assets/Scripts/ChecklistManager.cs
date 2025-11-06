// ChecklistManager.cs (Modificado)
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events; // NUEVO: Necesario para usar UnityEvent

public class ChecklistManager : MonoBehaviour
{
    // Singleton: Un patrón para tener una única instancia de este gestor
    public static ChecklistManager Instance { get; private set; }

    // Usamos un diccionario para llevar la cuenta de los objetos por su nombre
    public Dictionary<string, bool> photoChecklist = new Dictionary<string, bool>();

    // NUEVO: Variable para llevar la cuenta de cuántos objetos se han fotografiado.
    // El "private set" significa que solo este script puede cambiar su valor, pero otros pueden leerlo.
    public int photographedCount { get; private set; } = 0;

    // NUEVO: Un evento que se disparará cada vez que el contador cambie.
    // La UI se "suscribirá" a este evento para saber cuándo debe actualizarse.
    public UnityEvent<int> OnPhotoCountChanged;

    private void Awake()
    {
        // Lógica del Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Opcional: para que no se destruya al cambiar de escena

            // NUEVO: Es buena práctica inicializar el evento aquí.
            if (OnPhotoCountChanged == null)
                OnPhotoCountChanged = new UnityEvent<int>();
        }

        InitializeChecklist();
    }

    // Busca todos los objetos fotografiables en la escena y los añade a la lista
    void InitializeChecklist()
    {
        PhotographableObject[] allObjects = FindObjectsOfType<PhotographableObject>();
        foreach (var obj in allObjects)
        {
            if (!photoChecklist.ContainsKey(obj.objectName))
            {
                photoChecklist.Add(obj.objectName, false);
            }
        }
        PrintChecklistStatus();
    }

    // Método público para que otros scripts actualicen el checklist
    public void MarkAsPhotographed(string objectName)
    {
        // NUEVO: Hemos añadido "&& !photoChecklist[objectName]" para asegurarnos de que solo contamos
        // un objeto la PRIMERA vez que se fotografía, evitando errores si se le toma foto dos veces.
        if (photoChecklist.ContainsKey(objectName) && !photoChecklist[objectName])
        {
            photoChecklist[objectName] = true;

            // NUEVO: Incrementamos el contador.
            photographedCount++;

            Debug.Log(objectName + " marcado como fotografiado. Total de fotos: " + photographedCount);

            // NUEVO: Disparamos el evento y le pasamos el nuevo valor del contador.
            // El "?." es una comprobación de seguridad para no ejecutarlo si nadie está escuchando.
            OnPhotoCountChanged?.Invoke(photographedCount);

            PrintChecklistStatus();
        }
    }

    // Función de ayuda para ver el estado en la consola
    void PrintChecklistStatus()
    {
        Debug.Log("--- ESTADO DEL CHECKLIST ---");
        Debug.Log("Total Fotografiado: " + photographedCount + "/" + photoChecklist.Count); // NUEVO: Mensaje más informativo
        foreach (var item in photoChecklist)
        {
            Debug.Log(item.Key + ": " + (item.Value ? "Fotografiado" : "Pendiente"));
        }
        Debug.Log("--------------------------");
    }
}