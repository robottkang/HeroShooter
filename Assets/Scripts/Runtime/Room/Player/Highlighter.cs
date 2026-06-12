using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class Highlighter : MonoBehaviour, IEventListener<PositionRevealedEvent>
{
    private int _defaultLayer;
    private int _highlightLayer;
    private bool _active;
    private CancellationTokenSource _durationCts;

    public bool IsActive => _active;

    private void Awake()
    {
        _defaultLayer = gameObject.layer;
        _highlightLayer = LayerMask.NameToLayer("Highlighted");
    }

    private void OnEnable()
    {
        EventBus<PositionRevealedEvent>.Register(this);
    }

    private void OnDisable()
    {
        EventBus<PositionRevealedEvent>.Unregister(this);
    }

    public void SetHighlight(bool active)
    {
        if (_active == active) return;
        _active = active;
        SetLayerRecursively(gameObject, active ? _highlightLayer : _defaultLayer);
    }

    public void OnEvent(PositionRevealedEvent e)
    {
        if (e.Target != this) return;

        _durationCts?.Cancel();
        _durationCts?.Dispose();
        _durationCts = null;

        SetHighlight(true);

        if (e.Duration > 0f)
        {
            _durationCts = new CancellationTokenSource();
            ExpireAfter(e.Duration, _durationCts.Token).Forget();
        }
    }

    private async UniTaskVoid ExpireAfter(float duration, CancellationToken token)
    {
        await UniTask.WaitForSeconds(duration, cancellationToken: token);
        SetHighlight(false);
    }

    private void SetLayerRecursively(GameObject go, int layer)
    {
        go.layer = layer;
        foreach (Transform child in go.transform)
            SetLayerRecursively(child.gameObject, layer);
    }
}
