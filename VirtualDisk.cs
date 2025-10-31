using System;
using System.IO;

namespace OS_project
{
    public class VirtualDisk
    {
        public readonly int CLUSTER_SIZE;
        public readonly int CLUSTER_NUMBER;
        private string diskPath;
        private FileStream diskFile;
        private bool isOpen;
        private readonly int diskSize;

        public VirtualDisk()
        {
            CLUSTER_SIZE = FSConstants.CLUSTER_SIZE;
            CLUSTER_NUMBER = FSConstants.CLUSTER_COUNT;
            diskPath = null;
            diskFile = null;
            isOpen = false;
            diskSize = CLUSTER_SIZE * CLUSTER_NUMBER;
        }

        public void Initialize(string path, bool createIfMissing = true)
        {
            if (isOpen)
            {
                throw new Exception("Disk already open.");
            }

            diskPath = path;

            // check if file exists
            if (!File.Exists(path))
            {
                if (createIfMissing)
                {
                    CreateEmptyDisk(path);
                }
                else
                {
                    throw new Exception("Disk file not found.");
                }
            }

            diskFile = new FileStream(path, FileMode.Open, FileAccess.ReadWrite);
            isOpen = true;
        }

        // create new disk file
        private void CreateEmptyDisk(string path)
        {
            FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write);
            byte[] empty = new byte[CLUSTER_SIZE];

            for (int i = 0; i < CLUSTER_NUMBER; i++)
            {
                fs.Write(empty, 0, empty.Length);
            }

            fs.Close();
            Console.WriteLine("Created new virtual disk: " + path);
        }

        private void CheckCluster(int clusterNumber)
        {
            if (clusterNumber < 0 || clusterNumber >= CLUSTER_NUMBER)
            {
                throw new Exception("Cluster number out of range.");
            }
        }

        // read data from cluster
        public byte[] ReadCluster(int clusterNumber)
        {
            if (!isOpen)
                throw new Exception("Disk is not opened.");

            CheckCluster(clusterNumber);

            long offset = clusterNumber * CLUSTER_SIZE;
            diskFile.Seek(offset, SeekOrigin.Begin);

            byte[] data = new byte[CLUSTER_SIZE];
            int bytesRead = diskFile.Read(data, 0, CLUSTER_SIZE);

            if (bytesRead < CLUSTER_SIZE)
            {
                throw new Exception("Error: incomplete cluster read.");
            }

            return data;
        }

        // write data to cluster
        public void WriteCluster(int clusterNumber, byte[] data)
        {
            if (!isOpen)
                throw new Exception("Disk not opened.");

            CheckCluster(clusterNumber);

            if (data.Length != CLUSTER_SIZE)
            {
                throw new Exception("Data must be exactly " + CLUSTER_SIZE + " bytes.");
            }

            long offset = clusterNumber * CLUSTER_SIZE;
            diskFile.Seek(offset, SeekOrigin.Begin);
            diskFile.Write(data, 0, data.Length);
            diskFile.Flush();
        }

        public int GetDiskSize()
        {
            return diskSize;
        }

        public void CloseDisk()
        {
            if (isOpen)
            {
                diskFile.Close();
                isOpen = false;
                Console.WriteLine("Disk closed.");
            }
        }
    }
}