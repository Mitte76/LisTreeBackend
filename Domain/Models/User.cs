using System.ComponentModel.DataAnnotations;
namespace Domain.Models;

public class User
{
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public required string Username { get; set; }

    [Required]
    [EmailAddress]
    [MaxLength(254)]
    public required string Email { get; set; }
}