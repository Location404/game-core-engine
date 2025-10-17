namespace GameCoreEngime.Application.DTOs.Integration;

public record UserDto(
    Guid Id,
    string Username,
    string Email,
    int TotalPoints,
    bool IsInGameMatch
);