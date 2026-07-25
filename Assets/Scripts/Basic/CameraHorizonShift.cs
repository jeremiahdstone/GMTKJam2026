using UnityEngine;
using Unity.Cinemachine;

public class CameraHorizonShift : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private CinemachinePositionComposer positionComposer;

    [Header("Trigger")]
    [SerializeField] private float triggerY = 5f;

    [Header("Camera Shift")]
    [SerializeField] private float horizonOffsetY = 4f;
    [SerializeField] private float transitionSpeed = 1f;

    private float startingOffsetY;
    private float currentOffsetY;

    private void Awake()
    {
        if (positionComposer == null)
            positionComposer = GetComponent<CinemachinePositionComposer>();

        startingOffsetY = positionComposer.TargetOffset.y;
        currentOffsetY = startingOffsetY;
    }

    private void Update()
    {
        if (player == null || positionComposer == null)
            return;

        float targetOffsetY = player.position.y >= triggerY
            ? horizonOffsetY
            : startingOffsetY;

        currentOffsetY = Mathf.MoveTowards(
            currentOffsetY,
            targetOffsetY,
            transitionSpeed * Time.deltaTime
        );

        Vector3 offset = positionComposer.TargetOffset;
        offset.y = currentOffsetY;
        positionComposer.TargetOffset = offset;
    }
}