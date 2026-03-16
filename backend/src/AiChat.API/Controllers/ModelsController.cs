using AiChat.API.Filters;
using AiChat.Application.DTOs;
using AiChat.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiChat.API.Controllers;

[Authorize]
[AdminOnly]
[ApiController]
[Route("api/admin/[controller]")]
public class ModelsController : ControllerBase
{
    private readonly IAiModelService _modelService;
    private readonly ILogger<ModelsController> _logger;

    public ModelsController(IAiModelService modelService, ILogger<ModelsController> logger)
    {
        _modelService = modelService ?? throw new ArgumentNullException(nameof(modelService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AiModelDto>>> GetModels()
    {
        try
        {
            var models = await _modelService.GetAllModelsAsync();
            return Ok(models);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving models");
            return StatusCode(500, new { message = "An error occurred while retrieving models" });
        }
    }

    [HttpGet("by-channel/{channelId}")]
    public async Task<ActionResult<IEnumerable<AiModelDto>>> GetModelsByChannel(Guid channelId)
    {
        try
        {
            var models = await _modelService.GetModelsByChannelAsync(channelId);
            return Ok(models);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving models for channel {ChannelId}", channelId);
            return StatusCode(500, new { message = "An error occurred while retrieving models" });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AiModelDto>> GetModel(Guid id)
    {
        try
        {
            var model = await _modelService.GetModelByIdAsync(id);
            if (model == null)
                return NotFound(new { message = "Model not found" });

            return Ok(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving model {ModelId}", id);
            return StatusCode(500, new { message = "An error occurred while retrieving the model" });
        }
    }

    [HttpPost]
    public async Task<ActionResult<AiModelDto>> CreateModel([FromBody] CreateAiModelRequest request)
    {
        try
        {
            var model = await _modelService.CreateModelAsync(request);
            return CreatedAtAction(nameof(GetModel), new { id = model.Id }, model);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating model");
            return StatusCode(500, new { message = "An error occurred while creating the model" });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<AiModelDto>> UpdateModel(Guid id, [FromBody] UpdateAiModelRequest request)
    {
        try
        {
            var model = await _modelService.UpdateModelAsync(id, request);
            if (model == null)
                return NotFound(new { message = "Model not found" });

            return Ok(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating model {ModelId}", id);
            return StatusCode(500, new { message = "An error occurred while updating the model" });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteModel(Guid id)
    {
        try
        {
            var success = await _modelService.DeleteModelAsync(id);
            if (!success)
                return NotFound(new { message = "Model not found" });

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting model {ModelId}", id);
            return StatusCode(500, new { message = "An error occurred while deleting the model" });
        }
    }
}
