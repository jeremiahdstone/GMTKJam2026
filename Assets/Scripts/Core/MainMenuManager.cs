using UnityEngine;
using DG.Tweening;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private GameObject gameLogo;

    [Header("Logo Bob")]
    [SerializeField] private float bobDistance = 8f;
    [SerializeField] private float bobDuration = 2.5f;

    private Tween logoTween;
    private RectTransform logoRect;

    private void Start()
    {
        logoRect = gameLogo.GetComponent<RectTransform>();

        logoTween = logoRect
            .DOAnchorPosY(
                logoRect.anchoredPosition.y + bobDistance,
                bobDuration
            )
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo)
            .SetUpdate(true);
    }

    public void StartGame()
    {
        SceneFader.instance.FadeToScene("GameScene");
    }

    private void OnDestroy()
    {
        logoTween?.Kill();
    }
}