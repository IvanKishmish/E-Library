namespace ELibrary.Application.Common.Interfaces;

public interface IInvalidateCacheCommand
{
    string[] CacheKeysToInvalidate { get; }
}