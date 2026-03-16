using AiChat.Application.DTOs;
using AiChat.Domain.Aggregates.ConversationAggregate;

namespace AiChat.Application.Interfaces;

public interface IConversationService
{
    Task<IEnumerable<ConversationDto>> GetUserConversationsAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 分页获取用户会话列表
    /// </summary>
    Task<PagedResult<ConversationDto>> GetUserConversationsPagedAsync(
        Guid userId,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default);

    Task<ConversationDto?> GetConversationByIdAsync(Guid conversationId, Guid userId, CancellationToken cancellationToken = default);
    Task<ConversationDto> CreateConversationAsync(Guid userId, CreateConversationRequest request, CancellationToken cancellationToken = default);
    Task<ConversationDto?> UpdateConversationAsync(Guid conversationId, Guid userId, UpdateConversationRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteConversationAsync(Guid conversationId, Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<MessageDto>> GetConversationMessagesAsync(Guid conversationId, Guid userId, CancellationToken cancellationToken = default);
    Task<SendMessageResponse> SendMessageAsync(Guid conversationId, Guid userId, SendMessageRequest request, CancellationToken cancellationToken = default);
    Task<MessageDto> AddUserMessageAsync(Guid conversationId, Guid userId, string content, CancellationToken cancellationToken = default);
    Task<MessageDto> AddAssistantMessageAsync(Guid conversationId, Guid userId, string content, CancellationToken cancellationToken = default);

    /// <summary>
    /// 添加 AI 响应消息（带思维链和 Token 统计）
    /// </summary>
    Task<MessageDto> AddAssistantMessageAsync(
        Guid conversationId,
        Guid userId,
        string content,
        string? reasoningContent,
        int? inputTokens,
        int? outputTokens,
        CancellationToken cancellationToken = default);

    // === 高级功能 ===

    /// <summary>
    /// 切换收藏状态
    /// </summary>
    Task<bool> ToggleStarAsync(Guid conversationId, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 设置搜索开关
    /// </summary>
    Task<bool> SetSearchEnabledAsync(Guid conversationId, Guid userId, bool enabled, CancellationToken cancellationToken = default);

    /// <summary>
    /// 设置历史消息策略
    /// </summary>
    Task<bool> SetHistoryTypeAsync(Guid conversationId, Guid userId, HistoryType type, int count, CancellationToken cancellationToken = default);

    /// <summary>
    /// 设置会话头像
    /// </summary>
    Task<bool> SetAvatarAsync(Guid conversationId, Guid userId, string? avatar, AvatarType type, CancellationToken cancellationToken = default);

    /// <summary>
    /// 软删除消息
    /// </summary>
    Task<bool> SoftDeleteMessageAsync(Guid conversationId, Guid messageId, Guid userId, CancellationToken cancellationToken = default);

    // === 对话分享功能 ===

    /// <summary>
    /// 分享对话（生成分享链接）
    /// </summary>
    /// <param name="conversationId">对话ID</param>
    /// <param name="userId">用户ID</param>
    /// <param name="messageIds">要分享的消息ID列表，-1表示全部消息</param>
    Task<string> ShareConversationAsync(Guid conversationId, Guid userId, List<int> messageIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取分享的对话详情
    /// </summary>
    Task<SharedConversationDto?> GetSharedConversationAsync(string shareHash, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取用户的分享列表
    /// </summary>
    Task<IEnumerable<SharedConversationPreviewDto>> ListUserSharedConversationsAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 删除分享
    /// </summary>
    Task<bool> DeleteSharedConversationAsync(string shareHash, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 导入分享的对话（创建新对话）
    /// </summary>
    Task<ConversationDto> ImportSharedConversationAsync(string shareHash, Guid userId, CancellationToken cancellationToken = default);
}

