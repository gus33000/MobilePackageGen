using StorageSpace.Data.Subtypes;

namespace StorageSpace
{
    public class Disk
    {
        public int ID
        {
            get; set;
        }

        public Guid UUID
        {
            get; set;
        }

        public string Name
        {
            get; set;
        }

        public int TotalBlocks
        {
            get; set;
        }

        public List<SlabAllocation> SlabAllocations
        {
            get; set;
        } = [];
    }
}
