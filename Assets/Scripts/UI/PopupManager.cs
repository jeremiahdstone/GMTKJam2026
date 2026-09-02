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
    string description)
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
            canvas
        );

        return popup;
    }

    public void ReleasePopup(InfoPopup popup)
    {
        PoolManager.instance.Release(popup);
    }
}