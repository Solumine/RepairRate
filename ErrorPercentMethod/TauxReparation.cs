namespace Reparation;

internal class TauxReparation
{
    public string Operator { get; init; }
    public string Card { get; init; }
    public int Ok { get; init; }
    public int Failure { get; init; }
    public DateTime DateTime { get; init; }
    public string Description { get; init;}

    public TauxReparation(string ope, string card, int ok, int failure, DateTime dateTime, string description = null)
    {
        Operator = ope;
        Card = card;
        Ok = ok;
        Failure = failure;
        DateTime = dateTime;
        Description = description;
    }
}
