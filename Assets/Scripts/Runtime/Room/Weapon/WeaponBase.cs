using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

public abstract class WeaponBase : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] protected float damage;
    [SerializeField] protected int magazineSize;
    [SerializeField] protected int reserveAmmo;
    [SerializeField] protected float roundsPerMinute;
    [SerializeField] protected float range;
    [SerializeField] protected float reloadTime;
    [SerializeField] protected bool isAutomatic;
    [SerializeField] protected LayerMask hitMask;

    [Header("References")]
    [SerializeField] protected Transform casingEjectPoint;
    [SerializeField] protected ParticleSystem muzzleFlashVFX;
    [SerializeField] protected GameObject casingPrefab;

    [Header("Audio")]
    [SerializeField] protected AudioClip fireClip;
    [SerializeField] protected AudioClip fireEmptyClip;
    [SerializeField] protected AudioClip reloadClip;
    [SerializeField] protected AudioClip reloadEmptyClip;

    protected AudioSource _audioSource;
    protected Animator _animator;
    protected Camera _playerCamera;
    protected int _currentAmmo;
    protected bool _isReloading;
    protected float _lastFireTime;
    private CancellationTokenSource _reloadCts;

    public int CurrentAmmo => _currentAmmo;
    public int ReserveAmmo => reserveAmmo;
    public bool IsReloading => _isReloading;
    public bool IsAutomatic => isAutomatic;

    protected virtual void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _animator = GetComponent<Animator>();
        _currentAmmo = magazineSize;
    }

    public void SetCamera(Camera cam)
    {
        _playerCamera = cam;
    }

    public virtual void Fire(bool justPressed = false)
    {
        if (_isReloading) return;
        if (Time.time - _lastFireTime < 60f / roundsPerMinute) return;

        if (_currentAmmo <= 0)
        {
            if (justPressed) _audioSource.PlayOneShot(fireEmptyClip);
            return;
        }

        _currentAmmo--;
        _lastFireTime = Time.time;

        if (muzzleFlashVFX != null)
            muzzleFlashVFX.Play();

        _audioSource.PlayOneShot(fireClip);
        EjectCasing();
        if (_animator != null)
            _animator.Play("Fire", 0, 0f);

        if (Physics.Raycast(_playerCamera.transform.position, _playerCamera.transform.forward, out RaycastHit hit, range, hitMask))
        {
            if (hit.collider.TryGetComponent<IDamageable>(out var target))
                target.TakeDamage(damage);
        }
    }

    public virtual void Reload()
    {
        if (_isReloading) return;
        if (_currentAmmo == magazineSize) return;
        if (reserveAmmo <= 0) return;

        ReloadTask().Forget();
    }

    public virtual void CancelReload()
    {
        if (!_isReloading) return;
        _reloadCts?.Cancel();
        _reloadCts?.Dispose();
        _reloadCts = null;
        _isReloading = false;
    }

    public virtual void FillAmmunition(int amount)
    {
        if (amount == 0)
            _currentAmmo = magazineSize;
        else
            _currentAmmo = Mathf.Clamp(_currentAmmo + amount, 0, magazineSize);
    }

    public virtual void EjectCasing()
    {
        if (casingPrefab != null && casingEjectPoint != null)
            Instantiate(casingPrefab, casingEjectPoint.position, casingEjectPoint.rotation);
    }

    private async UniTask ReloadTask()
    {
        _isReloading = true;
        _reloadCts = new CancellationTokenSource();

        _animator.Play("Reload", 0, 0f);
        if (_currentAmmo != 0)
            _audioSource.PlayOneShot(reloadClip);
        else
            _audioSource.PlayOneShot(reloadEmptyClip);

        try
        {
            await UniTask.WaitForSeconds(reloadTime, cancellationToken: _reloadCts.Token);
        }
        catch (System.OperationCanceledException)
        {
            return;
        }

        int take = Mathf.Min(magazineSize - _currentAmmo, reserveAmmo);
        _currentAmmo += take;
        reserveAmmo -= take;
        _isReloading = false;
        _reloadCts?.Dispose();
        _reloadCts = null;
    }
}
