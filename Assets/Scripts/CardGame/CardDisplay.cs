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
        if (CardManager.Instance.playerStats == null || CardManager.Instance.playerStats.currentMana < cardData.manaCost)
        {
            Debug.Log($"마나가 부족합니다. (필요 : {cardData.manaCost}, 현재 : {CardManager.Instance.playerStats.currentMana}");
            transform.position = originalPosition;
            return;
        }
        isDragging = false;

        //레이캐스트로 타겟 감지
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        //카드 사용 판정
        bool cardUsed = false;

        //적 위에 드롭했는지 검사
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, enemyLayer))
        {
            CharacterStats enemyStats = hit.collider.GetComponent<CharacterStats>();

            if (enemyStats != null)
            {
                if (cardData.cardType == CardData.CardType.Attack)
                {
                    //공격 카드면 데미지 추가
                    enemyStats.TakeDamage(cardData.effectAmount);
                    Debug.Log($"{cardData.cardName} 카드로 적에게 {cardData.effectAmount} 데미지를 입혔습니다.");
                    cardUsed = true;
                }
                else
                {
                    Debug.Log("이 카드는 적에게 사용할 수 없습니다.");
                }
            }
            
        }
        else if (Physics.Raycast(ray, out hit, Mathf.Infinity, playerLayer))
        {
            CharacterStats playerStats = hit.collider.GetComponent<CharacterStats>();

            if (playerStats != null)
            {
                if (cardData.cardType == CardData.CardType.Heal)
                {
                    //힘 카드면 회복하기
                    playerStats.Heal(cardData.effectAmount);
                    Debug.Log($"{cardData.cardName} 카드로 플레이어의 체력을 {cardData.effectAmount} 회복했습니다.");
                    cardUsed = true;
                }
                else
                {
                    Debug.Log("이 카드는 플레이어에게 사용할 수 없습니다.");
                }
            }
        }
        else if (CardManager.Instance != null)
        {
            //버린 카드 더미 근처에 드롭 했는지 검사
            float distToDiscard = Vector3.Distance(transform.position, CardManager.Instance.discardPosition.position);
            if (distToDiscard < 0.2f)
            {
                //카드 버리기
                CardManager.Instance.DiscardCard(cardIndex);
                return;
            }
        }
        if (!cardUsed)
        {
            transform.position = originalPosition;
            CardManager.Instance.ArrangeHand();
        }
        else
        {
            //카드를 사용했다면 버린 카드 더미로 이동
            if (CardManager.Instance != null)
                CardManager.Instance.DiscardCard(cardIndex);

            //카드 사용시 마나 소모 (카드가 성공적으로 사용된 후 추가
            CardManager.Instance.playerStats.UseMana(cardData.manaCost);
            Debug.Log($"마나를 {cardData.manaCost} 사용 했습니다.");
        }

        //카드를 사용하지 않으면 우너래 위치로 되돌리기
        if (!cardUsed)
        {
            transform .position = originalPosition;
        }
        else
        {
            Destroy(gameObject);
        }
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
    }
}
