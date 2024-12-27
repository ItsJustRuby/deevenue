namespace Deevenue.Infrastructure.Jobs;

internal static class Stride
{
    public static List<int> GetEvenlySpacedIndices(int totalCount, int countToPick)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(countToPick, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(totalCount, countToPick);

        var current = 0f;
        var stride = (float)totalCount / countToPick;

        var results = new List<int>();

        while (current <= totalCount - 1)
        {
            results.Add((int)Math.Floor(current));
            current += stride;
        }
        return results;
    }
}
