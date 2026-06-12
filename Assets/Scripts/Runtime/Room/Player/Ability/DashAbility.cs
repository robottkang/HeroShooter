using Cysharp.Threading.Tasks;
using UnityEngine;

public class DashAbility : AbilityBase
{
    [SerializeField] private float dashDistance = 8f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float abilityActiveDuration = 0.2f;

    protected override void UseAbility(PlayerController player)
    {
        DashAsync(player).Forget();
        AbilityActiveAsync(player).Forget();
    }

    private async UniTaskVoid DashAsync(PlayerController player)
    {
        Vector2 input = player.MoveInput;
        Vector3 dir = input.sqrMagnitude > 0.01f
            ? (player.transform.right * input.x + player.transform.forward * input.y).normalized
            : player.transform.forward;

        float speed = dashDistance / dashDuration;
        float elapsed = 0f;

        player.IsAbilityOverriding = true;

        while (elapsed < dashDuration)
        {
            player.CC.Move(dir * speed * Time.deltaTime);
            elapsed += Time.deltaTime;
            await UniTask.Yield(cancellationToken: player.destroyCancellationToken);
        }

        player.IsAbilityOverriding = false;
    }

    private async UniTaskVoid AbilityActiveAsync(PlayerController player)
    {
        player.IsAbilityActive = true;

        await UniTask.WaitForSeconds(abilityActiveDuration,
            cancellationToken: player.destroyCancellationToken);

        player.IsAbilityActive = false;
    }
}
