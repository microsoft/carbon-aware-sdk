namespace CarbonAware.Model;

[Serializable]
public record EmissionsData
{
    ///<example> ueast </example>
    public string Location { get; set; }
    ///<example> 2021-09-09 </example>
    public DateTime Time { get; set; }
    ///<example> 38.56 </example>

    public double Rating { get; set; }


    public bool TimeBetween(DateTime fromNotInclusive, DateTime? endInclusive)
    {
        if (endInclusive == null) return false;

        return Time > fromNotInclusive && Time <= endInclusive;
    }

}
