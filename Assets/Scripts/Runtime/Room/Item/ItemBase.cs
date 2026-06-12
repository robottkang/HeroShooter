using UnityEngine;

public abstract class ItemBase : MonoBehaviour
{
    [SerializeField] private float rotSpeed = 30f;
    [SerializeField] private float acquireTime = 5f;

    private PlayerController _candidate;
    [SerializeField, Fusion.ReadOnly] private float _progress;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent<PlayerController>(out var player) && _candidate != null) return;
        _candidate = player;
        _progress = 0f;
        EventBus<ItemAcquireProgressEvent>.Raise(new ItemAcquireProgressEvent(0f, true));
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.TryGetComponent<PlayerController>(out var player) || player != _candidate) return;
        _candidate = null;
        _progress = 0f;
        EventBus<ItemAcquireProgressEvent>.Raise(new ItemAcquireProgressEvent(0f, false));
    }

    private void OnDestroy()
    {
        if (_candidate != null)
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

    public abstract void Acquire(PlayerController player);
}
