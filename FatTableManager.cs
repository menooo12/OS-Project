using System;
using System.Collections.Generic;

namespace OS_project
{
    public class FatTableManager
    {
        private VirtualDisk disk;
        private int[] fat;
        private int CLUSTER_SIZE;
        private int CLUSTER_COUNT;
        private int FAT_ENTRIES;

        public FatTableManager(VirtualDisk disk)
        {
            this.disk = disk;
            this.CLUSTER_SIZE = disk.CLUSTER_SIZE;
            this.CLUSTER_COUNT = FSConstants.CLUSTER_COUNT;
            this.FAT_ENTRIES = FSConstants.CLUSTER_COUNT;
            fat = new int[FAT_ENTRIES];

            // check if FAT clusters are enough
            int entriesPerCluster = CLUSTER_SIZE / 4;
            int fatClusters = FSConstants.FAT_CLUSTER_COUNT;
            if (entriesPerCluster * fatClusters < FAT_ENTRIES)
            {
                throw new Exception("Not enough FAT clusters!");
            }
            InitializeFat();
        }
        // Initialize FAT with default values
        private void InitializeFat()
        {
            for (int i = 0; i <= FSConstants.FAT_END_CLUSTER; i++)
            {
                fat[i] = -1;
            }
            for (int i = FSConstants.FAT_END_CLUSTER + 1; i < FAT_ENTRIES; i++)
            {
                fat[i] = 0;
            }
        }
        // Load FAT from disk
        public void LoadFatFromDisk()
        {
            int totalBytes = FAT_ENTRIES * 4;
            byte[] buffer = new byte[totalBytes];
            int offset = 0;
            for (int c = FSConstants.FAT_START_CLUSTER; c <= FSConstants.FAT_END_CLUSTER; c++)
            {
                byte[] clusterData = disk.ReadCluster(c);
                int bytesToCopy = Math.Min(clusterData.Length, buffer.Length - offset);
                Array.Copy(clusterData, 0, buffer, offset, bytesToCopy);
                offset += bytesToCopy;
            }
            for (int i = 0; i < FAT_ENTRIES; i++)
            {
                fat[i] = BitConverter.ToInt32(buffer, i * 4);
            }
        }
        // Save FAT to disk
        public void FlushFatToDisk()
        {
            int totalBytes = FAT_ENTRIES * 4;
            byte[] buffer = new byte[totalBytes];
            for (int i = 0; i < FAT_ENTRIES; i++)
            {
                byte[] entryBytes = BitConverter.GetBytes(fat[i]);
                Array.Copy(entryBytes, 0, buffer, i * 4, 4);
            }
            int offset = 0;
            for (int c = FSConstants.FAT_START_CLUSTER; c <= FSConstants.FAT_END_CLUSTER; c++)
            {
                byte[] clusterData = new byte[CLUSTER_SIZE];
                int bytesToCopy = Math.Min(CLUSTER_SIZE, buffer.Length - offset);
                Array.Copy(buffer, offset, clusterData, 0, bytesToCopy);
                disk.WriteCluster(c, clusterData);
                offset += bytesToCopy;
            }
        }
        // Get value at index
        public int GetFatEntry(int index)
        {
            if (index < 0 || index >= FAT_ENTRIES)
                throw new Exception("Index out of range!");

            return fat[index];
        }
        // Set value at index
        public void SetFatEntry(int index, int value)
        {
            if (index < 0 || index >= FAT_ENTRIES)
                throw new Exception("Index out of range!");
            if (index <= FSConstants.FAT_END_CLUSTER)
                throw new Exception("Can't modify reserved clusters!");

            fat[index] = value;
        }
        // Return copy of whole FAT
        public int[] ReadAllFat()
        {
            int[] copy = new int[FAT_ENTRIES];
            Array.Copy(fat, copy, FAT_ENTRIES);
            return copy;
        }
        // Write whole FAT array
        public void WriteAllFat(int[] entries)
        {
            if (entries == null)
                throw new Exception("Entries cannot be null!");
            if (entries.Length != FAT_ENTRIES)
                throw new Exception("Wrong FAT size!");
            for (int i = 0; i <= FSConstants.FAT_END_CLUSTER; i++)
            {
                if (entries[i] != fat[i])
                    throw new Exception("Can't change reserved clusters!");
            }
            Array.Copy(entries, fat, FAT_ENTRIES);
        }
        // Follow chain and return list of clusters
        public List<int> FollowChain(int startCluster)
        {
            if (startCluster < 0 || startCluster >= FAT_ENTRIES)
                throw new Exception("Invalid cluster index!");

            if (startCluster <= FSConstants.FAT_END_CLUSTER)
                throw new Exception("Start cluster is reserved!");

            List<int> chain = new List<int>();
            HashSet<int> visited = new HashSet<int>();
            int current = startCluster;

            while (true)
            {
                if (current < 0 || current >= FAT_ENTRIES)
                    throw new Exception("Chain has invalid cluster!");
                if (visited.Contains(current))
                    throw new Exception("Chain has a loop!");
                visited.Add(current);
                chain.Add(current);
                int next = fat[current];
                if (next == -1)
                    break;
                if (next == 0)
                    throw new Exception("Broken chain found!");
                current = next;
            }
            return chain;
        }

        // Allocate new chain of clusters
        public int AllocateChain(int count)
        {
            if (count <= 0)
                throw new Exception("Count must be positive!");
            List<int> freeClusters = new List<int>();
            for (int i = FSConstants.CONTENT_START_CLUSTER; i < FAT_ENTRIES; i++)
            {
                if (fat[i] == 0)
                {
                    freeClusters.Add(i);
                    if (freeClusters.Count == count)
                        break;
                }
            }
            if (freeClusters.Count < count)
                throw new Exception($"Not enough free clusters! Need {count}, found {freeClusters.Count}");
            for (int j = 0; j < freeClusters.Count - 1; j++)
            {
                fat[freeClusters[j]] = freeClusters[j + 1];
            }
            fat[freeClusters[freeClusters.Count - 1]] = -1;

            return freeClusters[0];
        }
        // Free a chain
        public void FreeChain(int startCluster)
        {
            if (startCluster < 0 || startCluster >= FAT_ENTRIES)
                throw new Exception("Invalid cluster index!");

            if (startCluster <= FSConstants.FAT_END_CLUSTER)
                throw new Exception("Can't free reserved cluster!");
            List<int> chain = FollowChain(startCluster);
            foreach (int c in chain)
            {
                if (c <= FSConstants.FAT_END_CLUSTER)
                    continue;
                fat[c] = 0;
            }
        }
        // Count free clusters
        public int GetFreeClusterCount()
        {
            int count = 0;
            for (int i = FSConstants.CONTENT_START_CLUSTER; i < FAT_ENTRIES; i++)
            {
                if (fat[i] == 0)
                    count++;
            }
            return count;
        }
    }
}