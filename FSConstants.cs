using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OS_project
{
    internal class FSConstants
    {
        // cluster settings
        public const int CLUSTER_SIZE = 1024;
        public const int CLUSTER_COUNT = 1024;

        // superblock location
        public const int SUPERBLOCK_CLUSTER = 0;

        // FAT table location
        public const int FAT_START_CLUSTER = 1;
        public const int FAT_CLUSTER_COUNT = (CLUSTER_COUNT * sizeof(int)) / CLUSTER_SIZE;
        public const int FAT_END_CLUSTER = FAT_START_CLUSTER + FAT_CLUSTER_COUNT - 1;

        // content area
        public const int CONTENT_START_CLUSTER = FAT_END_CLUSTER + 1;
        public const int ROOT_DIR_FIRST_CLUSTER = CONTENT_START_CLUSTER;
    }
}