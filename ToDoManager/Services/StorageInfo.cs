namespace ITTitans.ToDoManager.Services;

internal class StorageInfo : IStorageInfo
{
    public string StorageLabel { get; }

    public StorageInfo(string storageLabel)
    {
        StorageLabel = storageLabel;
    }
}
