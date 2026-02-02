namespace LifeGames.Application.Options;

public class BoardOptions
{
    public const string SectionName = "Board";

    public int MaxIterationsForFinalState { get; set; } = 10000;
}
