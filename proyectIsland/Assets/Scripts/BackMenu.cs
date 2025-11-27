using UnityEngine;

public class BackButton : MonoBehaviour
{
    public GameObject menuPrincipal;
    public GameObject menuActual;

    public void AbrirConfig()
    {
        menuPrincipal.SetActive(false);
        menuActual.SetActive(true);
    }

  
}
