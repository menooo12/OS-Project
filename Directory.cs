using System;
using System.Collections.Generic;

namespace OS_project
{
    public class Directory
    {
        private const int ENTRY_SIZE = 32;
        private int CLUSTER_SIZE;
        private int ENTRIES_PER_CLUSTER;

        private VirtualDisk disk;
        private FatTableManager fatManager;

        public Directory(VirtualDisk d, FatTableManager fat)
        {
            disk = d;
            fatManager = fat;
            CLUSTER_SIZE = FSConstants.CLUSTER_SIZE;
            ENTRIES_PER_CLUSTER = CLUSTER_SIZE / ENTRY_SIZE;
        }

        // read directory entries from cluster chain
        public List<DirectoryEntry> ReadDirectory(int startCluster)
        {
            List<DirectoryEntry> entries = new List<DirectoryEntry>();
            List<int> chain = fatManager.FollowChain(startCluster);

            foreach (int cluster in chain)
            {
                byte[] data = disk.ReadCluster(cluster);

                for (int i = 0; i < ENTRIES_PER_CLUSTER; i++)
                {
                    int offset = i * ENTRY_SIZE;
                    DirectoryEntry entry = ParseEntry(data, offset);

                    if (!entry.IsEmpty())
                        entries.Add(entry);
                }
            }
            return entries;
        }

        // convert raw 32 bytes into directory entry
        private DirectoryEntry ParseEntry(byte[] data, int offset)
        {
            DirectoryEntry entry = new DirectoryEntry();

            byte[] nameBytes = new byte[11];
            Array.Copy(data, offset, nameBytes, 0, 11);
            entry.Name = Parse8Dot3Name(nameBytes);

            entry.Attribute = data[offset + 11];
            entry.FirstCluster = BitConverter.ToInt32(data, offset + 12);
            entry.FileSize = BitConverter.ToInt32(data, offset + 16);

            return entry;
        }

        // search for entry by name (case insensitive)
        public DirectoryEntry FindDirectoryEntry(int startCluster, string name)
        {
            if (string.IsNullOrEmpty(name)) return null;

            string search = name.ToUpper();
            List<DirectoryEntry> entries = ReadDirectory(startCluster);

            foreach (var e in entries)
                if (e.Name.ToUpper() == search)
                    return e;

            return null;
        }

        // add entry to first empty slot (allocate new cluster if needed)
        public bool AddDirectoryEntry(int startCluster, DirectoryEntry newEntry)
        {
            List<int> chain = fatManager.FollowChain(startCluster);

            foreach (int cluster in chain)
            {
                byte[] data = disk.ReadCluster(cluster);

                for (int i = 0; i < ENTRIES_PER_CLUSTER; i++)
                {
                    int offset = i * ENTRY_SIZE;

                    if (data[offset] == 0x00 || data[offset] == 0xE5)
                    {
                        WriteEntry(data, offset, newEntry);
                        disk.WriteCluster(cluster, data);
                        return true;
                    }
                }
            }

            // allocate new cluster
            int last = chain[chain.Count - 1];
            int newCluster = fatManager.AllocateChain(1);

            fatManager.SetFatEntry(last, newCluster);

            byte[] newData = new byte[CLUSTER_SIZE];
            WriteEntry(newData, 0, newEntry);
            disk.WriteCluster(newCluster, newData);

            fatManager.FlushFatToDisk();
            return true;
        }

        // write entry fields into cluster bytes
        private void WriteEntry(byte[] data, int offset, DirectoryEntry entry)
        {
            byte[] name = FormatNameTo8Dot3(entry.Name);
            Array.Copy(name, 0, data, offset, 11);

            data[offset + 11] = entry.Attribute;

            byte[] cl = BitConverter.GetBytes(entry.FirstCluster);
            Array.Copy(cl, 0, data, offset + 12, 4);

            byte[] size = BitConverter.GetBytes(entry.FileSize);
            Array.Copy(size, 0, data, offset + 16, 4);

            for (int i = 20; i < ENTRY_SIZE; i++)
                data[offset + i] = 0x00;
        }

        // remove entry and free its FAT chain
        public bool RemoveDirectoryEntry(int startCluster, string name)
        {
            List<int> chain = fatManager.FollowChain(startCluster);

            foreach (int cluster in chain)
            {
                byte[] data = disk.ReadCluster(cluster);

                for (int i = 0; i < ENTRIES_PER_CLUSTER; i++)
                {
                    int offset = i * ENTRY_SIZE;
                    DirectoryEntry entry = ParseEntry(data, offset);

                    if (!entry.IsEmpty() && entry.Name.ToUpper() == name.ToUpper())
                    {
                        if (entry.FirstCluster > 0)
                            fatManager.FreeChain(entry.FirstCluster);

                        data[offset] = 0x00;
                        disk.WriteCluster(cluster, data);
                        fatManager.FlushFatToDisk();

                        return true;
                    }
                }
            }
            return false;
        }

        // format string name into 8.3 bytes
        public static byte[] FormatNameTo8Dot3(string name)
        {
            byte[] result = new byte[11];

            for (int i = 0; i < 11; i++)
                result[i] = 0x20;

            if (string.IsNullOrEmpty(name)) return result;

            name = name.ToUpper();
            string[] parts = name.Split('.');

            string baseName = parts[0];
            string ext = parts.Length > 1 ? parts[1] : "";

            for (int i = 0; i < Math.Min(8, baseName.Length); i++)
                result[i] = (byte)baseName[i];

            for (int i = 0; i < Math.Min(3, ext.Length); i++)
                result[8 + i] = (byte)ext[i];

            return result;
        }

        // convert raw 8.3 bytes to normal filename string
        public static string Parse8Dot3Name(byte[] raw)
        {
            if (raw == null || raw.Length < 11) return "";
            if (raw[0] == 0x00 || raw[0] == 0xE5) return "";

            byte[] baseName = new byte[8];
            Array.Copy(raw, 0, baseName, 0, 8);

            byte[] ext = new byte[3];
            Array.Copy(raw, 8, ext, 0, 3);

            string name = Converter.BytesToString(baseName).TrimEnd();
            string extension = Converter.BytesToString(ext).TrimEnd();

            return extension.Length > 0 ? name + "." + extension : name;
        }

        // list entries in root directory
        public void ListRootDirectory()
        {
            int root = FSConstants.ROOT_DIR_FIRST_CLUSTER;
            List<DirectoryEntry> entries = ReadDirectory(root);

            Console.WriteLine("Root Directory:");
            Console.WriteLine("=========================");

            if (entries.Count == 0)
            {
                Console.WriteLine("(empty)");
            }
            else
            {
                foreach (var e in entries)
                {
                    string t = (e.Attribute & 0x10) != 0 ? "DIR" : "FILE";
                    Console.WriteLine($"{e.Name,-12} {t,-5} Cl: {e.FirstCluster,-3} Size: {e.FileSize}");
                }
            }
            Console.WriteLine("=========================");
        }
    }
}
