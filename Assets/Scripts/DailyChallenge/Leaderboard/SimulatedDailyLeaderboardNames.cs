/// <summary>
/// Curated international-neutral first names + surname initials for "Maya K." style.
/// </summary>
public static class SimulatedDailyLeaderboardNames
{
    public static readonly string[] FirstNames =
    {
        "Alex", "Maya", "Luca", "Emma", "Noah", "Sofia", "Leo", "Mila", "Sam", "Nora",
        "Kai", "Lina", "Max", "Aya", "Finn", "Zoe", "Theo", "Sara", "Eli", "Mia",
        "Owen", "Iris", "Jonas", "Hana", "Felix", "Clara", "Hugo", "Yuna", "Nico", "Lara",
        "Omar", "Ines", "Ravi", "Noor", "Elias", "Amira", "Jasper", "Leila", "Mateo", "Anya",
        "Soren", "Tara", "Ivo", "Vera", "Remy", "Nia", "Arlo", "Sena", "Pavel", "Kira"
    };

    public static readonly string[] SurnameInitials =
    {
        "A", "B", "C", "D", "E", "F", "G", "H", "J", "K",
        "L", "M", "N", "P", "R", "S", "T", "V", "W", "Y"
    };

    public static string BuildDisplayName(DailyChallengeDeterministicRng rng, int index)
    {
        int first = rng.NextInt(0, FirstNames.Length);
        // Offset by index so adjacent entries rarely share the same first+initial pair.
        int initial = (rng.NextInt(0, SurnameInitials.Length) + index) % SurnameInitials.Length;
        return FirstNames[first] + " " + SurnameInitials[initial] + ".";
    }
}
