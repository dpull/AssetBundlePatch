using System;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using UnityEngine;
using System.Net;

namespace dpull
{
    class UnzipProgress : Progress
    {
        Stream SourceArchiveFile;
        ZipInputStream ZipStream;
        string DestinationDirectoryName;
        byte[] Buffer = new byte[ZipFile.BufferSize];
        Stream WriteFileStream;

        System.Diagnostics.Stopwatch ReaderCost = new System.Diagnostics.Stopwatch();
        System.Diagnostics.Stopwatch WriterCost = new System.Diagnostics.Stopwatch();

        public UnzipProgress(string sourceArchiveFileName, string destinationDirectoryName)
        {
            SourceArchiveFile = FileEx.OpenRead(sourceArchiveFileName);
            ZipStream = new ZipInputStream(SourceArchiveFile);
            DestinationDirectoryName = destinationDirectoryName;
        }

        public override void Update()
        {
            for (int i = 0; i < 8; ++i)
            {
                if (Process())
                    break;
            }
        }

        public override string GetDebug()
        {
            return string.Format("Unzip:{0}, Write:{1}", ReaderCost.ElapsedMilliseconds, WriterCost.ElapsedMilliseconds);
        }

        bool Process()
        {
            try
            {
                if (WriteFileStream == null)
                {
                    ProcessNextEntry();
                    if (CurState != State.Uncompleted)
                        return true;
                }
                
                ProcessFile();
                return false;
            }
            catch(Exception e)
            {
                CurState = State.Failed;
                Debug.LogException(e);
                return true;
            }
        }

        void ProcessNextEntry()
        {
            while (true)
            {
                var theEntry = ZipStream.GetNextEntry();
                if (theEntry == null)
                {
                    CurState = State.Succeed;
                    return;
                }

                var fileName = Path.GetFileName(theEntry.Name);
                if (string.IsNullOrEmpty(fileName))
                    continue;

                string file = Path.Combine(DestinationDirectoryName, theEntry.Name);
                DirectoryEx.CreateDirectory(Directory.GetParent(file));

                WriteFileStream = File.OpenWrite(file);
                return;
            }
        }

        void ProcessFile()
        {
            ReaderCost.Start();
            var size = ZipStream.Read(Buffer, 0, Buffer.Length);
            ReaderCost.Stop();
            
            if (size <= 0) 
            {
                WriteFileStream.Close();
                WriteFileStream = null;
                return;
            }
            
            ProgressCurValue = SourceArchiveFile.Position;
            ProgressTotalValue = SourceArchiveFile.Length;

            WriterCost.Start();
            WriteFileStream.Write(Buffer, 0, size);
            WriterCost.Stop();
        }
    }
}