using System.ComponentModel.DataAnnotations;

namespace API.DTOs;

public class CreateProductDto
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    [Required]
    [Range(100, double.PositiveInfinity)]
    public long Price { get; set; }

    [Required]
    public IFormFile File { get; set; } = null!; // ! -> override

    [Required]
    public required string Type { get; set; }

    [Required]
    public required string Brand { get; set; }

    [Required]
    [Range(0, 1000)]
    public int QuantityInStock { get; set; }
}
