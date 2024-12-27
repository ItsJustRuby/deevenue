using Deevenue.Domain.Media;
using Microsoft.AspNetCore.Mvc;

namespace Deevenue.Api.Controllers;

public class FileController(IMediumStorage mediumStorage) : DeevenueControllerBase
{
    [HttpGet("{mediumId:Guid}")]
    public async Task<ActionResult> Get(Guid mediumId)
    {
        var maybeFile = await mediumStorage.GetAsync(mediumId);

        if (maybeFile == null)
            return NotFound();

        return File(maybeFile.Bytes, maybeFile.ContentType);
    }
}
