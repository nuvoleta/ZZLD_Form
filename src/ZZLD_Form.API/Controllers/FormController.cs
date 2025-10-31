using Microsoft.AspNetCore.Mvc;
using ZZLD_Form.Core.Services;
using ZZLD_Form.Shared.DTOs;

namespace ZZLD_Form.API.Controllers;

/// <summary>
/// Controller for ZZLD form operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class FormController : ControllerBase
{
    private readonly IFormService _formService;
    private readonly ILogger<FormController> _logger;

    public FormController(IFormService formService, ILogger<FormController> logger)
    {
        _formService = formService ?? throw new ArgumentNullException(nameof(formService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Generates a new ZZLD form
    /// </summary>
    /// <param name="request">Form generation request with personal data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Form generation result with download URL</returns>
    [HttpPost("generate")]
    [ProducesResponseType(typeof(FormGenerationResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<FormGenerationResult>> GenerateForm(
        [FromBody] FormGenerationRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Generating form for {Email}", request.Email);

            var result = await _formService.GenerateFormAsync(request, cancellationToken);

            if (!result.Success)
            {
                _logger.LogWarning("Form generation failed: {ErrorMessage}", result.ErrorMessage);
                return BadRequest(new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Form generation failed",
                    Detail = result.ErrorMessage
                });
            }

            _logger.LogInformation("Form generated successfully with ID {FormId}", result.FormId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error generating form");
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "An unexpected error occurred",
                Detail = ex.Message
            });
        }
    }

    /// <summary>
    /// Retrieves a previously generated form by ID
    /// </summary>
    /// <param name="formId">Form identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Form generation result with download URL</returns>
    [HttpGet("{formId}")]
    [ProducesResponseType(typeof(FormGenerationResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<FormGenerationResult>> GetForm(
        string formId,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Retrieving form {FormId}", formId);

            var result = await _formService.GetFormAsync(formId, cancellationToken);

            if (!result.Success)
            {
                _logger.LogWarning("Form retrieval failed for {FormId}: {ErrorMessage}", formId, result.ErrorMessage);
                return NotFound(new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "Form not found",
                    Detail = result.ErrorMessage
                });
            }

            _logger.LogInformation("Form {FormId} retrieved successfully", formId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving form {FormId}", formId);
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "An unexpected error occurred",
                Detail = ex.Message
            });
        }
    }
}
