using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class WeaponInventory : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float switchCooldown = 0.1f;

    private readonly List<WeaponBase> _weapons = new();
    private int _currentIndex = -1;
    private float _lastSwitchTime = float.MinValue;

    public WeaponBase CurrentWeapon => _currentIndex >= 0 ? _weapons[_currentIndex] : null;
    public event Action<WeaponBase> OnWeaponChanged;

    private void Awake()
    {
        foreach (Transform child in transform)
        {
            if (child.TryGetComponent<WeaponBase>(out var weapon))
            {
                weapon.SetCamera(playerCamera);
                _weapons.Add(weapon);
                child.gameObject.SetActive(false);
            }
        }

        if (_weapons.Count > 0)
            EquipInternal(0);
    }

    private void Update()
    {
        HandleScrollSwitch();
        HandleNumberKeySwitch();
    }

    public void AddWeapon(WeaponBase weapon)
    {
        if (_weapons.Contains(weapon)) return;
        weapon.transform.SetParent(transform);
        weapon.SetCamera(playerCamera);
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
            OnWeaponChanged?.Invoke(null);
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
        if (Time.time - _lastSwitchTime < switchCooldown) return;
        EquipInternal(index);
    }

    private void EquipInternal(int index)
    {
        if (_currentIndex >= 0)
        {
            _weapons[_currentIndex].CancelReload();
            _weapons[_currentIndex].gameObject.SetActive(false);
        }

        _currentIndex = index;
        _lastSwitchTime = Time.time;
        _weapons[_currentIndex].gameObject.SetActive(true);
        OnWeaponChanged?.Invoke(CurrentWeapon);
    }

    private void HandleScrollSwitch()
    {
        if (_weapons.Count < 2) return;

        float scroll = Mouse.current.scroll.ReadValue().y;
        if (scroll > 0f)
            Equip((_currentIndex - 1 + _weapons.Count) % _weapons.Count);
        else if (scroll < 0f)
            Equip((_currentIndex + 1) % _weapons.Count);
    }

    private void HandleNumberKeySwitch()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        KeyControl[] keyControls = {
            kb.digit1Key, kb.digit2Key, kb.digit3Key,
            kb.digit4Key, kb.digit5Key, kb.digit6Key,
            kb.digit7Key, kb.digit8Key, kb.digit9Key };

        for (var i = 0; i < 9; i += 1)
        {
            if (keyControls[i].wasPressedThisFrame)
            {
                Equip(i);
                break;
            }
        }
    }
}
