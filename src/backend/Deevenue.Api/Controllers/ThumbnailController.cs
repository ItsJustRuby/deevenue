using Deevenue.Domain.Thumbnails;
using Microsoft.AspNetCore.Mvc;

namespace Deevenue.Api.Controllers;

public class ThumbnailController(IThumbnailStorage storage) : DeevenueControllerBase
{
    [HttpGet("{mediumId:Guid}_{size:ThumbnailSizeAbbreviation}.{extension:regex(^(jpg)|(webm)$)}", Name = "getThumbnail")]
    public async Task<ActionResult> Get(Guid mediumId, ThumbnailSizeAbbreviation size, string extension)
    {
        var isAnimated = extension == "webm";
        var thumbnail = await storage.GetAsync(mediumId, isAnimated, ThumbnailSizes.ByAbbreviation(size));

        if (thumbnail == null)
            // This is fine, the frontend can display a CSS spinner if it wants to.
            return NotFound();

        // Thanks to ThumbnailSheets, thumbnail contents can change.
        // Since their URLs however do not, we have to tell clients that they must
        // first revalidate with the backend if they need to fetch the new thumbnail.
        // That is what "no-cache" achieves (do not mind the name).
        Response.Headers.Append("Cache-Control", "no-cache");
        return File(thumbnail.Bytes, thumbnail.ContentType);
    }
}
