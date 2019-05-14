using System;
using System.Runtime.InteropServices;
using java.io;
using File = System.IO.File;

namespace NStagger.ModelMapper
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            ObjectInputStream modelReader = new ObjectInputStream(new FileInputStream(@"C:\Users\Rojan\Desktop\swedish.bin\swedish.bin"));

            se.su.ling.stagger.SUCTagger stagger = (se.su.ling.stagger.SUCTagger)modelReader.readObject();

            SUCTagger sucTagger = Mapper.Map<SUCTagger>(stagger);

            var size = Marshal.SizeOf(sucTagger);
            // Both managed and unmanaged buffers required.
            byte[] bytes = new byte[size];
            IntPtr ptr = Marshal.AllocHGlobal(size);
            // Copy object byte-to-byte to unmanaged memory.
            Marshal.StructureToPtr(sucTagger, ptr, false);
            // Copy data from unmanaged memory to managed buffer.
            Marshal.Copy(ptr, bytes, 0, size);
            // Release unmanaged memory.
            Marshal.FreeHGlobal(ptr);

            File.WriteAllBytes(@"C:\Users\Rojan\Desktop\swedish.bin\swedish.nbin", bytes);

            //BinaryFormatter formatter = new BinaryFormatter();

            //formatter.Serialize(new FileStream(@"C:\Users\Rojan\Desktop\swedish.bin\swedish.nbin", FileMode.Create), sucTagger);
        }
    }
}
