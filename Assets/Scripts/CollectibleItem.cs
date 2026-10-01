using UnityEngine;

// 필수과제 2-2: 상호작용 성공 시 눈에 보이는 반응 (아이템 획득 및 회전/사라짐)
public class CollectibleItem : MonoBehaviour, IInteractable
{
    [Header("Item Settings")]
    [SerializeField] private string itemName = "에너지 크리스탈";
    [SerializeField] private float rotateSpeed = 60f;
    [SerializeField] private GameObject pickupEffectPrefab; // 선택적 파티클 효과

    private void Update()
    {
        // 필드에서 천천히 회전 (시각적 어포던스 제공)
        transform.Rotate(Vector3.up * rotateSpeed * Time.deltaTime, Space.World);
    }

    public string GetInteractionPrompt()
    {
        return $"[E] {itemName} 획득하기";
    }

    public void Interact()
    {
        Debug.Log($"[CollectibleItem] 아이템 획득: {itemName}");

        // 파티클 생성 (선택)
        if (pickupEffectPrefab != null)
        {
            Instantiate(pickupEffectPrefab, transform.position, Quaternion.identity);
        }

        // 아이템 오브젝트 제거 (상호작용 성공 피드백)
        Destroy(gameObject);
    }
}
