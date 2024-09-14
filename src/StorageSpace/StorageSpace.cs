using StorageSpace.Data;
using StorageSpace.Data.Subtypes;

namespace StorageSpace
{
    public class StorageSpace
    {
        public SPACEDB SPACEDB
        {
            get; private set;
        }

        public SDBC SDBC
        {
            get; private set;
        }

        public List<SDBB> SDBBs
        {
            get; private set;
        }

        public Dictionary<uint, byte[]> BlockEntries
        {
            get; private set;
        }

        public List<Block> Blocks
        {
            get; private set;
        }

        public List<byte[]> SDBBStorageInformation
        {
            get; private set;
        }

        public List<byte[]> SDBBPhysicalDisks
        {
            get; private set;
        }

        public List<byte[]> SDBBVolumes
        {
            get; private set;
        }

        public List<byte[]> SDBBSlabAllocation
        {
            get; private set;
        }

        public StorageSpace(Stream stream)
        {
            long ogSeek = stream.Position;

            using BinaryReader reader = new(stream);

            SPACEDB = SPACEDB.Parse(stream);

            int SDBCOffset = 0x1000;

            stream.Seek(ogSeek + SDBCOffset, SeekOrigin.Begin);

            SDBC = SDBC.Parse(stream);

            if (SDBC.StorageGUID != SPACEDB.StorageGUID)
            {
                throw new Exception("Invalid OSPool! SDBC is not for the given SpaceDB!");
            }

            stream.Seek(ogSeek + SDBCOffset + SDBC.SDBCLength, SeekOrigin.Begin);

            SDBBStorageInformation = [];
            SDBBPhysicalDisks = [];
            SDBBVolumes = [];
            SDBBSlabAllocation = [];

            BlockEntries = [];

            SDBBs = [];
            Blocks = [];

            for (uint j = 8; j < SDBC.SDBBLength; j++)
            {
                SDBB SDBB = SDBB.Parse(stream);
                SDBBs.Add(SDBB);

                if (SDBB.ParentSDBBIndex == 0) // Empty Entry
                {
                    //throw new Exception("An entry exists which is empty, this is abnormal");
                    continue;
                }

                if (BlockEntries.TryGetValue(SDBB.ParentSDBBIndex, out byte[]? value))
                {
                    BlockEntries[SDBB.ParentSDBBIndex] = [.. value, .. SDBB.Data];
                }
                else
                {
                    BlockEntries[SDBB.ParentSDBBIndex] = SDBB.Data;
                }
            }

            for (uint j = 8; j < SDBC.SDBBLength; j++)
            {
                if (!BlockEntries.ContainsKey(j))
                {
                    continue;
                }

                byte[] BlockBuffer = BlockEntries[j];
                using MemoryStream BlockStream = new(BlockBuffer);

                Block Block = Block.Parse(BlockStream);
                Blocks.Add(Block);

                switch (Block.Type)
                {
                    case BlockType.StoragePool:
                        SDBBStorageInformation.Add(Block.Data);
                        break;
                    case BlockType.PhysicalDisk:
                        SDBBPhysicalDisks.Add(Block.Data);
                        break;
                    case BlockType.Volume:
                        SDBBVolumes.Add(Block.Data);
                        break;
                    case BlockType.SlabAllocation:
                        SDBBSlabAllocation.Add(Block.Data);
                        break;
                    default:
                        throw new Exception($"Unknown Entry Type! {Block.Type}");
                }
            }

            stream.Seek(ogSeek, SeekOrigin.Begin);
        }
    }
}