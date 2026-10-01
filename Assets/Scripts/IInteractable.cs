public interface IInteractable
{
    // 상호작용 프롬프트 메시지 (예: "[E] 문 열기", "[E] 아이템 줍기")
    string GetInteractionPrompt();

    // 상호작용 실행 시 호출되는 메서드
    void Interact();
}
