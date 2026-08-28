using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

public class PlayerCameraController : MonoBehaviour
{
    [SerializeField] private Camera remoteCamera;
    [SerializeField] private Camera localCamera;

    [SerializeField] private float normalViewAngle = 60f;
    [SerializeField] private float aimingViewAngle = 45f;
    [SerializeField] private float angleSwitchingDuration = 0.3f;

    private float _elapsedTime = 0f;
    private CancellationTokenSource _cts = new();

    public Camera RemoteCamera => remoteCamera;
    public Camera LocalCamera => localCamera;

    private void Start()
    {
        ActiveLocalCamera();
    }

    public void SetFieldOfView(bool isAiming)
    {
        _cts.Cancel();
        _cts.Dispose();
        _cts = new();

        SetFieldOfViewAsync(isAiming).Forget();
    }

    private async UniTaskVoid SetFieldOfViewAsync(bool isAiming)
    {
        if (isAiming)
        {
            while (_elapsedTime < angleSwitchingDuration)
            {
                _elapsedTime += Time.deltaTime;
                localCamera.fieldOfView = Mathf.Lerp(normalViewAngle, aimingViewAngle, _elapsedTime / angleSwitchingDuration);
                await UniTask.Yield(cancellationToken: _cts.Token);
            }
            localCamera.fieldOfView = aimingViewAngle;
        }
        else
        {
            while (_elapsedTime > 0f)
            {
                _elapsedTime -= Time.deltaTime;
                localCamera.fieldOfView = Mathf.Lerp(normalViewAngle, aimingViewAngle, _elapsedTime / angleSwitchingDuration);
                await UniTask.Yield(cancellationToken: _cts.Token);
            }
            localCamera.fieldOfView = normalViewAngle;
        }
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
