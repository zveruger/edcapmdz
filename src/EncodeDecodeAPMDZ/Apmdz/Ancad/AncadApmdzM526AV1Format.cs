using System;

namespace EncodeDecodeAPMDZ.Ancad
{
    internal sealed class AncadApmdzM526AV1Format : BaseAndcadApmdzFormat
    {
        #region private
        private const char DiskNameC = 'C';
        private const char DiskNameD = 'D';
        private const char DiskNameZ = 'Z';
        //---------------------------------------------------------------------
        private const byte DiskNameCByte = 0;
        private const byte DiskNameDByte = (byte)DiskNameD;
        #endregion
        #region protected
        protected override byte GetDefaultDiskNameByte()
        {
            return DiskNameCByte;
        }
        protected override byte GetDiskName(char diskNameSymbol)
        {
            if (diskNameSymbol >= DiskNameDByte && diskNameSymbol <= DiskNameZ)
            { return Convert.ToByte((byte)diskNameSymbol - DiskNameDByte + 2); }

            return DiskNameCByte;
        }
        protected override bool TryDiskName(byte diskNameByte, out char diskNameSymbol)
        {
            if (diskNameByte == DiskNameCByte)
            {
                diskNameSymbol = DiskNameC;
                return true;
            }

            diskNameSymbol = Convert.ToChar(diskNameByte + DiskNameDByte - 2);
            if (diskNameSymbol > DiskNameC && diskNameSymbol <= DiskNameZ)
            { return true; }

            diskNameSymbol = DiskNameC;
            return false;
        }
        #endregion
    }
}