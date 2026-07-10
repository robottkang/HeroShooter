using Fusion;
using UnityEngine;

public abstract class ItemBase : NetworkBehaviour
{
    [SerializeField] private float rotSpeed = 30f;
    [SerializeField] private float acquireTime = 5f;

    private IItemInteractor _candidate;
    [SerializeField, ReadOnly] private float _progress;

    private void OnTriggerEnter(Collider other)
    {
        if (_candidate != null) return;
        if (!other.TryGetComponent<IItemInteractor>(out var interactor)) return;
        _candidate = interactor;
        _candidate.OnNearItem(true);
        _progress = 0f;
        EventBus<ItemAcquireProgressEvent>.Raise(new ItemAcquireProgressEvent(0f, true));
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.TryGetComponent<IItemInteractor>(out var interactor) || interactor != _candidate) return;
        _candidate.OnNearItem(false);
        _candidate = null;
        _progress = 0f;
        EventBus<ItemAcquireProgressEvent>.Raise(new ItemAcquireProgressEvent(0f, false));
    }

    private void OnDestroy()
    {
        if (_candidate == null) return;
        _candidate.OnNearItem(false);
        EventBus<ItemAcquireProgressEvent>.Raise(new ItemAcquireProgressEvent(0f, false));
    }

    private void Update()
    {
        HandleAcquire();

        transform.Rotate(Vector3.up, rotSpeed * Time.deltaTime);
    }

    private void HandleAcquire()
    {
        if (_candidate == null) return;

        if (!_candidate.IsAcquiring)
        {
            if (_progress > 0f)
            {
                _progress = 0f;
                EventBus<ItemAcquireProgressEvent>.Raise(new ItemAcquireProgressEvent(0f, true));
            }
            return;
        }

        _progress += Time.deltaTime;
        EventBus<ItemAcquireProgressEvent>.Raise(new ItemAcquireProgressEvent(_progress / acquireTime, true));

        if (_progress >= acquireTime)
            Acquire(_candidate);
    }

    public abstract void Acquire(IItemInteractor player);
}
