using System.IO;
using System.IO.Compression;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using ICSharpCode.SharpZipLib.Zip;

namespace PsypherLibrary.SupportLibrary.Utils.FileManager
{
    public static class FileCompressionUtilities
    {
        public static class GZip
        {
            public static byte[] Decompress(byte[] gzip)
            {
                // Create a GZIP stream with decompression mode.
                // ... Then create a buffer and write into while reading from the GZIP stream.
                using (GZipStream stream = new GZipStream(new MemoryStream(gzip), CompressionMode.Decompress))
                {
                    const int size = 4096;
                    byte[] buffer = new byte[size];
                    using (MemoryStream memory = new MemoryStream())
                    {
                        int count = 0;
                        do
                        {
                            count = stream.Read(buffer, 0, size);
                            if (count > 0)
                            {
                                memory.Write(buffer, 0, count);
                            }
                        } while (count > 0);

                        return memory.ToArray();
                    }
                }
            }

            public static byte[] Compress(byte[] raw)
            {
                using (MemoryStream memory = new MemoryStream())
                {
                    using (GZipStream gzip = new GZipStream(memory,
                        CompressionMode.Compress, true))
                    {
                        gzip.Write(raw, 0, raw.Length);
                    }

                    return memory.ToArray();
                }
            }
        }

        public static class SharpZip
        {
            public static void AddDirectoryFilesToTar(TarArchive tarArchive, string sourceDirectory, bool recurse)
            {
                // Optionally, write an entry for the directory itself.
                // Specify false for recursion here if we will add the directory's files individually.
                //
                TarEntry tarEntry = TarEntry.CreateEntryFromFile(sourceDirectory);
                tarArchive.WriteEntry(tarEntry, false);

                // Write each file to the tar.
                //
                string[] filenames = Directory.GetFiles(sourceDirectory);
                foreach (string filename in filenames)
                {
                    tarEntry = TarEntry.CreateEntryFromFile(filename);
                    tarArchive.WriteEntry(tarEntry, true);
                }

                if (recurse)
                {
                    string[] directories = Directory.GetDirectories(sourceDirectory);
                    foreach (string directory in directories)
                        AddDirectoryFilesToTar(tarArchive, directory, recurse);
                }
            }

            public static void CreateTarGZ_FromDirectory(string tgzFilename, string sourceDirectory)
            {
                Stream outStream = File.Create(tgzFilename);
                Stream gzoStream = new GZipOutputStream(outStream);
                TarArchive tarArchive = TarArchive.CreateOutputTarArchive(gzoStream);

                // Note that the RootPath is currently case sensitive and must be forward slashes e.g. "c:/temp"
                // and must not end with a slash, otherwise cuts off first char of filename
                // This is scheduled for fix in next release
                tarArchive.RootPath = sourceDirectory.Replace('\\', '/');
                if (tarArchive.RootPath.EndsWith("/"))
                    tarArchive.RootPath = tarArchive.RootPath.Remove(tarArchive.RootPath.Length - 1);

                AddDirectoryFilesToTar(tarArchive, sourceDirectory, true);

                tarArchive.Close();
            }

            public static void ExtractTGZ(string gzArchiveName, string destFolder)
            {
                Stream inStream = File.OpenRead(gzArchiveName);
                Stream gzipStream = new GZipInputStream(inStream);

                TarArchive tarArchive = TarArchive.CreateInputTarArchive(gzipStream);
                tarArchive.ExtractContents(destFolder);
                tarArchive.Close();

                gzipStream.Close();
                inStream.Close();
            }

            public static void ExtractZip(string zipArchiveName, string destPath)
            {
                /*Stream inStream = File.OpenRead(zipArchiveName);
            ZipFile zipFile = new ZipFile(inStream);

            if (!Directory.Exists(destPath))
            {
                Directory.CreateDirectory(destPath);
            }

            if (!zipFile.TestArchive(true))
            {
                Debug.Log("Zip file failed integrity check!");
                zipFile.IsStreamOwner = false;
            }
            else
            {
                foreach (ZipEntry zipEntry in zipFile)
                {
                    var isDirectory = zipEntry.IsDirectory;

                    string entryName = zipEntry.Name;
                    Debug.Log("Unpacking zip file entry: " + entryName);

                    if (isDirectory)
                    {
                      
                    }

                    byte[] buffer = new byte[4096]; // 4K is optimum
                    Stream stream = zipFile.GetInputStream(zipEntry);

                    // Manipulate the output filename here as desired.
                    string fullZipToPath = destPath + Path.GetFileName(entryName);


                    // Unzip file in buffered chunks. This is just as fast as unpacking to a buffer the full size
                    // of the file, but does not waste memory.
                    // The "using" will close the stream even if an exception occurs.
                    using (var streamWriter = File.Create(fullZipToPath))
                    {
                        StreamUtils.Copy(stream, streamWriter, buffer);
                    }
                }
            }

            zipFile.IsStreamOwner = false;
            zipFile.Close();
            inStream.Close();*/

                FastZip fastZip = new FastZip();

                fastZip.ExtractZip(zipArchiveName, destPath, null);
            }
        }
    }
}