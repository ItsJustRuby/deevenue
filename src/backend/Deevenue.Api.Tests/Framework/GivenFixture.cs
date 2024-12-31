
namespace Deevenue.Api.Tests.Framework;

internal class GivenFixture
{
    public static readonly GivenFixture Given = new();
    public readonly SessionFixture Session = new();
    public readonly FileUploadFixture FileUpload = new();
}
