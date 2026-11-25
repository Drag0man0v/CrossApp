using Core;
using Core.Abstractions;
using Core.Storage;

static class StoreFactory
{
    public static ILibraryStore Create(string[] args)
    {
        string dataPath = "library.json";

        ILibraryStore store = args.Contains("--file")
            ? new FileLibraryStore(dataPath)
            : new InMemoryLibraryStore(SampleData.Books());

        if (args.Contains("--cache"))
        {
            return new CachingLibraryStore(store);
        }

        return store;
    }


    public static ILibraryStore Create(string[] args, string dataPath)
    {

        ILibraryStore store = args.Contains("--file")
            ? new FileLibraryStore(dataPath)
            : new InMemoryLibraryStore(SampleData.Books());

        if (args.Contains("--cache"))
        {
            return new CachingLibraryStore(store);
        }

        return store;
    }
}