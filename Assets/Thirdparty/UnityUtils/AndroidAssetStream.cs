using UnityEngine;
using System;

#if UNITY_ANDROID
namespace dpull
{
    public class AndroidAssetStream : System.IO.Stream
    {
        AndroidJavaObject AndroidInputStream;
        long AndroidInputStreamLength = long.MaxValue;
        long AndroidInputStreamPostion = 0;
                 
        public AndroidAssetStream(string fileName)
        {
            var noCompressExt = new string[]{
                ".jpg", ".jpeg", ".png", ".gif",
                ".wav", ".mp2", ".mp3", ".ogg", ".aac",
                ".mpg", ".mpeg", ".mid", ".midi", ".smf", ".jet",
                ".rtttl", ".imy", ".xmf", ".mp4", ".m4a",
                ".m4v", ".3gp", ".3gpp", ".3g2", ".3gpp2",
                ".amr", ".awb", ".wma", ".wmv"
            };

            var ext = System.IO.Path.GetExtension(fileName);

            using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                using (var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    using (var assetManager = activity.Call<AndroidJavaObject>("getAssets")) //android.content.res.AssetManager
                    {
                        if (Array.Exists<string>(noCompressExt, (obj)=>{ return obj == ext; }))
                        {
                            using (var assetFileDescriptor = assetManager.Call<AndroidJavaObject>("openFd", fileName)) //assets/ //android.content.res.AssetFileDescriptor
                            {
                                AndroidInputStreamLength = assetFileDescriptor.Call<long>("getLength");
                            }
                        }
                        AndroidInputStream = assetManager.Call<AndroidJavaObject>("open", fileName);
                    }
                }
            }

            if (AndroidInputStream == null)
                throw new System.IO.FileNotFoundException("getAssets failed", fileName);
        }
        
        public override void Flush ()
        {
            throw new NotImplementedException();
        }
        
        public override int Read(byte[] buffer, int offset, int count)
        {
            var ret = Read(AndroidInputStream, buffer, offset, count);
            if (ret > 0)
                AndroidInputStreamPostion += ret;
            return ret;
        }

        public override long Seek(long offset, System.IO.SeekOrigin origin)
        {
            if (offset - AndroidInputStreamPostion < 0)
                throw new NotImplementedException();
                
            long skip;
            switch (origin)
            {
            case System.IO.SeekOrigin.Begin:
                skip = AndroidInputStream.Call<long>("skip", offset - AndroidInputStreamPostion);
                AndroidInputStreamPostion += skip;
                break;

            case System.IO.SeekOrigin.Current:
                skip = AndroidInputStream.Call<long>("skip", offset);
                AndroidInputStreamPostion += skip;
                break;

            case System.IO.SeekOrigin.End:
                throw new NotImplementedException();
            }
            return AndroidInputStreamPostion;
        }
        
        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }
        
        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }
        
        public override bool CanRead 
        { 
            get 
            { 
                return AndroidInputStream != null;
            }
        }
        public override bool CanSeek 
        {
            get
            {
                return false;
            }
        }
        public override bool CanWrite 
        {
            get 
            { 
                return false;
            } 
        }
        public override long Length 
        { 
            get 
            { 
                if (AndroidInputStreamLength == long.MaxValue)
                    throw new NotImplementedException();
                return AndroidInputStreamLength;
            } 
        }
        
        public override long Position
        {
            get 
            {
                return AndroidInputStreamPostion;
            }
            set 
            {
                throw new NotImplementedException();
            }
        }   

        int Read(AndroidJavaObject javaObject, byte[] buffer, int offset, int count)
        {
            var args = new object[]{buffer, offset, count};
            IntPtr methodID = AndroidJNIHelper.GetMethodID<int>(javaObject.GetRawClass(), "read", args, false);
            jvalue[] array = AndroidJNIHelper.CreateJNIArgArray(args);
            try
            {
                var readLen = AndroidJNI.CallIntMethod(javaObject.GetRawObject(), methodID, array);
                if (readLen > 0)
                {
                    var temp = AndroidJNI.FromByteArray(array[0].l);
                    Array.Copy(temp, offset, buffer, offset, readLen);
                }
                return readLen;
            }
            finally
            {
                AndroidJNIHelper.DeleteJNIArgArray(args, array);
            }
        }
    }
}
#endif