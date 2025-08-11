using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Health))]
public class PlayerHealth : MonoBehaviour
{
    Health health;

    void Awake()
    {
        health = GetComponent<Health>();
        health.OnDeath += HandleDeath;
    }

    void HandleDeath()
    {
        GameManager.I?.SetPlayerControl(false);
        Invoke(nameof(ReloadLevel), 3f);
    }

    void ReloadLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}

