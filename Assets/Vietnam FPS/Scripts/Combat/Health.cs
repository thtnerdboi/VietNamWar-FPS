
using UnityEngine;

public class Health : MonoBehaviour
{
    public float maxHealth = 100f;
    [HideInInspector] public float current;
    public bool destroyOnDeath = false;

    public System.Action OnDeath;

    void Awake() { current = maxHealth; }

    public void Damage(float amt)
    {
        current -= amt;
        if (current <= 0f)
        {
            OnDeath?.Invoke();
            if (destroyOnDeath) Destroy(gameObject);
            enabled = false;
        }
    }
}
