using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
public class CardDisplay : MonoBehaviour
{
    public CardData cardData;
    public int cardIndex;

    public MeshRenderer cardRenderer;
    public TextMeshPro nameText;
    public TextMeshPro costText;
    public TextMeshPro attackText;
    public TextMeshPro descriptionText;

    public bool isDragging = false;
    private Vector3 originalPosition;

    public LayerMask enemyLayer;
    public LayerMask playerLayer;

    void Start()
    {
        playerLayer = LayerMask.GetMask("Player");
        enemyLayer = LayerMask.GetMask("Enemy");

        SetupCard(cardData);
    }
    private void OnMouseDown()
    {
        originalPosition = transform.position;
        isDragging = true;
    }
    private void OnMouseDrag()
    {
        if (isDragging)
        {
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = Camera.main.WorldToScreenPoint(transform.position).z;
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
            transform.position = new Vector3(worldPos.x, worldPos.y, transform.position.z);
        }
    }
    private void OnMouseUp()
    {
        //if (CardManager.Instance.playerStats == null || CardManager.Instance.playerStats.currentMana < cardData.manaCost)
        //{
        //    Debug.Log($"마나가 부족합니다. (필요 : {cardData.manaCost}, 현재 : {CardManager.Instance.playerStats.currentMana}");
        //    transform.position = originalPosition;
        //    return;
        //}
        isDragging = false;
        
        if (CardManager.Instance != null)
        {
            float disToDiscard = Vector3.Distance(transform.position, CardManager.Instance.discardPosition.position);

            if (disToDiscard < 2.0f)
            {
                CardManager.Instance.DiscardCard(cardIndex);
                return;
            }
        }
        if (CardManager.Instance.playerStats != null && CardManager.Instance.playerStats.currentMana < cardData.manaCost)
        {
            Debug.LogWarning($"마나가 부족합니다! 필요 : {cardData.manaCost}, 현재 : {CardManager.Instance.playerStats?.currentMana?? 0}");
            transform.position = originalPosition;
            return;
        }

        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        //카드 사용 판정 지역 변수
        bool cardUsed = false;

        //적 위에 드롭 했는지 검사
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, enemyLayer))
        {
            CharacterStats enemyStats = hit.collider.GetComponent<CharacterStats>();

            if (enemyStats != null)
            {
                if (cardData.cardType == CardData.CardType.Attack)
                {
                    enemyStats.TakeDamage(cardData.effectAmount);
                    Debug.Log($"{cardData.cardName} 카드로 적에게 {cardData.effectAmount} 데미지를 입혔습니다.");
                    cardUsed = true;
                }
            }
            else
            {
                Debug.Log("이 카드는 적에게 사용할 수 없습니다.");
            }
        }
        else if (Physics.Raycast(ray, out hit, Mathf.Infinity, playerLayer))
        {
            if (CardManager.Instance.playerStats != null)
            {
                if (cardData.cardType == CardData.CardType.Heal)
                {
                    CardManager.Instance.playerStats.Heal(cardData.effectAmount);
                    Debug.Log($"{cardData.cardName} 카드로 플레이어의 체력을 {cardData.effectAmount} 회복했습니다");
                    cardUsed = true;
                }
            }
            else
            {
                Debug.Log("이 카드는 플레이어에게 사용할 수 없습니다.");
            }
        }
        if (!cardUsed)
        {
            transform.position = originalPosition;
            if (CardManager.Instance != null)
                CardManager.Instance.ArrangeHand();
            return;
        }

        //카드 소모시 마나 소모
        CardManager.Instance.playerStats.UseMana(cardData.manaCost);
        Debug.Log($"마나를 {cardData.manaCost} 사용했습니다. (남은 마나 : {CardManager.Instance.playerStats.currentMana})");

        //추가 효과가 있는 경우 처리
        if (cardData.additionalEffects != null && cardData.additionalEffects.Count > 0)
        {
            ProcessAdditionalEffectAndDiscard();
        }
        else
        {
            if (CardManager.Instance != null)
                CardManager.Instance.DiscardCard(cardIndex);
        }

        ////레이캐스트로 타겟 감지
        //RaycastHit hit;
        //Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        ////카드 사용 판정
        //bool cardUsed = false;

        ////적 위에 드롭했는지 검사
        //if (Physics.Raycast(ray, out hit, Mathf.Infinity, enemyLayer))
        //{
        //    CharacterStats enemyStats = hit.collider.GetComponent<CharacterStats>();

        //    if (enemyStats != null)
        //    {
        //        if (cardData.cardType == CardData.CardType.Attack)
        //        {
        //            //공격 카드면 데미지 추가
        //            enemyStats.TakeDamage(cardData.effectAmount);
        //            Debug.Log($"{cardData.cardName} 카드로 적에게 {cardData.effectAmount} 데미지를 입혔습니다.");
        //            cardUsed = true;
        //        }
        //        else
        //        {
        //            Debug.Log("이 카드는 적에게 사용할 수 없습니다.");
        //        }
        //    }
            
        //}
        //else if (Physics.Raycast(ray, out hit, Mathf.Infinity, playerLayer))
        //{
        //    CharacterStats playerStats = hit.collider.GetComponent<CharacterStats>();

        //    if (playerStats != null)
        //    {
        //        if (cardData.cardType == CardData.CardType.Heal)
        //        {
        //            //힘 카드면 회복하기
        //            playerStats.Heal(cardData.effectAmount);
        //            Debug.Log($"{cardData.cardName} 카드로 플레이어의 체력을 {cardData.effectAmount} 회복했습니다.");
        //            cardUsed = true;
        //        }
        //        else
        //        {
        //            Debug.Log("이 카드는 플레이어에게 사용할 수 없습니다.");
        //        }
        //    }
        //}
        //else if (CardManager.Instance != null)
        //{
        //    //버린 카드 더미 근처에 드롭 했는지 검사
        //    float distToDiscard = Vector3.Distance(transform.position, CardManager.Instance.discardPosition.position);
        //    if (distToDiscard < 0.2f)
        //    {
        //        //카드 버리기
        //        CardManager.Instance.DiscardCard(cardIndex);
        //        return;
        //    }
        //}
        //if (!cardUsed)
        //{
        //    transform.position = originalPosition;
        //    CardManager.Instance.ArrangeHand();
        //}
        //else
        //{
        //    //카드를 사용했다면 버린 카드 더미로 이동
        //    if (CardManager.Instance != null)
        //        CardManager.Instance.DiscardCard(cardIndex);

        //    //카드 사용시 마나 소모 (카드가 성공적으로 사용된 후 추가
        //    CardManager.Instance.playerStats.UseMana(cardData.manaCost);
        //    Debug.Log($"마나를 {cardData.manaCost} 사용 했습니다.");
        //}

        ////카드를 사용하지 않으면 우너래 위치로 되돌리기
        //if (!cardUsed)
        //{
        //    transform .position = originalPosition;
        //}
        //else
        //{
        //    Destroy(gameObject);
        //}
    }
    public void SetupCard(CardData data)
    {
        cardData = data;

        //3D 텍스트 업데이트
        if (nameText != null ) nameText.text = data.cardName;
        if (costText != null) costText.text = data.manaCost.ToString();
        if (attackText != null) attackText.text = data.effectAmount.ToString();
        if (descriptionText != null) descriptionText.text = data.description;

        //카드 텍스처 설정
        if (cardRenderer != null && data.artwork != null)
        {
            Material cardMaterial = cardRenderer.material;
            cardMaterial.mainTexture = data.artwork.texture;
        }

        //카드 설명 텍스트에 추가 효과 설명 추가 
        if (descriptionText != null)
        {
            descriptionText.text = data.description + data.GetAdditionalEffectDescription();
        }
    }
    public void ProcessAdditionalEffectAndDiscard()
    {
        //카드 데이터 및 인덱스 보존
        CardData cardDataCopy = cardData;
        int cardIndexCopy = cardIndex;

        //추가 효과 적용
        foreach (var effect in cardDataCopy.additionalEffects)
        {
            switch (effect.effectType)
            {
                case CardData.AdditionalEffectType.DrawCard:
                    for (int i = 0; i < effect.effectAmount; i++)
                    {
                        if (CardManager.Instance != null)
                        {
                            CardManager.Instance.DrawCard();
                        }

                    }
                    Debug.Log($"{effect.effectAmount} 장의 카드를 드로우 했습니다.");

                    break;

                case CardData.AdditionalEffectType.DisacardCard:
                    for (int i = 0; i < effect.effectAmount; i++)
                    {
                        if (CardManager.Instance != null && CardManager.Instance.handCards.Count > 0)
                        {
                            int randomIndex = Random.Range(0, CardManager.Instance.handCards.Count);

                            Debug.Log($"랜덤 카드 버리기 : 선택된 인덱스 {randomIndex}, 현재 손패 크기 : {CardManager.Instance.handCards.Count}");

                            if (cardIndexCopy < CardManager.Instance.handCards.Count)
                            {
                                if (randomIndex != cardIndexCopy)
                                {
                                    CardManager.Instance.DiscardCard(randomIndex);

                                    //만약 버린 카드의 인댁스가 현재 카드의 인덱스보다 작으면 현재 카드의 인덱스를 1 감소시켜야함
                                    if (randomIndex < cardIndexCopy)
                                    {
                                        cardIndexCopy--;
                                    }
                                    
                                }
                                else if (CardManager.Instance.handCards.Count > 1)
                                {
                                    //다른 카드 선택
                                    int newIndex = (randomIndex + 1) % CardManager.Instance.handCards.Count;
                                    CardManager.Instance.DiscardCard(newIndex);

                                    if (randomIndex < cardIndexCopy)
                                    {
                                        cardIndexCopy--;
                                    }
                                }
                            }
                            else
                            {
                                //CardIndexCopy가 더이상 유효하지 않은 경우, 아무 카드나 버림
                                CardManager.Instance.DiscardCard(randomIndex);
                            }
                        }
                    }
                    break;

                case CardData.AdditionalEffectType.GainMana:
                    if (CardManager.Instance.playerStats != null)
                    {
                        CardManager.Instance.playerStats.GainMana(effect.effectAmount);
                        Debug.Log($"마나를 {effect.effectAmount} 획득 했습니다");
                    }
                    break;

                case CardData.AdditionalEffectType.ReduceEnemyMana:
                    if (CardManager.Instance.enemyStats != null)
                    {
                        CardManager.Instance.enemyStats.UseMana(effect.effectAmount);
                        Debug.Log($"적이 마나를 {effect.effectAmount} 잃었습니다");
                    }
                    break;
            }
        }
        if (CardManager.Instance != null)
        {
            CardManager.Instance.DiscardCard(cardIndexCopy);
        }
    }
}
