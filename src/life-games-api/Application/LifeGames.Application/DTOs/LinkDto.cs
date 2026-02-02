namespace LifeGames.Application.DTOs;

public record LinkDto(string Href, string Rel, string Method = "GET");
