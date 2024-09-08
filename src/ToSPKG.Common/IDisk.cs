
namespace ToSPKG
{
    public interface IDisk
    {
        IEnumerable<IPartition> Partitions
        {
            get;
        }
    }
}
