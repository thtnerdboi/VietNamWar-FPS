using UnityEngine;

/// <summary>
/// Draws a dynamic crosshair in the centre of the screen and reacts to
/// <see cref="Gun"/> events for basic shooting feedback.
/// </summary>
[DefaultExecutionOrder(500)]
public class CrosshairUI : MonoBehaviour
{
    [Header("Appearance")]
    [Tooltip("Base gap between each crosshair arm and the screen centre in pixels.")]
    public float baseGap = 8f;
    [Tooltip("Maximum additional expansion applied after a shot.")]
    public float shotExpansion = 14f;
    [Tooltip("Length of each crosshair arm in pixels.")]
    public float lineLength = 12f;
    [Tooltip("Thickness of each crosshair arm in pixels.")]
    public float lineThickness = 2f;
    [Tooltip("Default colour for the crosshair.")]
    public Color crosshairColor = new Color(1f, 1f, 1f, 0.85f);
    [Tooltip("Colour shown briefly when the shot hits a target.")]
    public Color hitColor = new Color(1f, 0.25f, 0.25f, 0.95f);

    [Header("Behaviour")]
    [Tooltip("How quickly the crosshair expands after each shot.")]
    public float expansionSpeed = 80f;
    [Tooltip("How quickly the crosshair returns to its base size.")]
    public float recoverySpeed = 120f;
    [Tooltip("Duration of the hit-colour flash in seconds.")]
    public float hitFlashDuration = 0.12f;
    [Tooltip("Automatically locate a gun in the scene if none is assigned.")]
    public bool autoAssignGun = true;
    [Tooltip("If true the crosshair is hidden whenever the OS cursor is visible.")]
    public bool hideWhenCursorVisible = true;

    [Header("References")]
    [Tooltip("Gun whose events drive the crosshair feedback.")]
    public Gun targetGun;

    Texture2D pixelTexture;
    float currentGap;
    float targetGap;
    float hitTimer;
    Gun subscribedGun;

    void Awake()
    {
        CreateTexture();
        currentGap = baseGap;
        targetGap = baseGap;
    }

    void OnEnable()
    {
        SubscribeToGun();
    }

    void OnDisable()
    {
        UnsubscribeFromGun();
    }

    void Update()
    {
        if (pixelTexture == null)
        {
            CreateTexture();
        }

        if (autoAssignGun && targetGun == null)
        {
#if UNITY_2023_1_OR_NEWER
            targetGun = FindFirstObjectByType<Gun>(FindObjectsInactive.Include);
#else
            targetGun = FindObjectOfType<Gun>(true);
#endif
            SubscribeToGun();
        }

        float gapDifference = targetGap - currentGap;
        float speed = gapDifference > 0f ? expansionSpeed : recoverySpeed;
        currentGap = Mathf.MoveTowards(currentGap, targetGap, speed * Time.deltaTime);
        targetGap = Mathf.MoveTowards(targetGap, baseGap, recoverySpeed * Time.deltaTime);

        if (hitTimer > 0f)
        {
            hitTimer -= Time.deltaTime;
        }
    }

    void OnGUI()
    {
        if (pixelTexture == null) return;
        if (hideWhenCursorVisible && Cursor.visible) return;
        if (targetGun != null && !targetGun.enabled) return;

        var originalColour = GUI.color;
        GUI.color = hitTimer > 0f ? hitColor : crosshairColor;

        float centreX = Screen.width * 0.5f;
        float centreY = Screen.height * 0.5f;
        float halfThickness = lineThickness * 0.5f;

        DrawHorizontal(centreX - currentGap - lineLength, centreY - halfThickness, lineLength);
        DrawHorizontal(centreX + currentGap, centreY - halfThickness, lineLength);
        DrawVertical(centreX - halfThickness, centreY - currentGap - lineLength, lineLength);
        DrawVertical(centreX - halfThickness, centreY + currentGap, lineLength);

        GUI.color = originalColour;
    }

    void DrawHorizontal(float x, float y, float length)
    {
        var rect = new Rect(x, y, length, lineThickness);
        GUI.DrawTexture(rect, pixelTexture);
    }

    void DrawVertical(float x, float y, float length)
    {
        var rect = new Rect(x, y, lineThickness, length);
        GUI.DrawTexture(rect, pixelTexture);
    }

    void SubscribeToGun()
    {
        if (targetGun == null || targetGun == subscribedGun) return;

        UnsubscribeFromGun();

        targetGun.ShotFired += HandleShotFired;
        targetGun.TargetHit += HandleTargetHit;
        subscribedGun = targetGun;
    }

    void UnsubscribeFromGun()
    {
        if (subscribedGun == null) return;
        subscribedGun.ShotFired -= HandleShotFired;
        subscribedGun.TargetHit -= HandleTargetHit;
        subscribedGun = null;
    }

    void HandleShotFired()
    {
        targetGap = Mathf.Clamp(targetGap + shotExpansion, baseGap, baseGap + shotExpansion);
    }

    void HandleTargetHit()
    {
        hitTimer = hitFlashDuration;
    }

    void CreateTexture()
    {
        if (pixelTexture != null) return;

        pixelTexture = new Texture2D(1, 1, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };
        pixelTexture.SetPixel(0, 0, Color.white);
        pixelTexture.Apply();
    }
}
