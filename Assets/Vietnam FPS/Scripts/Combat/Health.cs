
using UnityEngine;

public class Health : MonoBehaviour
{
    public float maxHealth = 100f;
    [HideInInspector] public float current;
    public bool destroyOnDeath = false;

    public System.Action OnDeath;

    public bool IsDead => current <= 0f;

    void Awake() { current = maxHealth; }

    public void Damage(float amt)
    {
        if (IsDead) return;
        current = Mathf.Max(0f, current - amt);
        if (current <= 0f)
        {
            OnDeath?.Invoke();
            if (destroyOnDeath) Destroy(gameObject);
            enabled = false;
        }
    }

    public void Heal(float amt)
    {
        if (IsDead) return;
        current = Mathf.Min(current + amt, maxHealth);
    }

    public void ResetHealth()
    {
        current = maxHealth;
        enabled = true;
    }
}
