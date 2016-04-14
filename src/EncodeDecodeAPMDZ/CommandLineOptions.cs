using System;
using System.Linq;
using System.Text;

namespace EncodeDecodeAPMDZ
{
    internal sealed class CommandLineOptions
    {
        #region private
        private readonly Worker _worker;
        private readonly StringBuilder _errors = new StringBuilder();
        //---------------------------------------------------------------------
        private void _parseOption(string arg, string errorInfo)
        {
            if (_IsParamOption(arg, "ema"))
            {
                var fileEncodeTypeParam = _GetParam(arg, "ema");
                
                if(_IsOption(fileEncodeTypeParam, "v1"))
                { _worker.ActionType = WorkerActionType.EncodeInAncadApmdzM526AV1Format; }
                else if(_IsOption(fileEncodeTypeParam, "v2"))
                { _worker.ActionType = WorkerActionType.EncodeInAncadApmdzM526AV2Format; }
                else
                { _isOptionError(arg, errorInfo); }
            }
            else if (_IsOption(arg, "ntfcks"))
            { _worker.FictionalFileEncodeCheckSum = false; }
            else if (_IsOption(arg, "d"))
            { _worker.ActionType = WorkerActionType.Decode; }
            else if (_IsOption(arg, "dcks"))
            { _worker.DecodeCheckSum = true; }
            else if (_IsParamOption(arg, "dfdsk:"))
            {
                var diskName = _GetParam(arg, "dfdsk:");
                _worker.DefaultDiskName = diskName;
            }
            else
            { _isOptionError(arg, errorInfo); }
        }
        //---------------------------------------------------------------------
        private void _isOptionError(string arg, string errorInfo)
        {
            if (_errors.Length == 0)
            { _errors.AppendLine(errorInfo); }

            _errors.AppendLine(arg);
        }
        //---------------------------------------------------------------------
        private static string _GetParam(string arg, string optionName)
        {
            return arg.Substring(optionName.Length);
        }
        //---------------------------------------------------------------------
        private static bool _IsOption(string arg, string optionName)
        {
            return string.Compare(arg, optionName, StringComparison.Ordinal) == 0;
        }
        private static bool _IsOption(string arg, string optionName1, string optionName2)
        {
            return _IsOption(arg, optionName1) || _IsOption(arg, optionName2);
        }
        private static bool _IsOption(string arg, params string[] optionNames)
        {
            return optionNames.Any(optionName => _IsOption(arg, optionName));
        }
        //---------------------------------------------------------------------
        private static bool _IsParamOption(string arg, string optionName)
        {
            var result = string.Compare(arg, 0, optionName, 0, optionName.Length) == 0;
            return result;
        }
        #endregion
        internal CommandLineOptions(Worker worker)
        {
            if(worker == null)
            { throw new ArgumentNullException(nameof(worker)); }

            _worker = worker;
        }

        //---------------------------------------------------------------------
        public static bool IsOption(string arg, string optionName)
        {
            return _IsOption(arg, optionName);
        }
        public static bool IsOption(string arg, string optionName1, string optionName2)
        {
            return _IsOption(arg, optionName1, optionName2);
        }
        public static bool IsOption(string arg, params string[] optionNames)
        {
            return _IsOption(arg, optionNames);
        }
        //---------------------------------------------------------------------
        public string GetOptionErrors()
        {
            return _errors.ToString();
        }
        //---------------------------------------------------------------------
        public bool ParseOptions(string[] options, string errorInfo)
        {
            _errors.Clear();

            foreach (var option in options)
            {
                var arg = string.Compare(option, 0, "/", 0, 1) == 0 ? option.Substring(1) : option;
                _parseOption(arg, errorInfo);
            }

            return _errors.Length == 0;
        }
        //---------------------------------------------------------------------
    }
}