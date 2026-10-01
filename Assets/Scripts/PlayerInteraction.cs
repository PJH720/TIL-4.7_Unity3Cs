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

    private void Awake()
    {
        if (cameraTransform == null)
        {
            Camera cam = GetComponentInChildren<Camera>();
            if (cam != null)
            {
                cameraTransform = cam.transform;
            }
        }
    }

    private void Update()
    {
        // New Input System 키보드/마우스 직접 폴링 지원 (PlayerInput 유무와 무관하게 즉시 동작)
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            TryInteract();
        }
        else if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            TryInteract();
        }

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

    public void TryInteract()
    {
        if (currentTarget != null)
        {
            currentTarget.Interact();
        }
    }

    // New Input System: Interact Action (E 키 또는 액션 매핑)
    public void OnInteract(InputValue value)
    {
        if (value.isPressed)
        {
            TryInteract();
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

    // 화면 UI에 조준점(Crosshair) 및 상호작용 프롬프트 표시
    private void OnGUI()
    {
        // 1. 화면 정중앙 조준점 (+)
        GUIStyle crosshairStyle = new GUIStyle();
        crosshairStyle.fontSize = 20;
        crosshairStyle.normal.textColor = Color.white;
        crosshairStyle.alignment = TextAnchor.MiddleCenter;
        GUI.Label(new Rect(Screen.width / 2f - 10, Screen.height / 2f - 10, 20, 20), "+", crosshairStyle);

        // 2. 상호작용 가능 시 안내 문구
        if (!string.IsNullOrEmpty(currentPrompt))
        {
            GUIStyle promptStyle = new GUIStyle();
            promptStyle.fontSize = 22;
            promptStyle.fontStyle = FontStyle.Bold;
            promptStyle.normal.textColor = Color.yellow;
            promptStyle.alignment = TextAnchor.MiddleCenter;

            Rect rect = new Rect(Screen.width / 2f - 200, Screen.height / 2f + 40, 400, 40);
            GUI.Label(rect, currentPrompt, promptStyle);
        }
    }
}
