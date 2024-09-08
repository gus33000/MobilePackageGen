
namespace ToCBS
{
    public interface IDisk
    {
        IEnumerable<IPartition> Partitions
        {
            get;
        }
    }
}
