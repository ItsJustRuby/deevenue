using System.Text.RegularExpressions;

namespace Deevenue.Domain.Search;

internal interface ISorterFactory
{
    Regex Regex { get; }
    ISorter Create(GroupCollection groups);
}
