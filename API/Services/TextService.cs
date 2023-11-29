using System.Net;
using System.Text.RegularExpressions;
using Microsoft.SemanticKernel.Text;

namespace API.Services;

public interface ITextService
{
    int GetLinesCount(string content);
    string[] GetLines(string content);
}

public class TextService : ITextService
{
    public int GetLinesCount(string content)
    {
        return GetLines(content)?.Length ?? 0;
    }

    public string[] GetLines(string content)
    {
        return 
        TextChunker.SplitPlainTextParagraphs(
            TextChunker.SplitPlainTextLines(
                WebUtility.HtmlDecode(Regex.Replace(content, @"<[^>]+>|&nbsp;", "")),
                128),
            1024).ToArray();
    }
}