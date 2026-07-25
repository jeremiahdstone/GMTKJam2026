using UnityEngine;

public class MeshSortingLayer : MonoBehaviour
{
    public string sortingLayerName = "Default";
    public int orderInLayer = 4;

    void Start()
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.sortingLayerName = sortingLayerName;
            renderer.sortingOrder = orderInLayer;
        }
    }
}
