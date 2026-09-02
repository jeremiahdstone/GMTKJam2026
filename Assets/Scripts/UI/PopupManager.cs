using UnityEngine;

public class PopupManager : MonoBehaviour
{
    public static PopupManager instance;

    [SerializeField] private GameObject infoPopupPrefab;
    [SerializeField] private Canvas canvas;

    private void Awake()
    {
        instance = this;
    }

    public InfoPopup SpawnPopup(
        Transform anchor,
        Vector3 anchorPoint,
        string title,
        string description,
        PopupAnchorSpace anchorSpace = PopupAnchorSpace.World,
        float extraHorizontalPadding = 0f)
    {
        InfoPopup popup = PoolManager.instance.Spawn(infoPopupPrefab, this.transform).GetComponent<InfoPopup>();

        popup.transform.SetParent(canvas.transform, false);
        popup.transform.localScale = Vector3.one;
        popup.transform.SetAsLastSibling();

        popup.Initialize(
            anchor,
            anchorPoint,
            title,
            description,
            canvas,
            anchorSpace,
            extraHorizontalPadding
        );

        return popup;
    }

    public void ReleasePopup(InfoPopup popup)
    {
        PoolManager.instance.Release(popup);
    }
}