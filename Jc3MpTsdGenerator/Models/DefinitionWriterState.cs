using System;
using System.IO;

namespace Jc3MpTsdGenerator.Models
{
    public class DefinitionWriterState : IDisposable
    {
        private const string IndentationString = "    ";

        private readonly TextWriter _writer;

        private int _indentationLevel;

        public DefinitionWriterState(TextWriter writer)
        {
            _writer = writer;
        }

        public void WriteLine(string line) => _writer.WriteLine($"{GetIndentationString()}{line}");

        public void WriteBlankLine() => WriteLine(string.Empty);

        public void IncreaseIndentation(int amount = 1) => _indentationLevel += amount;

        public void DecreaseIndentation() => _indentationLevel--;

        private string GetIndentationString()
        {
            var indentationString = string.Empty;
            for (var i = 0; i < _indentationLevel; i++)
                indentationString += IndentationString;

            return indentationString;
        }

        public void Dispose()
        {
            _writer?.Dispose();
        }
    }
}