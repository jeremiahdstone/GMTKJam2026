using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum PopupAnchorSpace
{
    World,
    Canvas
}

public class InfoPopup : MonoBehaviour
{
    [Header("Text")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;

    [Header("Sizing")]
    [SerializeField] private float maxWidth = 80f;

    [Header("Positioning")]
    [SerializeField] private PopupAnchorSpace anchorSpace = PopupAnchorSpace.World;
    [SerializeField] private float horizontalPadding = 20f;
    [SerializeField] private float screenPadding = 10f;

    private RectTransform rectTransform;
    private VerticalLayoutGroup layoutGroup;

    private Canvas canvas;
    private RectTransform canvasRect;

    private Transform anchor;
    private Vector3 anchorPoint;

    private Camera worldCamera;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        layoutGroup = GetComponent<VerticalLayoutGroup>();
    }

    public void Initialize(
        Transform anchor,
        Vector3 anchorPoint,
        string title,
        string description,
        Canvas canvas)
    {
        this.anchor = anchor;
        this.anchorPoint = anchorPoint;
        this.canvas = canvas;

        canvasRect = canvas.transform as RectTransform;
        worldCamera = Camera.main;

        titleText.text = title;
        descriptionText.text = description;

        RefreshLayout();
        PositionPopup();
    }

    private void LateUpdate()
    {
        PositionPopup();
    }

    private void RefreshLayout()
    {
        // Natural width of the text before wrapping.
        float titleWidth = titleText.GetPreferredValues(
            titleText.text,
            Mathf.Infinity,
            Mathf.Infinity
        ).x;

        float descriptionWidth = descriptionText.GetPreferredValues(
            descriptionText.text,
            Mathf.Infinity,
            Mathf.Infinity
        ).x;

        float horizontalLayoutPadding =
            layoutGroup.padding.left +
            layoutGroup.padding.right;

        float desiredWidth =
            Mathf.Max(titleWidth, descriptionWidth)
            + horizontalLayoutPadding;

        float finalWidth = Mathf.Min(desiredWidth, maxWidth);

        // Root owns its width.
        rectTransform.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Horizontal,
            finalWidth
        );

        // TMP needs to know the new width before Unity calculates height.
        titleText.ForceMeshUpdate();
        descriptionText.ForceMeshUpdate();

        LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
    }

    private void PositionPopup()
    {
        if (anchor == null || canvasRect == null)
            return;

        Vector2 anchorScreenPosition = GetAnchorScreenPosition();

        // Work entirely in screen space first.
        Vector2 popupScreenSize =
            rectTransform.rect.size * canvas.scaleFactor;

        Vector2 popupScreenPosition =
            CalculateScreenPosition(
                anchorScreenPosition,
                popupScreenSize
            );

        Camera canvasCamera =
            canvas.renderMode == RenderMode.ScreenSpaceOverlay
                ? null
                : canvas.worldCamera;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                popupScreenPosition,
                canvasCamera,
                out Vector2 localPosition))
        {
            rectTransform.anchoredPosition = localPosition;
        }
    }

    private Vector2 GetAnchorScreenPosition()
    {
        if (anchorSpace == PopupAnchorSpace.Canvas &&
            anchor is RectTransform rectAnchor)
        {
            Camera canvasCamera =
                canvas.renderMode == RenderMode.ScreenSpaceOverlay
                    ? null
                    : canvas.worldCamera;

            return RectTransformUtility.WorldToScreenPoint(
                canvasCamera,
                rectAnchor.TransformPoint(anchorPoint)
            );
        }

        if (worldCamera == null)
            return Vector2.zero;

        Vector3 worldPosition =
            anchor.TransformPoint(anchorPoint);

        return worldCamera.WorldToScreenPoint(worldPosition);
    }

    private Vector2 CalculateScreenPosition(
        Vector2 anchorPosition,
        Vector2 popupSize)
    {
        Vector2 halfSize = popupSize * 0.5f;

        // -------------------------
        // Horizontal
        // -------------------------

        float rightCenterX =
            anchorPosition.x +
            horizontalPadding +
            halfSize.x;

        float leftCenterX =
            anchorPosition.x -
            horizontalPadding -
            halfSize.x;

        float rightEdge =
            rightCenterX + halfSize.x;

        float leftEdge =
            leftCenterX - halfSize.x;

        float x;

        // Prefer right.
        if (rightEdge <= Screen.width - screenPadding)
        {
            x = rightCenterX;
        }
        else if (leftEdge >= screenPadding)
        {
            x = leftCenterX;
        }
        else
        {
            // Popup is too wide to cleanly fit either side.
            x = Mathf.Clamp(
                rightCenterX,
                screenPadding + halfSize.x,
                Screen.width - screenPadding - halfSize.x
            );
        }

        // -------------------------
        // Vertical
        // -------------------------

        float y = anchorPosition.y;

        y = Mathf.Clamp(
            y,
            screenPadding + halfSize.y,
            Screen.height - screenPadding - halfSize.y
        );

        return new Vector2(x, y);
    }
}