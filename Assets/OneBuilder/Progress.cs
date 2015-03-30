using System;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using UnityEngine;
using System.Net;

namespace dpull
{
    abstract class Progress
    {
        public enum State
        {
            Uncompleted,
            Succeed,
            Failed,
        }

        public State CurState = State.Uncompleted;
        public long ProgressCurValue = 0;
        public long ProgressTotalValue = 1;

        public virtual void Update()
        {
            throw new NotImplementedException();
        }

        public virtual string GetDebug()
        {
            return string.Empty;
        }
    }
}