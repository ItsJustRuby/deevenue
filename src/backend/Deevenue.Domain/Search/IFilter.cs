using Deevenue.Domain.Rules;

namespace Deevenue.Domain.Search;

internal interface IFilter
{
    bool Rejects(SmallMediumDocument document, IFilterContext filterContext);
}

internal interface IFilterContext
{
    IRulesService RulesService { get; }
}
