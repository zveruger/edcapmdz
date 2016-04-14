using System;
using System.Linq;

namespace EncodeDecodeAPMDZ.Ancad
{
    internal abstract partial class BaseAndcadApmdzFormat : BaseApmdzFormat
    {
        #region private
        private const string FileId = "#FILE#EXP";
        private const string DiskNameSeparator = ":";
        #endregion
        #region protected
        protected static byte[] FileIdByteArray = FileId.Select(Convert.ToByte).ToArray();
        protected static byte[] SeparatorByteArray = { 0 };
        protected static byte[] FileCounterByteArray = { 1 };
        protected const int CounterByteLength = 4;
        protected const int CheckSumByteLength = 1;
        protected const int DataLengthByteLength = 2;
        protected const int DiskNameByteLength = 1;
        //---------------------------------------------------------------------
        protected abstract byte GetDefaultDiskNameByte();
        protected abstract byte GetDiskName(char diskNameSymbol);
        protected abstract bool TryDiskName(byte diskNameByte, out char diskNameSymbol);
        #endregion
        //---------------------------------------------------------------------
        public bool EncodeFile(string sourceFileName, string targetFileName, bool fictionalFileEncodeCheckSum, string defaultDiskName, out int dataCount)
        {
            var encodeDataParameters = new EncodeDataParameters
            {
                DefaultDiskName = defaultDiskName, 
                FictionalFileEncodeCheckSum = fictionalFileEncodeCheckSum
            };

            return EncodeFile(sourceFileName, targetFileName, encodeDataParameters, out dataCount);
        }
        public bool DecodeFile(string sourceFileName, string targetFileName, bool decodeCheckSum, out int dataCount)
        {
            var decodeDataParameters = new DecodeDataParameters
            {
                UserFileDecodeCheckSum = decodeCheckSum
            };

            return DecodeFile(sourceFileName, targetFileName, decodeDataParameters, out dataCount);
        }
        //---------------------------------------------------------------------
    }
}