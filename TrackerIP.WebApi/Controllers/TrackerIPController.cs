using Microsoft.AspNetCore.Mvc;
using TrackerIP.WebApi.Validation;
using TrackerIP.Domain.Models;

namespace TrackerIP.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TrackerIPController : ControllerBase
{
    private readonly ILogger<TrackerIPController> _logger;
    private readonly ITrackerIPService _trackerIpManager;
    private readonly IValidationFactory _validationFactory;

    public TrackerIPController(
        ILogger<TrackerIPController> logger,
        ITrackerIPService trackerIpManager,
        IValidationFactory validationFactory)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _trackerIpManager = trackerIpManager ?? throw new ArgumentNullException(nameof(trackerIpManager));
        _validationFactory = validationFactory ?? throw new ArgumentNullException(nameof(validationFactory));
    }

    [HttpGet]
    [Route("details/{ip}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IPDetails))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetDetails(string ip)
    {
        try
        {
            var validator = _validationFactory.CreateValidator<string>(ValidationType.IP);
            var validationResult = validator.Validate(ip);
            if (!validationResult.IsValid)
            {
                return BadRequest($"IP {ip} is invalid");
            }

            var result = await _trackerIpManager.GetIPDetailsAsync(ip);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error on fetching IP details for IP {ip}");
            return StatusCode(StatusCodes.Status500InternalServerError, $"Error on fetching IP details for IP {ip}");
        }
    }

    [HttpGet]
    [Route("country-report")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<CountryReport>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetReport([FromQuery] string[] countryCodes = null)
    {
        try
        {
            var validator = _validationFactory.CreateValidator<string[]>(ValidationType.CountryCode);
            var validationResult = validator.Validate(countryCodes);
            if (!validationResult.IsValid)
            {
                return BadRequest("Invalid country code(s). Country codes must be two-letter ISO codes.");
            }

            var result = await _trackerIpManager.GetCountryReportAsync(countryCodes);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while fetching country report data");
            return StatusCode(StatusCodes.Status500InternalServerError, "Error while fetching country report data");
        }
    }

    [HttpPost]
    [Route("update-all")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateIps()
    {
        try
        {
            await _trackerIpManager.UpdateIPDetailsAsync();
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while updating IP details");
            return StatusCode(StatusCodes.Status500InternalServerError, "Error while updating IP details");
        }
    }

}