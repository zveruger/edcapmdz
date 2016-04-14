using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using EncodeDecodeAPMDZ.Utils.Extensions;
using EncodeDecodeAPMDZ.Utils.ProductInformations;

namespace EncodeDecodeAPMDZ
{
    internal sealed class Application
    {
        #region private
        private readonly Worker _worker;
        private readonly ProductVersionInformation _productVersionInformation;
        private readonly List<string> _fileNames = new List<string>();
        private readonly StringBuilder _err = new StringBuilder();
        //---------------------------------------------------------------------
        private static ICollection<string> _GetArgvOptions(string[] argv)
        {
            var argsOptions = new List<string>(argv.Length);
            
            argsOptions.AddRange(argv);

            return argsOptions;
        }
        private void _processOptions(ICollection<string> argvOptions)
        {
            var opts = new List<string>();
            foreach (var arg in argvOptions.Where(arg => !string.IsNullOrEmpty(arg)))
            {
                if (CommandLineOptions.IsOption(arg, "/h", "/?"))
                {
                    _printHelp();
                    _Exit();
                }
                else if (CommandLineOptions.IsOption(arg, "/v"))
                {
                    Console.WriteLine("{0} [Version {1}]\n", _productVersionInformation.Name, _productVersionInformation.Version);
                    _Exit();
                }
                else if (arg[0] == '/')
                { opts.Add(arg); }
                else
                { _fileNames.Add(arg); }
            }

            var options = new CommandLineOptions(_worker);
            var parseOptionsResult = options.ParseOptions(opts.ToArray(), "Invalid command line options:");
            if (!(parseOptionsResult && _worker.IsActionAvailable))
            {
                var errorInfo = options.GetOptionErrors();
                if(!string.IsNullOrWhiteSpace(errorInfo))
                { _err.AppendLine(options.GetOptionErrors()); }

                _err.AppendLine($"For help on options type {_productVersionInformation.Name} /h");
                _error();
            }
        }
        private void _processFiles()
        {
            if (_fileNames.Count < 2)
            {
                _err.AppendLine(_fileNames.Count == 0 ? "Source file not set" : "Target file not set");
                _error();
            }
            else
            {
                var sourceFileName = _fileNames[_fileNames.Count - 2];
                var targetFileName = _fileNames[_fileNames.Count - 1];

                var errorFileNameCount = _fileNames.Count - 2;
                if (errorFileNameCount > 0)
                {
                    _err.AppendLine("Invalid command line options:");
                     for (var i = 0; i < errorFileNameCount; i++)
                     { _err.AppendLine(_fileNames[i]); }
                }

                _worker.Action(sourceFileName, targetFileName);
                Console.Write(_worker.GetInfo());
                _Exit();
            }
        }
        //---------------------------------------------------------------------
        private void _printHelp()
        {
            var help = new StringBuilder();
            //-----------------------------------------------------------------
            help.AppendLine();
          help.AppendFormat("{0} [Version {1}]", _productVersionInformation.Name, _productVersionInformation.Version);
            help.AppendLine();
            help.AppendLine("");
            help.AppendLine("Usage:");
            help.AppendLine("------");
          help.AppendFormat("    {0} [OPIONS] [file source] [file target]", _productVersionInformation.Name);
          help.AppendLine();
            help.AppendLine();
            help.AppendLine("OPTIONS:");
            help.AppendLine("--------");
            help.AppendLine();
            help.AppendLine("Encoding:");
            help.AppendLine("---------");
            help.AppendLine("    /emav1");
            help.AppendLine("    Get the list APMDZ Ancad M526-A with the version of the microcontroller to 2011 year.");
            help.AppendLine();
            help.AppendLine("    /emav2");
            help.AppendLine("    Get the list APMDZ Ancad M526-A with the new version of the microcontroller.");
            help.AppendLine();
            help.AppendLine("    /ntfcks");
            help.AppendLine("    Do not use a fictional checksum. (optional)");
            help.AppendLine();
            help.AppendLine("    /dfdsk:<diskName>");
            help.AppendLine("    Set default disk name. (Default value: D) (optional)");
            help.AppendLine();
            help.AppendLine("    Nodes:");
            help.AppendLine("    ------");
            help.AppendLine("    *) Valid formats paths in the file source:");
            help.AppendLine("           <disk>:\\<directory>\\ or <disk>:/<directory>/");
            help.AppendLine("                 :\\<directory>\\ or       :/<directory>/");
            help.AppendLine("                  \\<directory>\\ or        /<directory>/");
            help.AppendLine();
            help.AppendLine("    Examples:");
            help.AppendLine("    ---------");
          help.AppendFormat("    1) {0} /emav1 FileSource.txt Chklist.lst", _productVersionInformation.Name);
          help.AppendLine();
          help.AppendFormat("    2) {0} /emav2 /dfdsk:C FileSource.txt Chklist.lst", _productVersionInformation.Name);
          help.AppendLine();
            help.AppendLine();
            help.AppendLine("Decoding:");
            help.AppendLine("---------");
            help.AppendLine("    /d");
            help.AppendLine("    Decode the list APMDZ.");
            help.AppendLine();
            help.AppendLine("    /dcks");
            help.AppendLine("    Decode checksum in the list APMDZ. (optional)");
            help.AppendLine();
            help.AppendLine("    Nodes:");
            help.AppendLine("    ------");
            help.AppendLine("    *) Format paths in the file target:");
            help.AppendLine("           <disk>:\\<directory>\\");
            help.AppendLine("                    or");
            help.AppendLine("           <checksum file> <disk>:\\<directory>\\ (set flag /d and /dcks)");
            help.AppendLine();
            help.AppendLine("    Examples:");
            help.AppendLine("    ---------");
          help.AppendFormat("    1) {0} /d Chklist.lst FileTarget.txt", _productVersionInformation.Name);
          help.AppendLine();
          help.AppendFormat("    2) {0} /d /dcks Chklist.lst FileTarget.txt", _productVersionInformation.Name);
          help.AppendLine();
            help.AppendLine();
            help.AppendLine("Other:");
            help.AppendLine("------");
            help.AppendLine("    /v");
            help.AppendLine("    Print version number.");
            help.AppendLine();
            help.AppendLine("    /h or /?");
            help.Append("    Print this help message.");
            //-----------------------------------------------------------------
            Console.WriteLine(help.ToString());
        }
        //---------------------------------------------------------------------
        private void _error()
        {
            _err.AppendLine($"{_productVersionInformation.Name} has terminated");
            _Exit(true);
        }
        //---------------------------------------------------------------------
        private void _Exit(bool failed = false)
        {
            if(failed && _err.Length != 0)
            { Console.Write(_err); }

            Environment.Exit(failed ? 1 : 0);
        }
        #endregion
        internal Application(Worker worker)
        {
            if(worker == null)
            { throw new ArgumentNullException(nameof(worker)); }

            _worker = worker;

            var currentAssembly = Assembly.GetExecutingAssembly();
            _productVersionInformation = new ProductVersionInformation(currentAssembly.AssemblyProduct(), null, currentAssembly.AssemblyVersion());
        }

        //---------------------------------------------------------------------
        static void Main(string[] argv)
        {
            var encoder = new Worker();
            var application = new Application(encoder);

            var argvOptions = _GetArgvOptions(argv);
            application._processOptions(argvOptions);

            application._processFiles();
        }
        //---------------------------------------------------------------------
    }
}


