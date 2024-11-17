namespace Deevenue.Domain.Thumbnails;

public class ThumbnailSheetBucket(Guid sheetId) : IBucket
{
    public string Name => $"thumbnail-sheets-{sheetId}";
}
