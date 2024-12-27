namespace Deevenue.Infrastructure.Jobs;

internal static class JobNames
{
    public static readonly IReadOnlyList<string> JobKindNames;
    public static readonly IReadOnlyDictionary<string, string> JobKindNameByJobTypeName;

    static JobNames()
    {
        JobKindNameByJobTypeName = typeof(JobNames).Assembly
            .GetTypes()
            .Where(t => !t.IsInterface && !t.IsAbstract && t.IsAssignableTo(typeof(IDeevenueJob)))
            .ToDictionary(t => t.Name, t => (string?)
                t.GetCustomAttributesData()
                .SingleOrDefault(a => a.AttributeType == typeof(JobKindNameAttribute))
                ?.ConstructorArguments
                .First().Value ?? t.Name);

        JobKindNames = JobKindNameByJobTypeName.Values.ToList();
    }
}
