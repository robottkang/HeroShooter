using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class WeaponInventory : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] private FPArmsAnimatorController armsAnimator;

    private readonly List<WeaponBase> _weapons = new();
    private int _currentIndex = -1;
    private CancellationTokenSource _switchCts;
    private bool _isSwitching;

    public WeaponBase CurrentWeapon => _currentIndex >= 0 ? _weapons[_currentIndex] : null;
    public IReadOnlyList<WeaponBase> Weapons => _weapons;
    public int CurrentIndex => _currentIndex;
    public bool IsSwitching => _isSwitching;

    private void Awake()
    {
        foreach (Transform child in transform)
        {
            if (child.TryGetComponent<WeaponBase>(out var weapon))
            {
                _weapons.Add(weapon);
                child.gameObject.SetActive(false);
            }
        }

        if (_weapons.Count > 0)
            EquipInternal(0);
    }

    public void AddWeapon(WeaponBase weapon)
    {
        if (_weapons.Contains(weapon)) return;
        weapon.transform.SetParent(transform);
        weapon.transform.SetLocalPositionAndRotation(weapon.transform.position, weapon.transform.rotation);
        weapon.gameObject.SetActive(false);
        _weapons.Add(weapon);
    }

    public void RemoveWeapon(WeaponBase weapon)
    {
        int idx = _weapons.IndexOf(weapon);
        if (idx < 0) return;

        weapon.CancelReload();
        weapon.gameObject.SetActive(false);
        _weapons.RemoveAt(idx);

        if (_weapons.Count == 0)
        {
            _currentIndex = -1;
            EventBus<WeaponChangedEvent>.Raise(new WeaponChangedEvent(null));
            return;
        }

        int next = Mathf.Clamp(_currentIndex, 0, _weapons.Count - 1);
        _currentIndex = -1;
        EquipInternal(next);
    }

    public void Equip(int index)
    {
        if (index < 0 || index >= _weapons.Count) return;
        if (index == _currentIndex) return;
        _switchCts?.Cancel();
        _switchCts?.Dispose();
        _switchCts = new CancellationTokenSource();
        EquipAsync(index, _switchCts.Token).Forget();
    }

    public void EquipNext()
    {
        Equip((_currentIndex + 1) % _weapons.Count);
    }

    public void EquipPrev()
    {
        Equip((_currentIndex - 1 + _weapons.Count) % _weapons.Count);
    }

    private async UniTaskVoid EquipAsync(int index, CancellationToken token)
    {
        _isSwitching = true;

        bool holsterDone = false;
        void OnHolsterDone() => holsterDone = true;
        armsAnimator.OnHolsterEnded += OnHolsterDone;
        armsAnimator.SetHolstered(true);

        try
        {
            await UniTask.WaitUntil(() => holsterDone, cancellationToken: token);
        }
        catch (OperationCanceledException e)
        {
            Debug.Log(e);
            armsAnimator.OnHolsterEnded -= OnHolsterDone;
            _isSwitching = false;
            return;
        }

        armsAnimator.OnHolsterEnded -= OnHolsterDone;
        EquipInternal(index);
        armsAnimator.SetHolstered(false);
        _isSwitching = false;
    }

    private void EquipInternal(int index)
    {
        if (_currentIndex >= 0)
        {
            _weapons[_currentIndex].CancelReload();
            _weapons[_currentIndex].gameObject.SetActive(false);
        }

        _currentIndex = index;
        _weapons[_currentIndex].gameObject.SetActive(true);
        EventBus<WeaponChangedEvent>.Raise(new WeaponChangedEvent(CurrentWeapon));
    }
}
