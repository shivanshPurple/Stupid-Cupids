using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager singleton;
    public Camera cam;
    void Awake()
    {
        if (singleton == null)
            singleton = this;
        else if (singleton != this)
            Destroy(gameObject);
        cam = GetComponent<Camera>();
    }
}
