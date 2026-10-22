using System.ComponentModel.DataAnnotations;

namespace Auth.Api.Tests.Unit;

public class DtoValidationTests
{
    private static bool IsValid(object dto) =>
        Validator.TryValidateObject(dto, new ValidationContext(dto), new List<ValidationResult>(), true);
}
