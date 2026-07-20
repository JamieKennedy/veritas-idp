using System.ComponentModel.DataAnnotations;

namespace Veritas.PlatformService.Domain.Entities;

public class SystemFlag
{
    [Key]
    public required string Key
    {
        get; set;
    }
    public required bool Value
    {
        get; set;
    }
}
