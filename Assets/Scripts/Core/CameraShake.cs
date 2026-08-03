using UnityEngine;
using Unity.Cinemachine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance { get; private set; }

    [Header("References")]
    [SerializeField] private CinemachineImpulseSource impulseSource;

    [Header("Shake Settings")]
    [SerializeField, Min(0f)]
    private float cameraShakeMultiplier = 1f;

    [Tooltip("Maximum combined shake strength allowed at once.")]
    [SerializeField, Min(0f)]
    private float cameraShakeLimit = 3f;

    [Tooltip("How long it takes for the shake budget to fully recover.")]
    [SerializeField, Min(0.01f)]
    private float shakeRecoveryTime = 0.25f;

    private float currentShakeStrength;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (impulseSource == null)
            impulseSource = GetComponent<CinemachineImpulseSource>();
    }

    private void Update()
    {
        // Gradually free up shake strength as previous impulses fade.
        float recoverySpeed = cameraShakeLimit / shakeRecoveryTime;

        currentShakeStrength = Mathf.MoveTowards(
            currentShakeStrength,
            0f,
            recoverySpeed * Time.unscaledDeltaTime
        );
    }

    public void Shake(float strength = 1f)
    {
        if (impulseSource == null)
            return;

        float requestedStrength =
            Mathf.Max(0f, strength * cameraShakeMultiplier);

        float remainingStrength =
            Mathf.Max(0f, cameraShakeLimit - currentShakeStrength);

        float appliedStrength =
            Mathf.Min(requestedStrength, remainingStrength);

        if (appliedStrength <= 0f)
            return;

        currentShakeStrength += appliedStrength;
        impulseSource.GenerateImpulse(appliedStrength);
    }
}