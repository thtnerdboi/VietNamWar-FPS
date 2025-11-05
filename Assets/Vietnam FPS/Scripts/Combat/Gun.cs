
using System;
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

    [Header("Impact FX")]
    [Tooltip("Optional prefab spawned at the raycast hit point.")]
    public GameObject hitEffectPrefab;
    [Tooltip("Lifetime of spawned impact effects in seconds. Ignored for persistent prefabs.")]
    public float hitEffectLifetime = 2f;
    [Tooltip("Overall scale applied to the runtime-generated fallback impact decal.")]
    public float hitEffectScale = 0.15f;
    [Tooltip("Tint applied to the fallback impact decal.")]
    public Color hitEffectColor = new Color(1f, 0.6f, 0.2f, 0.9f);

    public event Action ShotFired;
    public event Action TargetHit;

    float nextFire;
    int ammo;
    bool reloading;
    Material fallbackImpactMaterial;

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
        ShotFired?.Invoke();

        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, range, hitMask))
        {
            var h = hit.collider.GetComponentInParent<Health>();
            if (h != null) h.Damage(damage);
            SpawnImpactEffect(hit);
            TargetHit?.Invoke();
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

    void SpawnImpactEffect(RaycastHit hit)
    {
        if (hitEffectPrefab)
        {
            var instance = Instantiate(hitEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal));
            if (hitEffectLifetime > 0f)
            {
                Destroy(instance, hitEffectLifetime);
            }
            return;
        }

        var decal = GameObject.CreatePrimitive(PrimitiveType.Quad);
        decal.name = "GunImpact (Generated)";
        decal.transform.SetPositionAndRotation(hit.point + hit.normal * 0.01f, Quaternion.LookRotation(hit.normal));
        decal.transform.localScale = Vector3.one * Mathf.Max(0.001f, hitEffectScale);
        if (hit.transform) decal.transform.SetParent(hit.transform, true);

        var renderer = decal.GetComponent<Renderer>();
        if (renderer != null)
        {
            var material = GetFallbackImpactMaterial();
            if (material != null)
            {
                renderer.sharedMaterial = material;
            }
        }

        var collider = decal.GetComponent<Collider>();
        if (collider != null)
        {
            Destroy(collider);
        }

        if (hitEffectLifetime > 0f)
        {
            Destroy(decal, hitEffectLifetime);
        }
    }

    Material GetFallbackImpactMaterial()
    {
        if (fallbackImpactMaterial == null)
        {
            var shader = Shader.Find("Unlit/Color");
            if (shader == null)
            {
                shader = Shader.Find("Legacy Shaders/Particles/Alpha Blended");
            }

            if (shader == null)
            {
                shader = Shader.Find("Universal Render Pipeline/Lit");
            }

            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            if (shader == null)
            {
                shader = Shader.Find("Sprites/Default");
            }

            if (shader != null)
            {
                fallbackImpactMaterial = new Material(shader);
            }
            else
            {
                return null;
            }
        }

        fallbackImpactMaterial.color = hitEffectColor;
        return fallbackImpactMaterial;
    }
}
