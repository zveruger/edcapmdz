using System;
using System.IO;

namespace EncodeDecodeAPMDZ
{
    internal abstract class BaseApmdzFormat
    {
        #region protected
        protected abstract bool DecodeDataAvailable(FileStream sourceStream);
        //---------------------------------------------------------------------
        protected virtual int EncodeData(string sourceFileName, string targetFileName, object userData)
        {
            return -1;
        }
        protected virtual int DecodeData(FileStream sourceFileStream, string targetFileName, object userData)
        {
            return -1;
        }
        //---------------------------------------------------------------------
        protected static int GetEncodeDataCount(StreamReader sourceStream)
        {
            if(sourceStream == null)
            { throw new ArgumentNullException(nameof(sourceStream)); }

            var dataCount = 0;

            try
            {
                if (sourceStream.BaseStream.CanSeek)
                { sourceStream.BaseStream.Seek(0, SeekOrigin.Begin); }

                while (!sourceStream.EndOfStream)
                {
                    if (!string.IsNullOrWhiteSpace(sourceStream.ReadLine()))
                    { ++dataCount; }
                }
            }
            finally
            {
                if (sourceStream.BaseStream.CanSeek)
                { sourceStream.BaseStream.Seek(0, SeekOrigin.Begin); }
            }

            return dataCount;
        }
        #endregion
        //---------------------------------------------------------------------
        public bool EncodeFile(string sourceFileName, string targetFileName, object userData, out int dataCount)
        {
            if (string.IsNullOrWhiteSpace(sourceFileName))
            { throw new ArgumentNullException(nameof(sourceFileName)); }
            if (!File.Exists(sourceFileName))
            { throw new FileNotFoundException(string.Empty, nameof(sourceFileName)); }
            if (string.IsNullOrWhiteSpace(targetFileName))
            { throw new ArgumentNullException(nameof(targetFileName)); }

            dataCount = EncodeData(sourceFileName, targetFileName, userData);
            return dataCount != -1;
        }
        //---------------------------------------------------------------------
        public bool DecodeFileAvailable(string sourceFileName)
        {
            if (string.IsNullOrWhiteSpace(sourceFileName))
            { throw new ArgumentNullException(nameof(sourceFileName)); }
            if (!File.Exists(sourceFileName))
            { throw new FileNotFoundException(string.Empty, nameof(sourceFileName)); }

            bool result;
            using (var sourceStream = File.Open(sourceFileName, FileMode.Open))
            {
                result = DecodeDataAvailable(sourceStream);
            }
            return result;
        }
        public bool DecodeFile(string sourceFileName, string targetFileName, object userData, out int dataCount)
        {
            if (string.IsNullOrWhiteSpace(sourceFileName))
            { throw new ArgumentNullException(nameof(sourceFileName)); }
            if (!File.Exists(sourceFileName))
            { throw new FileNotFoundException(string.Empty, nameof(sourceFileName)); }
            if (string.IsNullOrWhiteSpace(targetFileName))
            { throw new ArgumentNullException(nameof(targetFileName)); }

            if (!File.Exists(targetFileName))
            { File.Delete(targetFileName); }

            using (var sourceStream = File.Open(sourceFileName, FileMode.Open))
            {
                dataCount = DecodeData(sourceStream, targetFileName, userData);
            }

            return dataCount != -1;
        }
        //---------------------------------------------------------------------
    }
}