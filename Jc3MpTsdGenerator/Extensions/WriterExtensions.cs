using System;
using System.Text.RegularExpressions;
using Jc3MpTsdGenerator.Models;

namespace Jc3MpTsdGenerator.Extensions
{
    public static class WriterExtensions
    {
        private static string ProcessCodeComment(string comment)
        {
            var linkedTypeRegexReplace = new Regex(@"\{\{linked_type '(?<type>[^']+)'\}\}");
            comment = linkedTypeRegexReplace.Replace(comment, "{$1}");

            return comment;
        }

        public static void WriteCommentBlockContent(this DefinitionWriterState writer, string comment)
        {
            if (string.IsNullOrWhiteSpace(comment))
                return;

            comment = comment.Replace("\r\n", "\n").Replace("\r", "\n");
            comment = ProcessCodeComment(comment);

            foreach (var line in comment.Split(new[] { "\n" }, StringSplitOptions.None))
                writer.WriteLine($" * {line}");
        }
    }
}