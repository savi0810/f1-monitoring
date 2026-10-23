using System.ComponentModel.DataAnnotations;

namespace Auth.Api.Data.DTOs;

public class LoginDto
{
    [Required(ErrorMessage = "Электронная почта обязательна для заполнения.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Пароль обязателен для заполнения.")]
    public string Password { get; set; } = string.Empty;
}
