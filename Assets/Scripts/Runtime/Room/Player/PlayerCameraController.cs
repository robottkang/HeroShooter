using UnityEngine;

public class PlayerCameraController : MonoBehaviour
{
    [SerializeField] private Camera remoteCamera;
    [SerializeField] private Camera localCamera;

    private void Start()
    {
        ActiveLocalCamera();
    }

    public void SetFieldOfView(float angle, float during)
    {
        Mathf.Lerp(localCamera.fieldOfView, angle, Time.deltaTime / during);
    }

    public void ActiveRemoteCamera()
    {
        remoteCamera.gameObject.SetActive(true);
        localCamera.gameObject.SetActive(false);
    }

    public void ActiveLocalCamera()
    {
        remoteCamera.gameObject.SetActive(false);
        localCamera.gameObject.SetActive(true);
    }
}
