using System.ComponentModel.DataAnnotations;

namespace TestApiRestPlatform.Models;

public class ValidationModel
{
    [Required]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}
