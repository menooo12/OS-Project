using System;
using System.Collections.Generic;

namespace OS_project
{
    public class DirectoryEntry
    {
        public string Name { get; set; }
        public byte Attribute { get; set; }
        public int FirstCluster { get; set; }
        public int FileSize { get; set; }

        public DirectoryEntry(string name, byte attr, int firstCluster, int size)
        {
            Name = name;
            Attribute = attr;
            FirstCluster = firstCluster;
            FileSize = size;
        }

        public DirectoryEntry()
        {
            Name = "";
            Attribute = 0;
            FirstCluster = 0;
            FileSize = 0;
        }

        public bool IsEmpty()
        {
            return string.IsNullOrEmpty(Name) || Name[0] == 0x00 || Name[0] == (char)0xE5;
        }
    }
}