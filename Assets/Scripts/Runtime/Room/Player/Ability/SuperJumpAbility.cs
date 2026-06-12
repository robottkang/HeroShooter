using Cysharp.Threading.Tasks;
using UnityEngine;

public class SuperJumpAbility : AbilityBase
{
    [SerializeField] private float jumpVelocity = 15f;
    [SerializeField] private float gravityMultiplier = 0.2f;
    [SerializeField] private float hangDuration = 0.6f;
    [SerializeField] private float abilityActiveDuration = 0.2f;

    protected override void UseAbility(PlayerController player)
    {
        SuperJumpAsync(player).Forget();
        AbilityActiveAsync(player).Forget();
    }

    private async UniTaskVoid SuperJumpAsync(PlayerController player)
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

    private async UniTaskVoid AbilityActiveAsync(PlayerController player)
    {
        player.IsAbilityActive = true;

        await UniTask.WaitForSeconds(abilityActiveDuration,
            cancellationToken: player.destroyCancellationToken);

        player.IsAbilityActive = false;
    }
}
