using System;
using System.Text;

namespace OS_project
{
    internal class Program
    {
        static void Main(string[] args)
        {
            VirtualDisk disk = new VirtualDisk();
            string path = "minifat.bin";

            try
            {
                // ================================
                //   TASK 1: Virtual Disk
                // ================================
                disk.Initialize(path, true);
                Console.WriteLine("Virtual Disk created and opened successfully.");

                // ================================
                //   TASK 2: Superblock
                // ================================
                SuperblockManager superblock = new SuperblockManager(disk);

                Console.WriteLine("\n--- Writing Superblock ---");
                byte[] sbMessage = Encoding.ASCII.GetBytes("MiniFAT Superblock OK");
                byte[] sbData = new byte[FSConstants.CLUSTER_SIZE];
                Array.Copy(sbMessage, sbData, sbMessage.Length);

                superblock.WriteSuperblock(sbData);
                Console.WriteLine("Superblock written.");

                Console.WriteLine("--- Reading Superblock ---");
                byte[] sbRead = superblock.ReadSuperblock();
                string sbText = Encoding.ASCII.GetString(sbRead, 0, sbMessage.Length);
                Console.WriteLine("Superblock content: " + sbText);

                // ================================
                //   TASK 3: FAT TABLE
                // ================================
                FatTableManager fat = new FatTableManager(disk);

                Console.WriteLine("\n--- Loading FAT ---");
                fat.LoadFatFromDisk();
                Console.WriteLine("FAT loaded into memory.");
                int freeBefore = fat.GetFreeClusterCount();
                Console.WriteLine("Free clusters before allocation: " + freeBefore);
                Console.WriteLine("\n--- Allocating 3 clusters ---");
                int startCluster = fat.AllocateChain(3);
                Console.WriteLine("Allocated chain starting at cluster: " + startCluster);
                Console.WriteLine("\n--- Following cluster chain ---");
                var chain = fat.FollowChain(startCluster);
                Console.WriteLine("Chain: " + string.Join(" -> ", chain));
                Console.WriteLine("\n--- Freeing chain ---");
                fat.FreeChain(startCluster);
                Console.WriteLine("Chain freed.");
                int freeAfter = fat.GetFreeClusterCount();
                Console.WriteLine("Free clusters after freeing: " + freeAfter);
                Console.WriteLine("\n--- Saving FAT to disk ---");
                fat.FlushFatToDisk();
                Console.WriteLine("FAT saved successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("\nERROR: " + ex.Message);
            }
            finally
            {
                disk.CloseDisk();
            }
            Console.WriteLine("\nDone. Press any key to exit.");
            Console.ReadKey();
        }
    }
}
