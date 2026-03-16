namespace AiChat.Domain.Common;

/// <summary>
/// 领域事件接口
/// </summary>
public interface IDomainEvent
{
    DateTime OccurredOn { get; }
}
