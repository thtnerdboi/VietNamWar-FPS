using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Specialises the generic <see cref="Gun"/> behaviour to represent an M16 rifle
/// and to automatically attach the appropriate model/textures when they exist in
/// the project.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Gun))]
public class Rifle : MonoBehaviour
{
    [Header("Configuration")]
    [Tooltip("Optional reference to the prefab for the M16 rifle model.")]
    public GameObject m16Prefab;

    [Tooltip("Optional materials that should be applied to the instantiated model.")]
    public Material[] m16Materials;

    [Tooltip("Optional texture resource paths that will be applied to the first material.")]
    public List<string> textureResourcePaths = new List<string>
    {
        "Weapons/M16/Textures/M16_BaseColor",
        "Weapons/M16/Textures/M16_Normal"
    };

    [Tooltip("Parent transform that will receive the instantiated rifle visuals.")]
    public Transform visualAnchor;

    [Header("Resources")] [Tooltip("Fallback resource path used to load the M16 prefab if the field above is empty.")]
    public string prefabResourcePath = "Weapons/M16/M16";

    Gun gun;
    GameObject visualInstance;
    Material placeholderMaterial;

    void Reset()
    {
        gun = GetComponent<Gun>();
        ApplyDefaultStats();
    }

    void Awake()
    {
        gun = GetComponent<Gun>();
        if (!visualAnchor) visualAnchor = transform;

        if (gun != null)
        {
            // Ensure the gun stats match an M16 profile if they are unset in the inspector.
            ApplyDefaultStats();
        }

        TryAttachVisuals();
    }

    void ApplyDefaultStats()
    {
        if (gun == null) return;
        gun.damage = Mathf.Max(gun.damage, 34f);
        gun.range = Mathf.Max(gun.range, 120f);
        gun.fireRate = Mathf.Max(gun.fireRate, 10f);
        gun.magSize = Mathf.Max(gun.magSize, 30);
        gun.reloadTime = Mathf.Max(gun.reloadTime, 2.0f);
    }

    void TryAttachVisuals()
    {
        if (visualInstance != null) return;

        var prefab = m16Prefab != null ? m16Prefab : Resources.Load<GameObject>(prefabResourcePath);
        if (prefab != null)
        {
            visualInstance = Instantiate(prefab, visualAnchor, false);
            ApplyMaterials(visualInstance);
            return;
        }

        Debug.LogWarning($"Rifle: Could not locate an M16 prefab at '{prefabResourcePath}'. " +
                         "Using a generated placeholder model instead.");
        visualInstance = CreatePlaceholderModel();
    }

    void ApplyMaterials(GameObject target)
    {
        if (target == null) return;

        var renderers = target.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0) return;

        if (m16Materials != null && m16Materials.Length > 0)
        {
            foreach (var renderer in renderers)
            {
                renderer.sharedMaterials = m16Materials;
            }
            return;
        }

        if (textureResourcePaths == null || textureResourcePaths.Count == 0) return;

        var mainRenderer = renderers[0];
        var materials = mainRenderer.sharedMaterials;
        if (materials == null || materials.Length == 0) return;

        var material = new Material(materials[0]);
        bool appliedTexture = false;
        foreach (var path in textureResourcePaths)
        {
            if (string.IsNullOrEmpty(path)) continue;
            var texture = Resources.Load<Texture2D>(path);
            if (texture == null) continue;

            // Use the first texture as the base map, additional textures are added as detail maps.
            if (!appliedTexture)
            {
                material.mainTexture = texture;
                appliedTexture = true;
            }
            else
            {
                material.SetTexture(Shader.PropertyToID("_DetailAlbedoMap"), texture);
            }
        }

        if (!appliedTexture) return;

        materials[0] = material;
        mainRenderer.sharedMaterials = materials;
    }

    GameObject CreatePlaceholderModel()
    {
        if (visualAnchor == null) visualAnchor = transform;

        var root = new GameObject("M16_Placeholder");
        root.transform.SetParent(visualAnchor, false);

        CreatePart(root.transform, "Receiver", new Vector3(0.06f, 0.08f, 0.55f), new Vector3(0f, -0.03f, -0.05f));
        CreatePart(root.transform, "Barrel", new Vector3(0.03f, 0.03f, 0.55f), new Vector3(0f, 0.01f, 0.3f));
        CreatePart(root.transform, "Stock", new Vector3(0.08f, 0.08f, 0.35f), new Vector3(0f, -0.03f, -0.45f));
        CreatePart(root.transform, "Grip", new Vector3(0.05f, 0.12f, 0.1f), new Vector3(0f, -0.1f, -0.05f));
        CreatePart(root.transform, "CarryHandle", new Vector3(0.04f, 0.06f, 0.3f), new Vector3(0f, 0.05f, -0.05f));

        return root;
    }

    void CreatePart(Transform parent, string name, Vector3 scale, Vector3 localPosition)
    {
        var part = GameObject.CreatePrimitive(PrimitiveType.Cube);
        part.name = name;
        part.transform.SetParent(parent, false);
        part.transform.localScale = scale;
        part.transform.localPosition = localPosition;

        var collider = part.GetComponent<Collider>();
        if (collider != null)
        {
            Destroy(collider);
        }

        var renderer = part.GetComponent<Renderer>();
        if (renderer != null)
        {
            var material = GetPlaceholderMaterial();
            if (material != null)
            {
                renderer.sharedMaterial = material;
            }
        }
    }

    Material GetPlaceholderMaterial()
    {
        if (placeholderMaterial != null) return placeholderMaterial;

        var shader = Shader.Find("Unlit/Color");
        if (shader == null)
        {
            shader = Shader.Find("Legacy Shaders/Diffuse");
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
            placeholderMaterial = new Material(shader)
            {
                color = new Color(0.12f, 0.12f, 0.12f, 1f)
            };
        }

        return placeholderMaterial;
    }
}
