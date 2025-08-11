
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager I;

    void Awake()
    {
        if (I == null) I = this;
        else Destroy(gameObject);
    }

    public void SetPlayerControl(bool enabled)
    {
        var move = FindObjectOfType<PlayerMovement>(true);
        var look = FindObjectOfType<PlayerLook>(true);
        var gun = FindObjectOfType<Gun>(true);

        if (move) move.enabled = enabled;
        if (look) look.enabled = enabled;
        if (gun) gun.enabled = enabled;

        Cursor.lockState = enabled ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !enabled;
    }
}
