using UnityEngine;
using Unity.Cinemachine;
using DG.Tweening;

public class CameraHorizonShift : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private CinemachinePositionComposer positionComposer;

    [Header("Trigger")]
    [SerializeField] private float triggerY = 5f;

    [Header("Camera Shift")]
    [SerializeField] private float horizonOffsetY = 4f;

    [Header("Tween Durations")]
    [SerializeField] private float moveUpDuration = 5f;
    [SerializeField] private float moveDownDuration = 2f;

    [Header("Tween Easing")]
    [SerializeField] private Ease moveUpEase = Ease.InOutSine;
    [SerializeField] private Ease moveDownEase = Ease.InOutSine;

    private float startingOffsetY;
    private bool isAboveTrigger;
    private Tween offsetTween;

    private void Awake()
    {
        if (positionComposer == null)
            positionComposer = GetComponent<CinemachinePositionComposer>();

        if (positionComposer == null)
        {
            Debug.LogError(
                "CameraHorizonShift could not find a CinemachinePositionComposer.",
                this
            );

            enabled = false;
            return;
        }

        startingOffsetY = positionComposer.TargetOffset.y;

        if (player == null)
            return;

        isAboveTrigger = player.position.y >= triggerY;

        // Snap immediately to the correct position on Awake.
        SetOffsetY(isAboveTrigger ? horizonOffsetY : startingOffsetY);
    }

    private void Update()
    {
        if (player == null || positionComposer == null)
            return;

        bool playerIsAboveTrigger = player.position.y >= triggerY;

        // Only start a new tween when the player crosses the trigger.
        if (playerIsAboveTrigger == isAboveTrigger)
            return;

        isAboveTrigger = playerIsAboveTrigger;

        if (isAboveTrigger)
        {
            TweenToOffset(
                horizonOffsetY,
                moveUpDuration,
                moveUpEase
            );
        }
        else
        {
            TweenToOffset(
                startingOffsetY,
                moveDownDuration,
                moveDownEase
            );
        }
    }

    private void TweenToOffset(float targetY, float duration, Ease ease)
    {
        offsetTween?.Kill();

        float currentY = positionComposer.TargetOffset.y;

        offsetTween = DOTween.To(
                () => currentY,
                value =>
                {
                    currentY = value;
                    SetOffsetY(value);
                },
                targetY,
                duration
            )
            .SetEase(ease)
            .SetUpdate(true);
    }

    private void SetOffsetY(float y)
    {
        Vector3 offset = positionComposer.TargetOffset;
        offset.y = y;
        positionComposer.TargetOffset = offset;
    }

    private void OnDestroy()
    {
        offsetTween?.Kill();
    }
}