using System.ComponentModel.DataAnnotations;

public class ChangePasswordDTO
{
    [Required]
    [DataType(DataType.Password)]
    public string? CurrentPassword { get; set; }

    [Required]
    [DataType(DataType.Password)]
    public string? NewPassword { get; set; }
}

// DTOs for RetrievePassword and ResetPassword
public class RetrievePasswordDTO
{
    [Required]
    [DataType(DataType.EmailAddress)]
    public string? Email { get; set; }
}

public class ResetPasswordDTO
{
    [Required]
    public string? UserId { get; set; }
    [Required]
    public string? Token { get; set; }
    [Required]
    [DataType(DataType.Password)]
    public string? NewPassword { get; set; }
}
