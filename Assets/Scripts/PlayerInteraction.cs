using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Raycast Interaction Settings (과제 2-1)")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float rayDistance = 3.5f;
    [SerializeField] private LayerMask interactableLayer = ~0; // 기본 전체 감지

    [Header("UI Feedback")]
    [SerializeField] private string currentPrompt = "";

    private IInteractable currentTarget;

    private void Update()
    {
        PerformRaycast();
    }

    // 1인칭 카메라 정면 기준 Raycast 탐색
    private void PerformRaycast()
    {
        if (cameraTransform == null) return;

        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
        RaycastHit hit;

        // Raycast를 쏘아 IInteractable 컴포넌트가 있는지 확인
        if (Physics.Raycast(ray, out hit, rayDistance, interactableLayer))
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();
            if (interactable != null)
            {
                currentTarget = interactable;
                currentPrompt = interactable.GetInteractionPrompt();
                return;
            }
        }

        // 감지된 오브젝트가 없으면 타겟 초기화
        currentTarget = null;
        currentPrompt = "";
    }

    // New Input System: Interact Action (E 키 또는 좌클릭)
    public void OnInteract(InputValue value)
    {
        if (value.isPressed && currentTarget != null)
        {
            currentTarget.Interact();
        }
    }

    // --- (대체 방식) Trigger 기반 근접 상호작용 지원 ---
    private void OnTriggerEnter(Collider other)
    {
        IInteractable interactable = other.GetComponent<IInteractable>();
        if (interactable != null && currentTarget == null)
        {
            currentTarget = interactable;
            currentPrompt = interactable.GetInteractionPrompt();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        IInteractable interactable = other.GetComponent<IInteractable>();
        if (interactable != null && currentTarget == interactable)
        {
            currentTarget = null;
            currentPrompt = "";
        }
    }

    // Scene 뷰에서 Raycast 감지 사거리 시각화
    private void OnDrawGizmosSelected()
    {
        if (cameraTransform != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(cameraTransform.position, cameraTransform.forward * rayDistance);
        }
    }

    // 화면 UI에 현재 상호작용 프롬프트 표시 (디버그 및 피드백용)
    private void OnGUI()
    {
        if (!string.IsNullOrEmpty(currentPrompt))
        {
            GUIStyle style = new GUIStyle();
            style.fontSize = 22;
            style.normal.textColor = Color.yellow;
            style.alignment = TextAnchor.MiddleCenter;

            // 화면 정중앙 하단에 안내 문구 출력
            Rect rect = new Rect(Screen.width / 2f - 150, Screen.height / 2f + 50, 300, 40);
            GUI.Label(rect, currentPrompt, style);
        }
    }
}
