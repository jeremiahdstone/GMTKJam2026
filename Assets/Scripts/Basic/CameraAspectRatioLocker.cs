using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraAspectRatioLocker : MonoBehaviour
{
    [SerializeField] private float targetAspectWidth = 16f;
    [SerializeField] private float targetAspectHeight = 9f;

    private Camera cam;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        UpdateCameraRect();
    }

    private void Update()
    {
        UpdateCameraRect();
    }

    private void UpdateCameraRect()
    {
        float targetAspect = targetAspectWidth / targetAspectHeight;
        float windowAspect = (float)Screen.width / Screen.height;

        // Window is too wide: add black bars on left and right
        if (windowAspect > targetAspect)
        {
            float scaleWidth = targetAspect / windowAspect;
            float xOffset = (1f - scaleWidth) / 2f;

            cam.rect = new Rect(xOffset, 0f, scaleWidth, 1f);
        }
        // Window is too tall/narrow: add black bars on top and bottom
        else
        {
            float scaleHeight = windowAspect / targetAspect;
            float yOffset = (1f - scaleHeight) / 2f;

            cam.rect = new Rect(0f, yOffset, 1f, scaleHeight);
        }
    }
}