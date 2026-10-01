using System.Collections;
using UnityEngine;

// 필수과제 2-2: 상호작용 성공 시 눈에 보이는 반응 (문 열기/닫기)
public class DoorInteractable : MonoBehaviour, IInteractable
{
    [Header("Door Settings")]
    [SerializeField] private float openAngle = 90f;
    [SerializeField] private float smoothSpeed = 3f;
    [SerializeField] private MeshRenderer doorRenderer;
    [SerializeField] private Color activeColor = Color.green;

    private bool isOpen = false;
    private Quaternion closedRotation;
    private Quaternion openRotation;
    private Coroutine rotateCoroutine;
    private Color originalColor;

    private void Awake()
    {
        closedRotation = transform.localRotation;
        openRotation = closedRotation * Quaternion.Euler(0f, openAngle, 0f);

        if (doorRenderer != null && doorRenderer.material != null)
        {
            originalColor = doorRenderer.material.color;
        }
    }

    public string GetInteractionPrompt()
    {
        return isOpen ? "[E] 문 닫기" : "[E] 문 열기";
    }

    public void Interact()
    {
        isOpen = !isOpen;

        // 1. 회전 애니메이션 실행
        if (rotateCoroutine != null)
            StopCoroutine(rotateCoroutine);

        Quaternion targetRot = isOpen ? openRotation : closedRotation;
        rotateCoroutine = StartCoroutine(SmoothRotate(targetRot));

        // 2. 색상 피드백 변경 (눈에 보이는 반응)
        if (doorRenderer != null && doorRenderer.material != null)
        {
            doorRenderer.material.color = isOpen ? activeColor : originalColor;
        }

        Debug.Log($"[DoorInteractable] 문 상태 변경: {(isOpen ? "열림" : "닫힘")}");
    }

    private IEnumerator SmoothRotate(Quaternion targetRotation)
    {
        while (Quaternion.Angle(transform.localRotation, targetRotation) > 0.5f)
        {
            transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, Time.deltaTime * smoothSpeed);
            yield return null;
        }
        transform.localRotation = targetRotation;
    }
}
