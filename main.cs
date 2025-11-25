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
                // Task 1: Virtual Disk
                disk.Initialize(path, true);
                Console.WriteLine("Disk created and opened.");

                // write test data
                byte[] message = Encoding.ASCII.GetBytes("Hello Virtual Disk!");
                byte[] data = new byte[disk.CLUSTER_SIZE];
                Array.Copy(message, data, message.Length);

                disk.WriteCluster(0, data);
                Console.WriteLine("Data written to cluster 0.");

                // read it back
                byte[] readBack = disk.ReadCluster(0);
                string text = Encoding.ASCII.GetString(readBack, 0, message.Length);
                Console.WriteLine("Data read from cluster 0: " + text);

                // Task 2: Superblock Manager
                SuperblockManager superblock = new SuperblockManager(disk);
                Console.WriteLine("\nSuperblock initialized with zeros.");

                byte[] sbMessage = Encoding.ASCII.GetBytes("MiniFAT Superblock");
                byte[] sbData = new byte[FSConstants.CLUSTER_SIZE];
                Array.Copy(sbMessage, sbData, sbMessage.Length);

                superblock.WriteSuperblock(sbData);
                Console.WriteLine("Superblock updated.");
                byte[] sbRead = superblock.ReadSuperblock();
                string sbText = Encoding.ASCII.GetString(sbRead, 0, sbMessage.Length);
                Console.WriteLine("Superblock content: " + sbText);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
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