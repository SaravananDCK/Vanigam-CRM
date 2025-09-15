using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Vanigam.CRM.Data;
using Vanigam.CRM.Services;

namespace Vanigam.CRM.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
[EnableRateLimiting("Fixed")]
public class ImageController(ImageService _imageService, ILogger<ImageController> _logger) : ControllerBase
{
    private readonly ILogger<ImageController> logger = _logger;
    private readonly ImageService imageService = _imageService;

    [HttpPost]
    public async Task<IActionResult> PostAsync(IFormFile image)
    {
        var extension = image.ContentType switch
        {
            "image/png" => ".png",
            "image/jpeg" => ".jpg",
            _ => null
        };

        try
        {
            using var stream = image.OpenReadStream();
            return Ok($"\"{await imageService.SaveToUploadsAsync(extension, stream)}\"");
        }
        catch (ArgumentException ex)
        {
            return BadRequest($"\"{ex.Message}\"");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"\"{ex.Message}\"");
        }
    }
}
