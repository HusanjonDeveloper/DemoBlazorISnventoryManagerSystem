using System.ComponentModel.DataAnnotations;

namespace Application.DTO.Request.Identity;

public class CreateUserRequestDTO : LoginUserRequestDTO
{
    [Required] 
    public  string Name  { get; set; }
    [Required]
    [Compare(nameof(Password))]
    public  string ConfiromPassword { get; set; }
    [Required] 
    public  string Policy  { get; set; }
}