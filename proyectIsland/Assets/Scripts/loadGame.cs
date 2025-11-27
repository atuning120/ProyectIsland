using UnityEngine;
using UnityEngine.SceneManagement; // Necesario para manejar las escenas.

public class SceneChanger : MonoBehaviour
{
    // Función para cargar la escena por nombre
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
