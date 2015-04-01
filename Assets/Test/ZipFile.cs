using System;
using System.IO;
using ICSharpCode.SharpZipLib.Zip; // https://github.com/icsharpcode/SharpZipLib
using System.Collections;
using UnityEngine;

namespace dpull
{
    public class ZipFile
    {
        public static int BufferSize = 1024 * 1024; 

        public static void CreateFromDirectory(string sourceDirectoryName, string destinationArchiveFileName)
        {
            string[] filenames = Directory.GetFiles(sourceDirectoryName);
            var inDirStrLen = sourceDirectoryName.Length;

            if (!PathEx.EndWithDirectorySeparatorChar(sourceDirectoryName))
                inDirStrLen++;
            
            using (ZipOutputStream zipStream = new ZipOutputStream(File.Create(destinationArchiveFileName))) 
            {                
                byte[] buffer = new byte[BufferSize];
                
                zipStream.SetLevel(9); 
                
                foreach (string file in filenames)
                {
                    var entryName = file.Substring(inDirStrLen);
                    var fileInfo = new FileInfo(file);
                    
                    ZipEntry entry = new ZipEntry(entryName);
                    entry.DateTime = fileInfo.LastWriteTime;
                    zipStream.PutNextEntry(entry);
                    
                    using (FileStream fileStream = File.OpenRead(file)) 
                    {
                        while (true)
                        {
                            int size = fileStream.Read(buffer, 0, buffer.Length);
                            if (size <= 0) 
                                break;

                            zipStream.Write(buffer, 0, size);
                        }
                    }
                }
                
                zipStream.Finish();
                zipStream.Close();
            }
        }

        public static void CreateFromDirectory(string[] sourceFileNames, string destinationArchiveFileName)
        {
            using (ZipOutputStream zipStream = new ZipOutputStream(File.Create(destinationArchiveFileName))) 
            {
                byte[] buffer = new byte[BufferSize];
                
                zipStream.SetLevel(9); 
             
                foreach (string file in sourceFileNames)
                {
                    var entryName = Path.GetFileName(file);
                    var fileInfo = new FileInfo(file);

                    ZipEntry entry = new ZipEntry(entryName);
                    entry.DateTime = fileInfo.LastWriteTime;
                    zipStream.PutNextEntry(entry);
                    
                    using (FileStream fileStream = File.OpenRead(file)) 
                    {
                        while (true)
                        {
                            int size = fileStream.Read(buffer, 0, buffer.Length);
                            if (size <= 0) 
                                break;
                            
                            zipStream.Write(buffer, 0, size);
                        }
                    }
                }
                
                zipStream.Finish();
                zipStream.Close();
            }
        }

        public static void ExtractToDirectory(string sourceArchiveFileName, string destinationDirectoryName)
        {
            using (var zipStream = new ZipInputStream(FileEx.OpenRead(sourceArchiveFileName))) 
            {
                ZipEntry theEntry;
                byte[] data = new byte[BufferSize];
                
                while ((theEntry = zipStream.GetNextEntry()) != null) 
                {
                    var fileName = Path.GetFileName(theEntry.Name);
                    if (string.IsNullOrEmpty(fileName))
                        continue;
                    
                    string file = Path.Combine(destinationDirectoryName, theEntry.Name);
                    DirectoryEx.CreateDirectory(Directory.GetParent(file));
                    
                    using (FileStream fileStream = File.Create(file)) 
                    {
                        while (true)
                        {
                            var size = zipStream.Read(data, 0, data.Length);
                            if (size <= 0) 
                                break;
                            
                            fileStream.Write(data, 0, size);
                        }
                    }
                }
            }
        }

    } 
}
