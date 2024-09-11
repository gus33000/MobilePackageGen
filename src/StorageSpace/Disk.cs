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

        public List<DataEntry> sdbbEntryType4
        {
            get; set;
        } = new List<DataEntry>();
    }
}
