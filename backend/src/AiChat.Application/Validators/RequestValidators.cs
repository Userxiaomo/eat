using AiChat.Application.DTOs;
using FluentValidation;

namespace AiChat.Application.Validators;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("用户名不能为空")
            .Length(3, 50).WithMessage("用户名长度必须在3-50个字符之间")
            .Matches("^[a-zA-Z0-9_]+$").WithMessage("用户名只能包含字母、数字和下划线");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("邮箱不能为空")
            .EmailAddress().WithMessage("邮箱格式不正确")
            .MaximumLength(100).WithMessage("邮箱长度不能超过100个字符");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("密码不能为空")
            .MinimumLength(8).WithMessage("密码长度至少8个字符")
            .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)").WithMessage("密码必须包含大写字母、小写字母和数字");
    }
}

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("用户名不能为空")
            .MaximumLength(50).WithMessage("用户名长度不能超过50个字符");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("密码不能为空");
    }
}

public class CreateConversationRequestValidator : AbstractValidator<CreateConversationRequest>
{
    public CreateConversationRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("会话标题不能为空")
            .MaximumLength(200).WithMessage("会话标题长度不能超过200个字符");

        RuleFor(x => x.ModelId)
            .NotEmpty().WithMessage("模型ID不能为空")
            .MaximumLength(50).WithMessage("模型ID长度不能超过50个字符");

        When(x => x.Temperature.HasValue, () =>
        {
            RuleFor(x => x.Temperature!.Value)
                .InclusiveBetween(0.0, 2.0).WithMessage("Temperature必须在0.0-2.0之间");
        });

        When(x => x.MaxTokens.HasValue, () =>
        {
            RuleFor(x => x.MaxTokens!.Value)
                .GreaterThan(0).WithMessage("MaxTokens必须大于0")
                .LessThanOrEqualTo(32000).WithMessage("MaxTokens不能超过32000");
        });

        When(x => !string.IsNullOrEmpty(x.SystemPrompt), () =>
        {
            RuleFor(x => x.SystemPrompt)
                .MaximumLength(1000).WithMessage("系统提示词长度不能超过1000个字符");
        });
    }
}

public class UpdateConversationRequestValidator : AbstractValidator<UpdateConversationRequest>
{
    public UpdateConversationRequestValidator()
    {
        When(x => !string.IsNullOrEmpty(x.Title), () =>
        {
            RuleFor(x => x.Title)
                .MaximumLength(200).WithMessage("会话标题长度不能超过200个字符");
        });

        When(x => !string.IsNullOrEmpty(x.ModelId), () =>
        {
            RuleFor(x => x.ModelId)
                .MaximumLength(50).WithMessage("模型ID长度不能超过50个字符");
        });

        When(x => x.Temperature.HasValue, () =>
        {
            RuleFor(x => x.Temperature!.Value)
                .InclusiveBetween(0.0, 2.0).WithMessage("Temperature必须在0.0-2.0之间");
        });

        When(x => x.MaxTokens.HasValue, () =>
        {
            RuleFor(x => x.MaxTokens!.Value)
                .GreaterThan(0).WithMessage("MaxTokens必须大于0")
                .LessThanOrEqualTo(32000).WithMessage("MaxTokens不能超过32000");
        });

        When(x => !string.IsNullOrEmpty(x.SystemPrompt), () =>
        {
            RuleFor(x => x.SystemPrompt)
                .MaximumLength(1000).WithMessage("系统提示词长度不能超过1000个字符");
        });
    }
}

public class SendMessageRequestValidator : AbstractValidator<SendMessageRequest>
{
    public SendMessageRequestValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("消息内容不能为空")
            .MaximumLength(10000).WithMessage("消息长度不能超过10000个字符");
    }
}

