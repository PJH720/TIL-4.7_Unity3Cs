using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintSpeed = 8.5f; // 추가 Challenge: 달리기
    [SerializeField] private float gravity = -15f;
    [SerializeField] private float jumpHeight = 1.5f; // 추가 Challenge: 점프
    [SerializeField] private int maxJumpCount = 2; // 추가 Challenge: 2단 점프

    [Header("Camera & Look Settings (1인칭 시점)")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float lookSensitivityX = 0.15f;
    [SerializeField] private float lookSensitivityY = 0.15f;
    [SerializeField] private float minPitch = -85f;
    [SerializeField] private float maxPitch = 85f;

    private CharacterController controller;
    private Vector2 moveInput;
    private Vector2 lookInput;
    private bool isSprinting;
    private float currentSpeed;
    private float verticalVelocity;
    private int currentJumpCount;
    private float cameraPitch = 0f;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        currentSpeed = walkSpeed;

        if (cameraTransform == null)
        {
            Camera cam = GetComponentInChildren<Camera>();
            if (cam != null)
            {
                cameraTransform = cam.transform;
            }
        }

        // 마우스 커서 잠금 (1인칭 조작 시 화면 중앙 고정)
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        // 1. New Input System 기기 직접 폴링 지원 (PlayerInput 액션 매핑 유무와 무관하게 즉시 동작)
        if (Keyboard.current != null)
        {
            Vector2 kMove = Vector2.zero;
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) kMove.y += 1f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) kMove.y -= 1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) kMove.x += 1f;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) kMove.x -= 1f;

            if (kMove != Vector2.zero || moveInput == Vector2.zero)
            {
                moveInput = kMove;
            }

            isSprinting = Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed;

            if (Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                TryJump();
            }

            // ESC 키로 마우스 커서 토글 (테스트 편의)
            if (Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                Cursor.lockState = Cursor.lockState == CursorLockMode.Locked ? CursorLockMode.None : CursorLockMode.Locked;
                Cursor.visible = (Cursor.lockState != CursorLockMode.Locked);
            }
        }

        if (Mouse.current != null)
        {
            lookInput = Mouse.current.delta.ReadValue();
        }

        HandleRotation();
        HandleMovement();
    }

    // --- 1인칭 카메라 및 시점 회전 처리 (과제 1-2) ---
    private void HandleRotation()
    {
        // 1. 좌우 회전 (Yaw) -> 캐릭터 본체 회전
        transform.Rotate(Vector3.up * lookInput.x * lookSensitivityX);

        // 2. 상하 회전 (Pitch) -> 카메라만 상하 각도 조절 (목 꺾임 제한)
        if (cameraTransform != null)
        {
            cameraPitch -= lookInput.y * lookSensitivityY;
            cameraPitch = Mathf.Clamp(cameraPitch, minPitch, maxPitch);
            cameraTransform.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);
        }
    }

    // --- 이동 및 점프/중력 처리 (과제 1-1, 추가 챌린지) ---
    private void HandleMovement()
    {
        // 바닥 접지 상태 체크
        if (controller.isGrounded)
        {
            currentJumpCount = 0; // 접지 시 점프 카운트 리셋
            if (verticalVelocity < 0f)
            {
                verticalVelocity = -2f; // 바닥에 안정적으로 밀착
            }
        }

        // 달리기 속도 결정 (Shift 입력 시 sprintSpeed 적용)
        currentSpeed = isSprinting ? sprintSpeed : walkSpeed;

        // 플레이어 정면/우측 로컬 방향 기준 평면 이동 벡터 계산
        Vector3 moveDirection = (transform.forward * moveInput.y + transform.right * moveInput.x);
        if (moveDirection.sqrMagnitude > 1f)
            moveDirection.Normalize();

        // 중력 가속도 적용 (과도한 낙하 속도 제한)
        verticalVelocity += gravity * Time.deltaTime;
        verticalVelocity = Mathf.Max(verticalVelocity, -25f);

        // 최종 이동 벡터 합성 (X/Z 평면 이동 + Y축 중력/점프)
        Vector3 finalVelocity = (moveDirection * currentSpeed) + (Vector3.up * verticalVelocity);

        // Time.deltaTime을 곱해 프레임레이트 독립적인 이동 보정
        controller.Move(finalVelocity * Time.deltaTime);
    }

    private void TryJump()
    {
        if (controller.isGrounded || currentJumpCount < maxJumpCount)
        {
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            currentJumpCount++;
        }
    }

    // --- New Input System Callback Events (PlayerInput 컴포넌트 지원) ---

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    public void OnLook(InputValue value)
    {
        lookInput = value.Get<Vector2>();
    }

    public void OnSprint(InputValue value)
    {
        isSprinting = value.isPressed;
    }

    public void OnJump(InputValue value)
    {
        if (value.isPressed)
        {
            TryJump();
        }
    }
}
