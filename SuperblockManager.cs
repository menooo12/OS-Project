using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OS_project
{
    internal class SuperblockManager
    {
        private VirtualDisk disk;

        public SuperblockManager(VirtualDisk virtualDisk)
        {
            if (virtualDisk == null)
                throw new Exception("Virtual disk cannot be null");

            disk = virtualDisk;

            // initialize superblock with zeros
            byte[] empty = new byte[FSConstants.CLUSTER_SIZE];
            disk.WriteCluster(FSConstants.SUPERBLOCK_CLUSTER, empty);
        }

        public byte[] ReadSuperblock()
        {
            return disk.ReadCluster(FSConstants.SUPERBLOCK_CLUSTER);
        }

        public void WriteSuperblock(byte[] data)
        {
            if (data == null)
                throw new Exception("Data cannot be null");

            if (data.Length != FSConstants.CLUSTER_SIZE)
                throw new Exception("Superblock must be exactly " + FSConstants.CLUSTER_SIZE + " bytes.");

            disk.WriteCluster(FSConstants.SUPERBLOCK_CLUSTER, data);
        }
    }
}