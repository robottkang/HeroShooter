using Cysharp.Threading.Tasks;
using UnityEngine;

public class SuperJumpAbility : AbilityBase
{
    [SerializeField] private float jumpVelocity = 15f;
    [SerializeField] private float gravityMultiplier = 0.2f;
    [SerializeField] private float hangDuration = 0.6f;
    [SerializeField] private float abilityActiveDuration = 0.2f;

    public override AbilityBlockFlags BlockFlags => AbilityBlockFlags.Action;

    protected override async UniTask UseAbilityAsync(PlayerController player)
    {
        player.VerticalVelocity = jumpVelocity;

        await UniTask.WaitUntil(() => player.VerticalVelocity <= 0f,
            cancellationToken: player.destroyCancellationToken);

        player.GravityMultiplier = gravityMultiplier;
        player.VerticalVelocity = 0f;

        await UniTask.WaitForSeconds(hangDuration,
            cancellationToken: player.destroyCancellationToken);

        player.GravityMultiplier = 1f;
    }
}
