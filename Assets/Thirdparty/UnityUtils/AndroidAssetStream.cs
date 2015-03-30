using UnityEngine;
using System;

#if UNITY_ANDROID
namespace dpull
{
    public class AndroidAssetStream : System.IO.Stream
    {
        AndroidJavaObject AndroidInputStream;
        long AndroidInputStreamLength;
        long AndroidInputStreamPostion;
         
        public AndroidAssetStream(string fileName)
        {
            using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                using (var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    using (var assetManager = activity.Call<AndroidJavaObject>("getAssets")) //android.content.res.AssetManager
                    {
                        using (var assetFileDescriptor = assetManager.Call<AndroidJavaObject>("openFd", fileName)) //assets/ //android.content.res.AssetFileDescriptor
                        {
                            AndroidInputStreamLength = assetFileDescriptor.Call<long>("getLength");
                        }
                        AndroidInputStream = assetManager.Call<AndroidJavaObject>("open", fileName);
                    }
                }
            }
            
            if (AndroidInputStream == null)
                throw new System.IO.FileNotFoundException("getAssets failed", fileName);

            AndroidInputStreamPostion = 0;
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
			long skip;
			switch (origin)
			{
			case System.IO.SeekOrigin.Begin:
				skip = AndroidInputStreamLength = AndroidInputStream.Call<long>("skip", offset - AndroidInputStreamPostion);
				AndroidInputStreamPostion += skip;
				break;

			case System.IO.SeekOrigin.Current:
				skip = AndroidInputStreamLength = AndroidInputStream.Call<long>("skip", offset);
				AndroidInputStreamPostion += skip;
				break;

			case System.IO.SeekOrigin.End:
				skip = AndroidInputStreamLength = AndroidInputStream.Call<long>("skip", AndroidInputStreamLength - offset - AndroidInputStreamPostion);
				AndroidInputStreamPostion += skip;
				break;
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
        
        public override bool CanRead { get { return AndroidInputStream != null; } }
        public override bool CanSeek { get { return false;} }
        public override bool CanWrite { get { return false;} }
        public override long Length  { get { return AndroidInputStreamLength; } }
        
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

				/*
				for (var i = offset; i < readLen; ++i)
    			{
    				buffer[i] = AndroidJNI.GetByteArrayElement(array[0].l, i);
    			}
    			*/
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