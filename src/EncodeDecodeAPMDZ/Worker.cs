using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EncodeDecodeAPMDZ.Ancad;

namespace EncodeDecodeAPMDZ
{
    //-------------------------------------------------------------------------
    internal sealed class Worker
    {
        #region private
        private WorkerActionType _actionType = WorkerActionType.None;
        private readonly StringBuilder _info = new StringBuilder();
        //---------------------------------------------------------------------
        private static readonly Dictionary<WorkerActionType, Lazy<BaseApmdzFormat>> _ApmdzFormats = new Dictionary<WorkerActionType, Lazy<BaseApmdzFormat>>
        {
            { WorkerActionType.EncodeInAncadApmdzM526AV1Format, new Lazy<BaseApmdzFormat>(() => new AncadApmdzM526AV1Format()) },
            { WorkerActionType.EncodeInAncadApmdzM526AV2Format, new Lazy<BaseApmdzFormat>(() => new AncadApmdzM526AV2Format()) }
        }; 
        //---------------------------------------------------------------------
        private bool _decodeCheckSum;
        private bool _fictionalFileEncodeCheckSum = true;
        private string _defaultDiskName = "D";
        //---------------------------------------------------------------------
        private void _encodeAncadApmdzAction(BaseApmdzFormat apmdzFormat, string sourceFileName, string targetFileName)
        {
            var ancadApmdzFormat = (BaseAndcadApmdzFormat)apmdzFormat;

            int dataCount;
            if (ancadApmdzFormat.EncodeFile(sourceFileName, targetFileName, _fictionalFileEncodeCheckSum, _defaultDiskName, out dataCount))
            { _info.AppendLine($"Encoded data count: {dataCount}."); }
            else
            { _encodeError(sourceFileName); }
        }
        private void _encodeError(string fileName)
        {
            _info.AppendLine($"Encode error file '{fileName}'.");
        }
        //---------------------------------------------------------------------
        private void _decodeAction(string sourceFileName, string targetFileName)
        {
            var isNotFormat = true;
            foreach (var apmdzFormat in _ApmdzFormats.Select(keyValuePair => keyValuePair.Value.Value).Where(apmdzFormat => apmdzFormat.DecodeFileAvailable(sourceFileName)))
            {
                isNotFormat = false;
               
                var ancadApmdzFileFormat = apmdzFormat as BaseAndcadApmdzFormat;
                int dataCount = 0;

                var decodeFileResult = ancadApmdzFileFormat?.DecodeFile(sourceFileName, targetFileName, _decodeCheckSum, out dataCount) 
                    ?? apmdzFormat.DecodeFile(sourceFileName, targetFileName, _decodeCheckSum, out dataCount);

                if (decodeFileResult)
                { _info.AppendLine($"Decoded data count: {dataCount.ToString()}."); }
                else
                { _decodeError(sourceFileName, string.Empty); }

                break;
            }

            if (isNotFormat)
            { _decodeError(sourceFileName, "File format is not defined."); }
        }
        private void _decodeError(string fileName, string errorInfo)
        {
            { _info.AppendLine($"Decode error file '{fileName}'."); }

            if(!string.IsNullOrWhiteSpace(errorInfo))
            { _info.AppendLine(errorInfo); }
        }
        #endregion
        //---------------------------------------------------------------------
        public bool IsActionAvailable
        {
            get { return _actionType != WorkerActionType.None; }
        }
        //---------------------------------------------------------------------
        public void Action(string sourceFileName, string targetFileName)
        {
            switch (_actionType)
            {
                case WorkerActionType.EncodeInAncadApmdzM526AV1Format:
                case WorkerActionType.EncodeInAncadApmdzM526AV2Format:
                    { _encodeAncadApmdzAction(_ApmdzFormats[_actionType].Value, sourceFileName, targetFileName); }
                    break;
                case WorkerActionType.Decode:
                    { _decodeAction(sourceFileName, targetFileName); }
                    break;
                default:
                    { _encodeAncadApmdzAction(_ApmdzFormats[WorkerActionType.EncodeInAncadApmdzM526AV1Format].Value, sourceFileName, targetFileName); }
                    break;
            }
        }
        //---------------------------------------------------------------------
        public WorkerActionType ActionType
        {
            get { return _actionType; }
            set { _actionType = value; }
        }
        public bool DecodeCheckSum
        {
            get { return _decodeCheckSum; }
            set { _decodeCheckSum = value; }
        }
        public bool FictionalFileEncodeCheckSum
        {
            get { return _fictionalFileEncodeCheckSum; }
            set { _fictionalFileEncodeCheckSum = value; }
        }
        public string DefaultDiskName
        {
            get { return _defaultDiskName; }
            set
            {
                if(!string.IsNullOrWhiteSpace(value))
                { _defaultDiskName = value; }
            }
        }
        //---------------------------------------------------------------------
        public string GetInfo()
        {
            return _info.ToString();
        }
        //---------------------------------------------------------------------
    }
    //-------------------------------------------------------------------------
    internal enum WorkerActionType
    {
        None,
        //---------------------------------------------------------------------
        EncodeInAncadApmdzM526AV1Format,
        EncodeInAncadApmdzM526AV2Format,
        //---------------------------------------------------------------------
        Decode
        //---------------------------------------------------------------------
    }
    //-------------------------------------------------------------------------
}