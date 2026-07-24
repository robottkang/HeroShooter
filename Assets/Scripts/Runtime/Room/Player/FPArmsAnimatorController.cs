using System;
using UnityEngine;

public class FPArmsAnimatorController : MonoBehaviour, IEventListener<WeaponChangedEvent>
{
    [SerializeField] private PlayerController playerController;
    [SerializeField] private WeaponInventory weaponInventory;

    public const int LayerPoses = 0;
    public const int LayerLocomotion = 1;
    public const int LayerOverlay = 2;
    public const int LayerActions = 3;
    public const int LayerHolster = 4;
    public const int LayerWiggles = 5;

    private static readonly int RunningId = Animator.StringToHash("Running");
    private static readonly int HolsteredId = Animator.StringToHash("Holstered");
    private static readonly int AimId = Animator.StringToHash("Aim");
    private static readonly int MovementId = Animator.StringToHash("Movement");
    private static readonly int AimingId = Animator.StringToHash("Aiming");
    private static readonly int AimingSpeedMultId = Animator.StringToHash("Aiming Speed Multiplier");
    private static readonly int PlayRateHolsterId = Animator.StringToHash("Play Rate Holster");
    private static readonly int PlayRateUnholsterId = Animator.StringToHash("Play Rate Unholster");
    private static readonly int AlphaIKHandLeftId = Animator.StringToHash("Alpha IK Hand Left");
    private static readonly int AlphaIKHandRightId = Animator.StringToHash("Alpha IK Hand Right");

    public event Action OnHolsterEnded;
    public event Action OnReloadEnded;
    public event Action OnAmmunitionFilled;
    public event Action OnCasingEjected;

    private Animator _anim;
    private WeaponBase _currentWeapon;

    private void OnEnable()
    {
        EventBus<WeaponChangedEvent>.Register(this);
    }

    private void OnDisable()
    {
        EventBus<WeaponChangedEvent>.Unregister(this);
    }

    private void Awake()
    {
        _anim = GetComponent<Animator>();
    }

    private void OnDestroy()
    {
        UnsubscribeWeapon(_currentWeapon);
    }

    private void Update()
    {
        bool aiming = playerController.IsAiming;
        Vector2 move = playerController.MoveInput;

        _anim.SetBool(RunningId, playerController.IsSprinting && move.sqrMagnitude > 0.01f);
        _anim.SetBool(AimId, aiming);
        _anim.SetFloat(MovementId, move.magnitude, 0.1f, Time.deltaTime);
        _anim.SetFloat(AimingId, aiming ? 1f : 0f, 0.15f, Time.deltaTime);
    }

    private void ChangeAnimatorController(WeaponBase weapon)
    {
        UnsubscribeWeapon(_currentWeapon);
        _currentWeapon = weapon;
        _anim.runtimeAnimatorController = weapon.Controller;
        weapon.OnFire += PlayFireAnimation;
        weapon.OnReload += PlayReloadAnimation;
    }

    private void UnsubscribeWeapon(WeaponBase weapon)
    {
        if (weapon == null) return;
        weapon.OnFire -= PlayFireAnimation;
        weapon.OnReload -= PlayReloadAnimation;
    }

    public void OnEvent(WeaponChangedEvent e)
    {
        if (e.Source != Fusion.NetworkRunner.Instances[0].LocalPlayer) return;

        ChangeAnimatorController(e.Weapon);
    }

    private void PlayFireAnimation() => _anim.Play("Fire", LayerOverlay, 0f);
    private void PlayReloadAnimation() => _anim.Play("Reload", LayerActions, 0f);

    public void SetHolstered(bool holstered) => _anim.SetBool(HolsteredId, holstered);
    public void SetPlayRateHolster(float rate) => _anim.SetFloat(PlayRateHolsterId, rate);
    public void SetPlayRateUnholster(float rate) => _anim.SetFloat(PlayRateUnholsterId, rate);
    public void SetAimingSpeedMultiplier(float multiplier) => _anim.SetFloat(AimingSpeedMultId, multiplier);
    public void SetIKHandLeft(float alpha) => _anim.SetFloat(AlphaIKHandLeftId, alpha);
    public void SetIKHandRight(float alpha) => _anim.SetFloat(AlphaIKHandRightId, alpha);

    // AnimationEvent receivers
    private void OnAnimationEndedHolster() => OnHolsterEnded?.Invoke();
    private void OnAnimationEndedReload() => OnReloadEnded?.Invoke();
    private void OnAmmunitionFill() => OnAmmunitionFilled?.Invoke();
    private void OnEjectCasing() => OnCasingEjected?.Invoke();
}
