using UnityEngine;

public class LinkButton : MonoBehaviour
{
    public void openLink(string url)
    {
        Application.OpenURL(url);
    }
}
