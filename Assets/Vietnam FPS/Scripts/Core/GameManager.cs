
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager I;

    [Header("UI")]
    [SerializeField]
    Text messageText;

    Coroutine messageRoutine;

    void Awake()
    {
        if (I == null) I = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (messageText)
        {
            messageText.gameObject.SetActive(false);
        }
    }

    public void SetPlayerControl(bool enabled)
    {
#if UNITY_2023_1_OR_NEWER
        var move = FindFirstObjectByType<PlayerMovement>(FindObjectsInactive.Include);
        var look = FindFirstObjectByType<PlayerLook>(FindObjectsInactive.Include);
        var gun = FindFirstObjectByType<Gun>(FindObjectsInactive.Include);
#else
        var move = FindObjectOfType<PlayerMovement>(true);
        var look = FindObjectOfType<PlayerLook>(true);
        var gun = FindObjectOfType<Gun>(true);
#endif

        if (move) move.enabled = enabled;
        if (look) look.enabled = enabled;
        if (gun) gun.enabled = enabled;

        Cursor.lockState = enabled ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !enabled;
    }

    public void ShowMessage(string message, float duration = 3f)
    {
        if (!messageText)
        {
            Debug.LogWarning($"No messageText assigned on {nameof(GameManager)}. Message: {message}");
            return;
        }

        messageText.gameObject.SetActive(true);
        messageText.text = message;

        if (messageRoutine != null)
        {
            StopCoroutine(messageRoutine);
        }

        if (duration > 0f)
        {
            messageRoutine = StartCoroutine(HideMessageAfter(duration));
        }
        else
        {
            messageRoutine = null;
        }
    }

    private IEnumerator HideMessageAfter(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (messageText)
        {
            messageText.gameObject.SetActive(false);
        }

        messageRoutine = null;
    }
}
