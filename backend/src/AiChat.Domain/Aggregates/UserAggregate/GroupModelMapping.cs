namespace AiChat.Domain.Aggregates.UserAggregate;

/// <summary>
/// 分组-模型关联（多对多）
/// </summary>
public class GroupModelMapping
{
    public Guid GroupId { get; private set; }
    public Guid ModelId { get; private set; }

    private GroupModelMapping() { }

    public GroupModelMapping(Guid groupId, Guid modelId)
    {
        GroupId = groupId;
        ModelId = modelId;
    }
}
