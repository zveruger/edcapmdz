using System;
using System.IO;
using System.Linq;

namespace EncodeDecodeAPMDZ.Ancad
{
    internal abstract partial class BaseAndcadApmdzFormat
    {
        #region protected
        protected static byte[] FictionalFileCheckSum = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        //---------------------------------------------------------------------
        protected sealed class EncodeDataParameters
        {
            #region private
            private bool _fictionalFileEncodeCheckSum;
            #endregion
            internal EncodeDataParameters()
            { }

            public string DefaultDiskName { get; internal set; }
            public bool FileEncodeCheckSum { get; internal set; }
            public bool FictionalFileEncodeCheckSum
            {
                get { return _fictionalFileEncodeCheckSum; }
                set
                {
                    _fictionalFileEncodeCheckSum = value;
                    if (_fictionalFileEncodeCheckSum)
                    { FileEncodeCheckSum = true; }
                }
            }
        }
        //---------------------------------------------------------------------
        protected virtual void EncodeHeader(FileStream targetStream, int dataCount, bool fileEncodeCheckSum)
        {
            //Writing id file
            targetStream.Write(FileIdByteArray, 0, FileIdByteArray.Length);

            //Writing separator
            targetStream.Write(SeparatorByteArray, 0, SeparatorByteArray.Length);

            //Writing checksum flag
            targetStream.WriteByte((byte)(fileEncodeCheckSum ? 1 : 0));

            //Writing separator
            targetStream.Write(SeparatorByteArray, 0, SeparatorByteArray.Length);

            //Writing file count
            EncodeValueAsByteArray(targetStream, dataCount, CounterByteLength);
        }
        protected virtual void EncodeData(StreamReader sourceStream, FileStream targetStream, EncodeDataParameters encodeDataParameters)
        {
            while (!sourceStream.EndOfStream)
            {
                var line = sourceStream.ReadLine();
                if (!string.IsNullOrWhiteSpace(line))
                {
                    //---------------------------------------------------------
                    //Path - [disk name:][path to file]
                    var diskNameSeparatorIndex = line.IndexOf(DiskNameSeparator, 0, 2, StringComparison.OrdinalIgnoreCase);
                    //---------------------------------------------------------
                    var diskName = diskNameSeparatorIndex == -1 ? encodeDataParameters.DefaultDiskName : line.Substring(0, diskNameSeparatorIndex).Trim();
                    var diskNameByte = string.IsNullOrEmpty(diskName) ? GetDefaultDiskNameByte() : GetDiskName(char.ToUpper(diskName[0]));
                    //---------------------------------------------------------
                    var filePathNoDiskName = diskNameSeparatorIndex == -1 ? $"{DiskNameSeparator}{line}" : line.Substring(diskNameSeparatorIndex);
                    filePathNoDiskName = filePathNoDiskName.Replace("\\", "/");
                    //---------------------------------------------------------
                    //Creating file checksum (GOST P 34.11-94 c TestParamSet).
                    var fileCheckSum = CreateFileCheckSum(line, encodeDataParameters.FictionalFileEncodeCheckSum);
                    //---------------------------------------------------------
             
                    //Writing file counter
                    targetStream.Write(FileCounterByteArray, 0, FileCounterByteArray.Length);

                    //Writing separator
                    targetStream.Write(SeparatorByteArray, 0, SeparatorByteArray.Length);

                    var dataLength = filePathNoDiskName.Length //path to file length, no disk name
                                     + fileCheckSum.Length //checksum length
                                     + DiskNameByteLength  //disk name length
                                     + SeparatorByteArray.Length; //separator lenght

                    //Writing data length
                    EncodeValueAsByteArray(targetStream, dataLength, DataLengthByteLength);

                    //Writing disk name
                    targetStream.WriteByte(diskNameByte);

                    //Writing path to file, no disk name
                    var dataArray = filePathNoDiskName.Select(Convert.ToByte).ToArray();
                    targetStream.Write(dataArray, 0, dataArray.Length);

                    //Writing separator
                    targetStream.Write(SeparatorByteArray, 0, SeparatorByteArray.Length);

                    //Writing file checksum
                    if (encodeDataParameters.FileEncodeCheckSum)
                    { targetStream.Write(fileCheckSum, 0, fileCheckSum.Length); }
                }
            }
        }
        //---------------------------------------------------------------------
        protected virtual byte[] CreateFileCheckSum(string fileName, bool fictionalFileCheckSum)
        {
            return FictionalFileCheckSum;
        }
        //---------------------------------------------------------------------
        protected override int EncodeData(string sourceFileName, string targetFileName, object userData)
        {
            var encodeDataParameters = (userData as EncodeDataParameters) ?? new EncodeDataParameters();

            using (var sourceStream = File.OpenText(sourceFileName))
            {
                var dataCount = GetEncodeDataCount(sourceStream);
                using (var targetStream = File.Create(targetFileName))
                {
                    EncodeHeader(targetStream, dataCount, encodeDataParameters.FileEncodeCheckSum);
                    EncodeData(sourceStream, targetStream, encodeDataParameters);
                }
                return dataCount;
            }
        }
        //---------------------------------------------------------------------
        protected static void EncodeValueAsByteArray(FileStream stream, int value, int maxByteCount)
        {
            if (maxByteCount < 0 || maxByteCount > 4)
            { throw new ArgumentOutOfRangeException(nameof(maxByteCount)); }

            var encodeValueAsByteArray = BitConverter.GetBytes(value);
            for (var i = 0; (i < encodeValueAsByteArray.Length) && (i < maxByteCount); i++)
            {
                var data = encodeValueAsByteArray[i];
                stream.WriteByte(data);
            }
        }
        #endregion
    }
}