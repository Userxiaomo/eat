using AiChat.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiChat.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AvailableModelsController : ControllerBase
{
    private readonly IAiModelService _modelService;
    private readonly ILogger<AvailableModelsController> _logger;

    public AvailableModelsController(IAiModelService modelService, ILogger<AvailableModelsController> logger)
    {
        _modelService = modelService ?? throw new ArgumentNullException(nameof(modelService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// 获取所有可用的AI模型（公开端点，供前端使用）
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAvailableModels()
    {
        try
        {
            var models = await _modelService.GetAllModelsAsync();

            // 映射为前端需要的格式
            var result = models.Select(m => new
            {
                id = m.ModelId, // 使用ModelId作为前端的id
                name = m.Name,
                description = m.ModelType, // 使用ModelType作为description
                provider = m.ChannelName, // 使用ChannelName作为provider
                contextWindow = m.MaxTokens, // 使用MaxTokens作为contextWindow
                isEnabled = m.IsEnabled
            });

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving available models");
            return StatusCode(500, new { message = "An error occurred while retrieving models" });
        }
    }

    /// <summary>
    /// 获取单个模型详情
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetModel(Guid id)
    {
        try
        {
            var model = await _modelService.GetModelByIdAsync(id);
            if (model == null)
                return NotFound(new { message = "Model not found" });

            var result = new
            {
                id = model.ModelId,
                name = model.Name,
                description = model.ModelType,
                provider = model.ChannelName,
                contextWindow = model.MaxTokens,
                isEnabled = model.IsEnabled
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving model {ModelId}", id);
            return StatusCode(500, new { message = "An error occurred while retrieving the model" });
        }
    }
}
