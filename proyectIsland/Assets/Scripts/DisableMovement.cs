using UnityEngine;

public class DisableMovementInMenu : MonoBehaviour
{
    public GameObject locomotionSystem;

    void Start()
    {
        if (locomotionSystem != null)
            locomotionSystem.SetActive(false);
    }
}
