using System.Text.RegularExpressions;

namespace Deevenue.Domain.Search;

internal interface IFilterFactory
{
    Regex Regex { get; }
    IFilter Create(GroupCollection groups);
}
