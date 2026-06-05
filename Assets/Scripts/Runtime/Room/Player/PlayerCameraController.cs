using Cysharp.Threading.Tasks;
using UnityEngine;

public class PlayerCameraController : MonoBehaviour
{
    [SerializeField] private Camera remoteCamera;
    [SerializeField] private Camera localCamera;

    [SerializeField] private float normalViewAngle = 60f;
    [SerializeField] private float aimmingViewAngle = 45f;
    [SerializeField] private float angleSwitchingDuration = 0.3f;

    private void Start()
    {
        ActiveLocalCamera();
    }

    public async UniTaskVoid SetFieldOfView(bool isAiming)
    {
        var targetAngle = isAiming ? aimmingViewAngle : normalViewAngle;
        float start = localCamera.fieldOfView;
        float elapsed = 0f;

        while (elapsed < angleSwitchingDuration)
        {
            elapsed += Time.deltaTime;
            localCamera.fieldOfView = Mathf.Lerp(start, targetAngle, elapsed / angleSwitchingDuration);
            await UniTask.Yield();
        }

        localCamera.fieldOfView = targetAngle;
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
