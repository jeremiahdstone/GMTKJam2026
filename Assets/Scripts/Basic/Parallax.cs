using UnityEngine;

public class Parallax : MonoBehaviour
{
    private float length;
    private Vector3 startPos;

    [SerializeField] private GameObject cam;
    [SerializeField] private float parallaxEffect;

    private void Start()
    {
        startPos = transform.position;
        length = GetComponent<SpriteRenderer>().bounds.size.x;

        if (cam == null)
            cam = GameObject.FindGameObjectWithTag("MainCamera");
    }

    private void LateUpdate()
    {
        if (cam == null)
            return;

        float tempX = cam.transform.position.x * (1f - parallaxEffect);
        float distX = cam.transform.position.x * parallaxEffect;

        if (tempX > startPos.x + length)
            startPos.x += length;
        else if (tempX < startPos.x - length)
            startPos.x -= length;

        transform.position = new Vector3(
            startPos.x + distX,
            startPos.y,
            transform.position.z
        );
    }
}