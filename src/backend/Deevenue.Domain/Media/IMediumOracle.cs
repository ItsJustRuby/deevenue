namespace Deevenue.Domain.Media;

public interface IMediumOracle
{
    public MediaKind GuessMediaKind(MediumFileData mediumFileData);
}
