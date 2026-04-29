using UnityEngine;

public class AdditionalEffect
{
    public CardData.AdditionalEffectType effectType;
    public int effectAmount;

    public string GetDescription()
    {
        switch (effectType)
        {
            case CardData.AdditionalEffectType.DrawCard:
                return $"카드 {effectAmount} 장 드로우";
            case CardData.AdditionalEffectType.DisacardCard:
                return $"카드 {effectAmount} 장 버리기";
            case CardData.AdditionalEffectType.GainMana:
                return $"카드 {effectAmount} 장 획득";
            case CardData.AdditionalEffectType.ReduceEnemyMana:
                return $"카드 {effectAmount} 장 감소";
            case CardData.AdditionalEffectType.ReduceCardCost:
                return $"다음 카드 비용 {effectAmount} 버리기";
            default:
                return "";
        }

    }
}
