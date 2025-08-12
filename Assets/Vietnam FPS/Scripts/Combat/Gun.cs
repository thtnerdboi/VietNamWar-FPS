
using UnityEngine;

public class Gun : MonoBehaviour
{
    [Header("Stats")]
    public float damage = 34f;
    public float range = 120f;
    public float fireRate = 10f; // shots per second
    public int magSize = 30;
    public float reloadTime = 2.0f;

    [Header("Refs")]
    public Camera cam;
    public LayerMask hitMask;

    [Header("FX")]
    public ParticleSystem muzzleFlash;
    public AudioSource audioSrc;
    public AudioClip shotSFX;
    public AudioClip reloadSFX;

    float nextFire;
    int ammo;
    bool reloading;

    void Awake()
    {
        ammo = magSize;
        if (!cam) cam = Camera.main;
    }

    void Update()
    {
        // Fire
        if (Input.GetMouseButton(0)) TryFire();
        // Reload
        if (Input.GetKeyDown(KeyCode.R)) StartReload();
    }

    void TryFire()
    {
        if (Time.time < nextFire || reloading) return;
        if (ammo <= 0) { StartReload(); return; }

        nextFire = Time.time + 1f / fireRate;
        ammo--;

        if (muzzleFlash) muzzleFlash.Play();
        if (audioSrc && shotSFX) audioSrc.PlayOneShot(shotSFX);

        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, range, hitMask))
        {
            var h = hit.collider.GetComponentInParent<Health>();
            if (h != null) h.Damage(damage);
        }
    }

    void StartReload()
    {
        if (reloading || ammo == magSize) return;
        reloading = true;
        if (audioSrc && reloadSFX) audioSrc.PlayOneShot(reloadSFX);
        Invoke(nameof(FinishReload), reloadTime);
    }

    void FinishReload()
    {
        ammo = magSize;
        reloading = false;
    }
}
