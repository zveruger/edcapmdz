using System;
using System.IO;
using System.Linq;

namespace EncodeDecodeAPMDZ.Ancad
{
    internal abstract partial class BaseAndcadApmdzFormat
    {
        #region protected
        protected readonly long DataDiskNamePosition =
            //header length
                FileIdByteArray.Length
                + SeparatorByteArray.Length
                + CheckSumByteLength
                + SeparatorByteArray.Length
                + CounterByteLength
            //first part data length
                + FileCounterByteArray.Length
                + SeparatorByteArray.Length
                + DataLengthByteLength
                + DiskNameByteLength;
        //---------------------------------------------------------------------
        protected sealed class DecodeDataParameters
        {
            internal DecodeDataParameters()
            { }

            public bool FileDecodeCheckSum { get; internal set; }
            public bool UserFileDecodeCheckSum { get; internal set; }
        }
        //---------------------------------------------------------------------
        protected virtual bool DecodeDataHeader(FileStream sourceStream, out bool fileDecodeCheckSum)
        {
            var bufferFileId = new byte[FileIdByteArray.Length];
            var bufferSeparator = new byte[SeparatorByteArray.Length];
            var bufferCheckSum = new byte[CheckSumByteLength];
            var bufferFileCount = new byte[CounterByteLength];

            fileDecodeCheckSum = false;

            //Reading id file
            if (sourceStream.Read(bufferFileId, 0, bufferFileId.Length) != bufferFileId.Length || !bufferFileId.SequenceEqual(FileIdByteArray))
            { return false; }

            //Reading separator
            if (sourceStream.Read(bufferSeparator, 0, bufferSeparator.Length) != bufferSeparator.Length || !bufferSeparator.SequenceEqual(SeparatorByteArray))
            { return false; }

            //Reading checksum flag
            if (sourceStream.Read(bufferCheckSum, 0, bufferCheckSum.Length) == 0)
            { return false; }

            fileDecodeCheckSum = bufferCheckSum[0] != 0;

            //Reading separator
            if (sourceStream.Read(bufferSeparator, 0, bufferSeparator.Length) != bufferSeparator.Length || !bufferSeparator.SequenceEqual(SeparatorByteArray))
            { return false; }

            //Reading file count
            return sourceStream.Read(bufferFileCount, 0, bufferFileCount.Length) == bufferFileCount.Length;
        }
        protected virtual int DecodeData(FileStream sourceStream, StreamWriter targetStream, DecodeDataParameters decodeDataParameters)
        {
            var dataCount = 0;
            var bufferCounter = new byte[FileCounterByteArray.Length];
            var bufferSeparator = new byte[SeparatorByteArray.Length];
            var bufferDataLengt = new byte[DataLengthByteLength];
            var bufferCheckSum = new byte[32];
            var isFirstFileName = true;

            while (sourceStream.Position != sourceStream.Length)
            {
                //Reading file counter
                if (sourceStream.Read(bufferCounter, 0, bufferCounter.Length) != bufferCounter.Length || !bufferCounter.SequenceEqual(FileCounterByteArray))
                { break; }

                //Reading separator
                if (sourceStream.Read(bufferSeparator, 0, bufferSeparator.Length) != bufferSeparator.Length || !bufferSeparator.SequenceEqual(SeparatorByteArray))
                { break; }

                //Reading data length
                if (sourceStream.Read(bufferDataLengt, 0, bufferDataLengt.Length) != bufferDataLengt.Length)
                { break; }

                //Reading data
                var dataLength = DecodeByteArrayAsInt32(bufferDataLengt);
                var bufferDataLength = decodeDataParameters.FileDecodeCheckSum ? dataLength : dataLength - bufferCheckSum.Length;
                if (bufferDataLength <= 0)
                { break; }

                var bufferData = new byte[bufferDataLength];
                if (sourceStream.Read(bufferData, 0, bufferData.Length) != bufferData.Length)
                { break; }

                //Disk name
                char diskNameSymbol;
                if (!TryDiskName(bufferData[0], out diskNameSymbol))
                { break; }

                var fileNameLength = bufferData.Length - SeparatorByteArray.Length - DiskNameByteLength - (decodeDataParameters.FileDecodeCheckSum ? bufferCheckSum.Length : 0);
                if (fileNameLength <= 0)
                { break; }

                //For first file the not newline
                if (isFirstFileName)
                { isFirstFileName = false; }
                else
                { targetStream.WriteLine(); }

                //Path to file
                var bufferFileName = new byte[fileNameLength];
                Array.Copy(bufferData, 1, bufferFileName, 0, fileNameLength);

                //Full path to file
                var fullFilePath = diskNameSymbol.ToString() + new string(bufferFileName.Select(Convert.ToChar).ToArray());
                fullFilePath = fullFilePath.Replace("/", "\\");

                //Is Checksum
                if (decodeDataParameters.FileDecodeCheckSum && decodeDataParameters.UserFileDecodeCheckSum)
                {
                    //Reading file checksum
                    Array.Copy(bufferData, bufferData.Length - bufferCheckSum.Length, bufferCheckSum, 0, bufferCheckSum.Length);
                    var checkSum = new string(bufferCheckSum.Select(x =>
                    {
                        if (x == 0) { return '0'; }
                        return x < '!' ? '!' : Convert.ToChar(x);
                    }).ToArray());

                    targetStream.Write("{0} {1}", checkSum, fullFilePath);
                }
                else
                { targetStream.Write(fullFilePath); }

                ++dataCount;
            }

            return dataCount;
        }
        //---------------------------------------------------------------------
        protected override bool DecodeDataAvailable(FileStream sourceStream)
        {
            if (sourceStream.CanSeek)
            {
                var position = DataDiskNamePosition - DiskNameByteLength;
                if (sourceStream.Seek(position, SeekOrigin.Begin) == position)
                {
                    var buffer = new byte[DiskNameByteLength];
                    if (sourceStream.Read(buffer, 0, DiskNameByteLength) == DiskNameByteLength)
                    {
                        char diskNameSymbol;
                        return TryDiskName(buffer[0], out diskNameSymbol);
                    }
                }
            }
            return false;
        }
        protected override int DecodeData(FileStream sourceFileStream, string targetFileName, object userData)
        {
            var decodeDataParameters = userData as DecodeDataParameters ?? new DecodeDataParameters();

            using (var targetStream = File.CreateText(targetFileName))
            {
                bool fileDecodeCheckSum;
                if (DecodeDataHeader(sourceFileStream, out fileDecodeCheckSum))
                {
                    decodeDataParameters.FileDecodeCheckSum = fileDecodeCheckSum;

                    return DecodeData(sourceFileStream, targetStream, decodeDataParameters);
                }
            }

            return base.DecodeData(sourceFileStream, targetFileName, userData);
        }
        //---------------------------------------------------------------------
        protected static int DecodeByteArrayAsInt32(byte[] array)
        {
            if (array.Length > 4)
            { throw new ArgumentOutOfRangeException(nameof(array)); }

            var newArray = new byte[4];
            for (var i = 0; (i < array.Length && i < newArray.Length); i++)
            { newArray[i] = array[i]; }

            return BitConverter.ToInt32(newArray, 0);
        }
        #endregion
    }
}