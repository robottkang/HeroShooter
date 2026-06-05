using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

public class Crosshair : MonoBehaviour
{
    [SerializeField] private float fadeDuration = 0.2f;

    private CanvasGroup _canvasGroup;
    private CancellationTokenSource _cts;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
    }

    public void Show()
    {
        Fade(1f);
    }

    public void Hide()
    {
        Fade(0f);
    }

    private void Fade(float target)
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = new CancellationTokenSource();
        FadeTask(target, _cts.Token).Forget();
    }

    private async UniTaskVoid FadeTask(float target, CancellationToken ct)
    {
        float start = _canvasGroup.alpha;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            _canvasGroup.alpha = Mathf.Lerp(start, target, elapsed / fadeDuration);
            await UniTask.Yield(ct);
        }

        _canvasGroup.alpha = target;
    }

    private void OnDestroy()
    {
        _cts?.Cancel();
        _cts?.Dispose();
    }
}
