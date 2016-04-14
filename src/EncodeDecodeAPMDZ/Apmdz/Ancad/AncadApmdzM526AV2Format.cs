namespace EncodeDecodeAPMDZ.Ancad
{
    internal class AncadApmdzM526AV2Format : BaseAndcadApmdzFormat
    {
        #region private
        private const char DiskNameC = 'C';
        private const char DiskNameZ = 'Z';
        //---------------------------------------------------------------------
        private const byte DiskNameCByte = (byte)DiskNameC;
        #endregion
        #region protected
        protected override byte GetDefaultDiskNameByte()
        {
            return DiskNameCByte;
        }
        protected override byte GetDiskName(char diskNameSymbol)
        {
            if (diskNameSymbol >= DiskNameC && diskNameSymbol <= DiskNameZ)
            { return (byte)diskNameSymbol; }

            return DiskNameCByte;
        }
        protected override bool TryDiskName(byte diskNameByte, out char diskNameSymbol)
        {
            diskNameSymbol = (char)diskNameByte;
            if (diskNameSymbol >= DiskNameC && diskNameSymbol <= DiskNameZ)
            { return true; }

            diskNameSymbol = DiskNameC;
            return false;
        }
        #endregion
    }
}
