using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaminaSystem : Singleton<StaminaSystem>
{
    [SerializeField] private StaminaUI staminaUI;
    private const int MAX_STAMINA = 3;
    private int currentStamina = MAX_STAMINA;

    private void OnEnable()
    {
        ActionSystem.AttachPerformer<SpendStaminaGA>(SpendStaminaPerformer);
        ActionSystem.AttachPerformer<RefillStaminaGA>(RefillStaminaPerformer);
        ActionSystem.SubscribeReaction<EnemyTurnGA>(EnemyTurnPostReaction, ReactionTiming.POST);
    }
    private void OnDisable()
    {
        ActionSystem.DetachPerformer<SpendStaminaGA>();
        ActionSystem.DetachPerformer<RefillStaminaGA>();
        ActionSystem.UnsubscribeReaction<EnemyTurnGA>(EnemyTurnPostReaction, ReactionTiming.POST);

    }

    /// <summary>
    /// 检查法力值是否足够
    /// </summary>
    /// <param name="Stamina"></param>
    /// <returns></returns>
    public bool HasEnoughStamina(int Stamina)
    {
        return currentStamina >= Stamina;
    }

    public void Setup()
    {
        staminaUI.UpdateStamina(currentStamina);
    }

    private IEnumerator SpendStaminaPerformer(SpendStaminaGA spendStaminaGA)
    {
        currentStamina -= spendStaminaGA.Amount;

        Debug.Log(currentStamina);
        staminaUI.UpdateStamina(currentStamina);
        yield return null;
    }
    private IEnumerator RefillStaminaPerformer(RefillStaminaGA refillStaminaGA)
    {
        currentStamina = MAX_STAMINA;
        staminaUI.UpdateStamina(currentStamina);
        yield return null;
    }

    private void EnemyTurnPostReaction(EnemyTurnGA enemyTurnGA)
    {
        RefillStaminaGA refillStaminaGA = new();
        ActionSystem.Instance.AddReaction(refillStaminaGA);
    }
}
