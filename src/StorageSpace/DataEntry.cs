namespace StorageSpace
{
    public class DataEntry
    {
        public int virtual_disk_id
        {
            get; set;
        }
        public int virtual_disk_block_number
        {
            get; set;
        }
        public int parity_sequence_number
        {
            get; set;
        }
        public int mirror_sequence_number
        {
            get; set;
        }
        public int physical_disk_id
        {
            get; set;
        }
        public int physical_disk_block_number
        {
            get; set;
        }
    }
}
