using AiChat.Application.DTOs;
using AiChat.Application.Interfaces;
using AiChat.Domain.Aggregates.ConversationAggregate;
using AiChat.Domain.Aggregates.UserAggregate;

namespace AiChat.Application.Services;

public class ConversationService : IConversationService
{
    private readonly IConversationRepository _conversationRepository;
    private readonly IAiService _aiService;
    private readonly ISharedConversationRepository _sharedConversationRepository;
    private readonly IUserRepository _userRepository;

    public ConversationService(
        IConversationRepository conversationRepository,
        IAiService aiService,
        ISharedConversationRepository sharedConversationRepository,
        IUserRepository userRepository)
    {
        _conversationRepository = conversationRepository ?? throw new ArgumentNullException(nameof(conversationRepository));
        _aiService = aiService ?? throw new ArgumentNullException(nameof(aiService));
        _sharedConversationRepository = sharedConversationRepository ?? throw new ArgumentNullException(nameof(sharedConversationRepository));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    }

    public async Task<IEnumerable<ConversationDto>> GetUserConversationsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var conversations = await _conversationRepository.GetByUserIdAsync(userId, cancellationToken);

        return conversations.Select(c => new ConversationDto
        {
            Id = c.Id,
            Title = c.Title,
            ModelId = c.ModelId,
            Temperature = c.Settings.Temperature,
            MaxTokens = c.Settings.MaxTokens,
            SystemPrompt = c.Settings.SystemPrompt,
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt
        });
    }

    public async Task<PagedResult<ConversationDto>> GetUserConversationsPagedAsync(
        Guid userId,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var (conversations, totalCount) = await _conversationRepository.GetByUserIdPagedAsync(
            userId, page, pageSize, cancellationToken);

        var items = conversations.Select(c => new ConversationDto
        {
            Id = c.Id,
            Title = c.Title,
            ModelId = c.ModelId,
            Temperature = c.Settings.Temperature,
            MaxTokens = c.Settings.MaxTokens,
            SystemPrompt = c.Settings.SystemPrompt,
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt
        });

        return new PagedResult<ConversationDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<ConversationDto?> GetConversationByIdAsync(Guid conversationId, Guid userId, CancellationToken cancellationToken = default)
    {
        var conversation = await _conversationRepository.GetByIdAsync(conversationId, cancellationToken);

        if (conversation == null || conversation.UserId != userId)
            return null;

        return new ConversationDto
        {
            Id = conversation.Id,
            Title = conversation.Title,
            ModelId = conversation.ModelId,
            Temperature = conversation.Settings.Temperature,
            MaxTokens = conversation.Settings.MaxTokens,
            SystemPrompt = conversation.Settings.SystemPrompt,
            CreatedAt = conversation.CreatedAt,
            UpdatedAt = conversation.UpdatedAt
        };
    }

    public async Task<ConversationDto> CreateConversationAsync(Guid userId, CreateConversationRequest request, CancellationToken cancellationToken = default)
    {
        var settings = new ConversationSettings
        {
            Temperature = request.Temperature ?? 0.7,
            MaxTokens = request.MaxTokens ?? 2000,
            SystemPrompt = request.SystemPrompt
        };

        var conversation = new Conversation(
            Guid.NewGuid(),
            request.Title,
            userId,
            request.ModelId,
            settings
        );

        await _conversationRepository.AddAsync(conversation, cancellationToken);
        await _conversationRepository.SaveChangesAsync(cancellationToken);

        return new ConversationDto
        {
            Id = conversation.Id,
            Title = conversation.Title,
            ModelId = conversation.ModelId,
            Temperature = conversation.Settings.Temperature,
            MaxTokens = conversation.Settings.MaxTokens,
            SystemPrompt = conversation.Settings.SystemPrompt,
            CreatedAt = conversation.CreatedAt,
            UpdatedAt = conversation.UpdatedAt
        };
    }

    public async Task<ConversationDto?> UpdateConversationAsync(Guid conversationId, Guid userId, UpdateConversationRequest request, CancellationToken cancellationToken = default)
    {
        var conversation = await _conversationRepository.GetByIdAsync(conversationId, cancellationToken);

        if (conversation == null || conversation.UserId != userId)
            return null;

        if (!string.IsNullOrWhiteSpace(request.Title))
            conversation.UpdateTitle(request.Title);

        if (!string.IsNullOrWhiteSpace(request.ModelId))
            conversation.ChangeModel(request.ModelId);

        if (request.Temperature.HasValue || request.MaxTokens.HasValue || request.SystemPrompt != null)
        {
            var newSettings = new ConversationSettings
            {
                Temperature = request.Temperature ?? conversation.Settings.Temperature,
                MaxTokens = request.MaxTokens ?? conversation.Settings.MaxTokens,
                SystemPrompt = request.SystemPrompt ?? conversation.Settings.SystemPrompt
            };
            conversation.UpdateSettings(newSettings);
        }

        _conversationRepository.Update(conversation);
        await _conversationRepository.SaveChangesAsync(cancellationToken);

        return new ConversationDto
        {
            Id = conversation.Id,
            Title = conversation.Title,
            ModelId = conversation.ModelId,
            Temperature = conversation.Settings.Temperature,
            MaxTokens = conversation.Settings.MaxTokens,
            SystemPrompt = conversation.Settings.SystemPrompt,
            CreatedAt = conversation.CreatedAt,
            UpdatedAt = conversation.UpdatedAt
        };
    }

    public async Task<bool> DeleteConversationAsync(Guid conversationId, Guid userId, CancellationToken cancellationToken = default)
    {
        var conversation = await _conversationRepository.GetByIdAsync(conversationId, cancellationToken);

        if (conversation == null || conversation.UserId != userId)
            return false;

        _conversationRepository.Delete(conversation);
        await _conversationRepository.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<IEnumerable<MessageDto>> GetConversationMessagesAsync(Guid conversationId, Guid userId, CancellationToken cancellationToken = default)
    {
        var conversation = await _conversationRepository.GetByIdAsync(conversationId, cancellationToken);

        if (conversation == null || conversation.UserId != userId)
            return Enumerable.Empty<MessageDto>();

        return conversation.Messages
            .OrderBy(m => m.CreatedAt)
            .Select(m => new MessageDto
            {
                Id = m.Id,
                ConversationId = conversation.Id,
                Content = m.Content,
                Role = m.Role.ToString(),
                CreatedAt = m.CreatedAt
            });
    }

    public async Task<SendMessageResponse> SendMessageAsync(Guid conversationId, Guid userId, SendMessageRequest request, CancellationToken cancellationToken = default)
    {
        var conversation = await _conversationRepository.GetByIdAsync(conversationId, cancellationToken);

        if (conversation == null || conversation.UserId != userId)
            throw new InvalidOperationException("Conversation not found or access denied");

        // 获取历史消息
        var history = conversation.Messages
            .OrderBy(m => m.CreatedAt)
            .Select(m => new ChatMessage(m.Role.ToString(), m.Content))
            .ToList();

        // Add user message
        var userMessage = conversation.AddMessage(request.Content, MessageRole.User);
        await _conversationRepository.SaveChangesAsync(cancellationToken);

        // Get AI response
        var aiResponse = await _aiService.SendMessageAsync(
            request.Content,
            history,
            conversation.ModelId,
            (float)conversation.Settings.Temperature,
            (int)conversation.Settings.MaxTokens,
            conversation.Settings.SystemPrompt,
            cancellationToken);

        // Add assistant message
        var assistantMessage = conversation.AddMessage(aiResponse, MessageRole.Assistant);
        await _conversationRepository.SaveChangesAsync(cancellationToken);

        return new SendMessageResponse
        {
            UserMessage = new MessageDto
            {
                Id = userMessage.Id,
                ConversationId = conversationId,
                Content = userMessage.Content,
                Role = userMessage.Role.ToString(),
                CreatedAt = userMessage.CreatedAt
            },
            AssistantMessage = new MessageDto
            {
                Id = assistantMessage.Id,
                ConversationId = conversationId,
                Content = assistantMessage.Content,
                Role = assistantMessage.Role.ToString(),
                CreatedAt = assistantMessage.CreatedAt
            }
        };
    }

    public async Task<MessageDto> AddUserMessageAsync(Guid conversationId, Guid userId, string content, CancellationToken cancellationToken = default)
    {
        var conversation = await _conversationRepository.GetByIdAsync(conversationId, cancellationToken);

        if (conversation == null || conversation.UserId != userId)
            throw new InvalidOperationException("Conversation not found or access denied");

        var message = conversation.AddMessage(content, MessageRole.User);
        await _conversationRepository.SaveChangesAsync(cancellationToken);

        return new MessageDto
        {
            Id = message.Id,
            ConversationId = conversationId,
            Content = message.Content,
            Role = message.Role.ToString(),
            CreatedAt = message.CreatedAt
        };
    }

    public async Task<MessageDto> AddAssistantMessageAsync(Guid conversationId, Guid userId, string content, CancellationToken cancellationToken = default)
    {
        var conversation = await _conversationRepository.GetByIdAsync(conversationId, cancellationToken);

        if (conversation == null || conversation.UserId != userId)
            throw new InvalidOperationException("Conversation not found or access denied");

        var message = conversation.AddMessage(content, MessageRole.Assistant);
        await _conversationRepository.SaveChangesAsync(cancellationToken);

        return new MessageDto
        {
            Id = message.Id,
            ConversationId = conversationId,
            Content = message.Content,
            Role = message.Role.ToString(),
            CreatedAt = message.CreatedAt
        };
    }

    /// <summary>
    /// 添加 AI 响应消息（带思维链和 Token 统计）
    /// </summary>
    public async Task<MessageDto> AddAssistantMessageAsync(
        Guid conversationId,
        Guid userId,
        string content,
        string? reasoningContent,
        int? inputTokens,
        int? outputTokens,
        CancellationToken cancellationToken = default)
    {
        var conversation = await _conversationRepository.GetByIdAsync(conversationId, cancellationToken);

        if (conversation == null || conversation.UserId != userId)
            throw new InvalidOperationException("Conversation not found or access denied");

        var message = conversation.AddMessage(content, MessageRole.Assistant, reasoningContent, inputTokens, outputTokens);
        await _conversationRepository.SaveChangesAsync(cancellationToken);

        return new MessageDto
        {
            Id = message.Id,
            ConversationId = conversationId,
            Content = message.Content,
            Role = message.Role.ToString(),
            ReasoningContent = message.ReasoningContent,
            InputTokens = message.InputTokens,
            OutputTokens = message.OutputTokens,
            CreatedAt = message.CreatedAt
        };
    }

    // === 高级功能实现 ===

    public async Task<bool> ToggleStarAsync(Guid conversationId, Guid userId, CancellationToken cancellationToken = default)
    {
        var conversation = await _conversationRepository.GetByIdAsync(conversationId, cancellationToken);

        if (conversation == null || conversation.UserId != userId)
            return false;

        if (conversation.IsStar)
            conversation.Unstar();
        else
            conversation.Star();

        _conversationRepository.Update(conversation);
        await _conversationRepository.SaveChangesAsync(cancellationToken);

        return conversation.IsStar;
    }

    public async Task<bool> SetSearchEnabledAsync(Guid conversationId, Guid userId, bool enabled, CancellationToken cancellationToken = default)
    {
        var conversation = await _conversationRepository.GetByIdAsync(conversationId, cancellationToken);

        if (conversation == null || conversation.UserId != userId)
            return false;

        conversation.SetSearchEnabled(enabled);
        _conversationRepository.Update(conversation);
        await _conversationRepository.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> SetHistoryTypeAsync(Guid conversationId, Guid userId, HistoryType type, int count, CancellationToken cancellationToken = default)
    {
        var conversation = await _conversationRepository.GetByIdAsync(conversationId, cancellationToken);

        if (conversation == null || conversation.UserId != userId)
            return false;

        conversation.SetHistoryType(type, count);
        _conversationRepository.Update(conversation);
        await _conversationRepository.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> SetAvatarAsync(Guid conversationId, Guid userId, string? avatar, AvatarType type, CancellationToken cancellationToken = default)
    {
        var conversation = await _conversationRepository.GetByIdAsync(conversationId, cancellationToken);

        if (conversation == null || conversation.UserId != userId)
            return false;

        conversation.SetAvatar(avatar, type);
        _conversationRepository.Update(conversation);
        await _conversationRepository.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> SoftDeleteMessageAsync(Guid conversationId, Guid messageId, Guid userId, CancellationToken cancellationToken = default)
    {
        var conversation = await _conversationRepository.GetByIdAsync(conversationId, cancellationToken);

        if (conversation == null || conversation.UserId != userId)
            return false;

        conversation.SoftDeleteMessage(messageId);
        _conversationRepository.Update(conversation);
        await _conversationRepository.SaveChangesAsync(cancellationToken);

        return true;
    }

    // === 对话分享功能实现 ===

    public async Task<string> ShareConversationAsync(Guid conversationId, Guid userId, List<int> messageIds, CancellationToken cancellationToken = default)
    {
        var conversation = await _conversationRepository.GetByIdAsync(conversationId, cancellationToken);
        if (conversation == null || conversation.UserId != userId)
            throw new InvalidOperationException("Conversation not found or access denied");

        // 生成唯一分享Hash（base64编码避免特殊字符）
        var shareHash = Convert.ToBase64String(Guid.NewGuid().ToByteArray())
            .Replace("/", "_")
            .Replace("+", "-")
            .Substring(0, 16);

        // 序列化消息ID列表
        var messageIdsJson = System.Text.Json.JsonSerializer.Serialize(messageIds);

        // 创建分享记录
        var sharedConversation = new Domain.Aggregates.ConversationAggregate.SharedConversation(
            Guid.NewGuid(),
            shareHash,
            userId,
            conversationId,
            messageIds  // 传入List<int>而不是JSON字符串
        );

        await _sharedConversationRepository.AddAsync(sharedConversation, cancellationToken);
        await _sharedConversationRepository.SaveChangesAsync(cancellationToken);

        return shareHash;
    }

    public async Task<SharedConversationDto?> GetSharedConversationAsync(string shareHash, CancellationToken cancellationToken = default)
    {
        var shared = await _sharedConversationRepository.GetByShareHashAsync(shareHash, cancellationToken);
        if (shared == null)
            return null;

        var conversation = await _conversationRepository.GetByIdAsync(shared.ConversationId, cancellationToken);
        if (conversation == null)
            return null;

        var user = await _userRepository.GetByIdAsync(shared.UserId, cancellationToken);
        if (user == null)
            return null;

        // 反序列化消息ID列表
        var messageIds = System.Text.Json.JsonSerializer.Deserialize<List<int>>(shared.MessageIds) ?? new List<int>();

        // 获取消息
        IEnumerable<Domain.Aggregates.ConversationAggregate.Message> messages;
        if (messageIds.Contains(-1))
        {
            // -1表示分享全部消息
            messages = conversation.Messages;
        }
        else
        {
            // 只分享指定消息
            messages = conversation.Messages.Where((m, index) => messageIds.Contains(index)).ToList();
        }

        return new SharedConversationDto
        {
            ShareHash = shared.ShareHash,
            Username = user.Username,
            ConversationName = conversation.Title,
            ModelId = conversation.ModelId,
            Messages = messages.Select(m => new MessageDto
            {
                Role = m.Role.ToString(),
                Content = m.Content,
                CreatedAt = m.CreatedAt,
                InputTokens = m.InputTokens,
                OutputTokens = m.OutputTokens
            }),
            SharedAt = shared.CreatedAt
        };
    }

    public async Task<IEnumerable<SharedConversationPreviewDto>> ListUserSharedConversationsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var sharedList = await _sharedConversationRepository.GetByUserIdAsync(userId, cancellationToken);

        var result = new List<SharedConversationPreviewDto>();
        foreach (var shared in sharedList)
        {
            var conversation = await _conversationRepository.GetByIdAsync(shared.ConversationId, cancellationToken);
            if (conversation != null)
            {
                result.Add(new SharedConversationPreviewDto
                {
                    ShareHash = shared.ShareHash,
                    ConversationId = shared.ConversationId,
                    ConversationName = conversation.Title,
                    CreatedAt = shared.CreatedAt,
                    UpdatedAt = shared.UpdatedAt
                });
            }
        }

        return result;
    }

    public async Task<bool> DeleteSharedConversationAsync(string shareHash, Guid userId, CancellationToken cancellationToken = default)
    {
        var shared = await _sharedConversationRepository.GetByShareHashAsync(shareHash, cancellationToken);
        if (shared == null)
            return false;

        // 验证所有权
        if (shared.UserId != userId)
            throw new InvalidOperationException("Access denied");

        _sharedConversationRepository.Delete(shared);
        await _sharedConversationRepository.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<ConversationDto> ImportSharedConversationAsync(string shareHash, Guid userId, CancellationToken cancellationToken = default)
    {
        var sharedDto = await GetSharedConversationAsync(shareHash, cancellationToken);
        if (sharedDto == null)
            throw new InvalidOperationException("Shared conversation not found");

        // 创建新对话
        var newConversation = new Domain.Aggregates.ConversationAggregate.Conversation(
            Guid.NewGuid(),
            $"[导入] {sharedDto.ConversationName}",  // title
            userId,  // userId
            sharedDto.ModelId  // modelId
        );

        // 复制消息
        foreach (var message in sharedDto.Messages)
        {
            newConversation.AddMessage(
                message.Content,
                Enum.Parse<Domain.Aggregates.ConversationAggregate.MessageRole>(message.Role),
                null,  // reasoning
                message.InputTokens,
                message.OutputTokens
            );
        }

        await _conversationRepository.AddAsync(newConversation, cancellationToken);
        await _conversationRepository.SaveChangesAsync(cancellationToken);

        return new ConversationDto
        {
            Id = newConversation.Id,
            Title = newConversation.Title,
            ModelId = newConversation.ModelId,
            CreatedAt = newConversation.CreatedAt,
            UpdatedAt = newConversation.UpdatedAt
        };
    }
}
