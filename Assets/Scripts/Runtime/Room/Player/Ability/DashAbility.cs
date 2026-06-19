using Cysharp.Threading.Tasks;
using UnityEngine;

public class DashAbility : AbilityBase
{
    [SerializeField] private float dashDistance = 8f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float abilityActiveDuration = 0.2f;

    public override AbilityBlockFlags BlockFlags => AbilityBlockFlags.Move | AbilityBlockFlags.Action;

    protected override async UniTask UseAbilityAsync(PlayerController player)
    {
        Vector2 input = player.MoveInput;
        Vector3 dir = input.sqrMagnitude > 0.01f
            ? (player.transform.right * input.x + player.transform.forward * input.y).normalized
            : player.transform.forward;

        float speed = dashDistance / dashDuration;
        float elapsed = 0f;

        while (elapsed < dashDuration)
        {
            player.CC.Move(dir * speed * Time.deltaTime);
            elapsed += Time.deltaTime;
            await UniTask.Yield(cancellationToken: player.destroyCancellationToken);
        }
    }
}
