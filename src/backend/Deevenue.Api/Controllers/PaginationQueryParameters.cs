using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Deevenue.Api.Controllers;

public class PaginationQueryParameters
{
    [FromQuery(Name = "pageNumber")]
    [BindRequired]
    public required int PageNumber { get; set; }

    [FromQuery(Name = "pageSize")]
    [BindRequired]
    public required int PageSize { get; set; }
}
