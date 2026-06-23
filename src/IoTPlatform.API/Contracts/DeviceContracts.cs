using IoTPlatform.Models.Enums;

namespace IoTPlatform.API.Contracts;

public sealed record DeviceDto(
    Guid Id,
    string DeviceKey,
    string Name,
    string DeviceType,
    DeviceStatus Status,
    DateTimeOffset? LastSeenAt,
    Guid? DepartmentId);
