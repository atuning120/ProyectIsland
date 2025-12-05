using UnityEngine;

public class DiarioSeguidorJugador : MonoBehaviour
{
    public Transform libro;   // El libro
    public Transform camara;  // Main Camera del XR Origin
    private Vector3 offset;   // Posición inicial relativa al jugador

    void Start()
    {
        // Guardamos la posición inicial relativa al jugador
        offset = libro.position - camara.position;
    }

    void Update()
    {
        // Mantener el offset inicial relativo a la posición real del jugador
        libro.position = camara.position + offset;
    }
}
