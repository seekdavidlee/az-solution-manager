namespace AzSolutionManager.Core;

public interface IOneTimeOutWriter
{
    void Write<T>(T obj, bool compress = false);
}


