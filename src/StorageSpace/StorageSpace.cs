using StorageSpace.Data;
using StorageSpace.Data.Subtypes;
using System.Text;

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

        public static Dictionary<int, string> GetDisks(Stream stream)
        {
            Dictionary<int, string> disks = [];

            StorageSpace storageSpace = new(stream);

            foreach (byte[] SDBBVolume in storageSpace.SDBBVolumes)
            {
                int tempOffset = 0;

                byte DataLength = SDBBVolume[0];
                int Data = BigEndianToInt(SDBBVolume.Skip(1).Take(DataLength).ToArray());

                byte DataLength2 = SDBBVolume[1 + DataLength];

                int CommandSerialNumber = BigEndianToInt(SDBBVolume.Skip(1 + DataLength + 1).Take(DataLength2).ToArray());

                Guid VolumeGUID = new(SDBBVolume.Skip(1 + DataLength + 1 + DataLength2).Take(16).ToArray());

                /*tempOffset += 0x02;

                if (SDBBVolume[tempOffset] == 0x01)
                {
                    tempOffset += SDBBVolume[tempOffset] + 1;
                }

                tempOffset += 0x10;*/

                int VolumeLengthName = BitConverter.ToUInt16(SDBBVolume.Skip(1 + DataLength + 1 + DataLength2 + 16).Take(0x02).Reverse().ToArray(), 0);

                if (VolumeLengthName == 0)
                {
                    continue;
                }

                tempOffset = 1 + DataLength + 1 + DataLength2 + 16 + 0x02;

                byte[] virtualDiskName = new byte[VolumeLengthName * 2];
                byte[] tempVirtualDiskName = SDBBVolume.Skip(tempOffset).Take(VolumeLengthName * 2).ToArray();
                tempOffset += VolumeLengthName * 2;
                for (int j = 0; j < VolumeLengthName * 2; j += 2)
                {
                    virtualDiskName[j] = tempVirtualDiskName[j + 1];
                    virtualDiskName[j + 1] = tempVirtualDiskName[j];
                }

                int diskId = Data;
                string diskName = Encoding.Unicode.GetString(virtualDiskName);

                disks.Add(diskId, diskName);
            }

            return disks;
        }

        private static int BigEndianToInt(byte[] buf)
        {
            int val = 0;
            for (int i = 0; i < buf.Length; i++)
            {
                val *= 0x100;
                val += buf[i];
            }
            return val;
        }
    }
}