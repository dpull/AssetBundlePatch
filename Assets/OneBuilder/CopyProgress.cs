using System;
using System.IO;
using UnityEngine;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace dpull
{
    class CopyProgress : Progress
    {
        Stream Reader;
		Stream Writer;
		byte[] Buffer = new byte[1024 * 1024];
		public Stopwatch ReaderTime = new Stopwatch();
		public Stopwatch WriterTime = new Stopwatch();

		public CopyProgress(string srcFile, string dstFile)
        {
			Reader = FileEx.OpenRead(srcFile);
			Writer = File.OpenWrite(dstFile);

			ProgressTotalValue = Reader.Length;
			ProgressCurValue = 0;
        }

        public override void Update()
        {
			try
			{
				ReaderTime.Start();
				var count = Reader.Read(Buffer, 0, Buffer.Length);
				ReaderTime.Stop();
				
				if (count <= 0)
				{
					CurState = State.Succeed;
					return;
				}

				WriterTime.Start();
				Writer.Write(Buffer, 0, count);
				WriterTime.Stop();
				ProgressCurValue += count;
			}
			catch(Exception e)
			{
				CurState = State.Failed;
				Debug.LogException(e);
			}
        }      
    }
}