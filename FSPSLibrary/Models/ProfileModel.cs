using System.ComponentModel.DataAnnotations;

namespace FSPSLibrary.Models;

public class ProfileModel
{
    [Required]
    public string Name { get; set; } = string.Empty;
    [Required]
    public string Path { get; set; } = string.Empty;
}
