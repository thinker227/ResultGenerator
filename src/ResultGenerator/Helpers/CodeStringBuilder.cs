using System.Text;

namespace ResultGenerator.Helpers;

internal sealed class CodeStringBuilder
{
    private readonly StringBuilder builder;
    private readonly string indentation;
    private int indentationLevel;
    private bool endsWithNewline;

    public string AsString => ToString();



    public CodeStringBuilder(string indentation = "    ")
    {
        builder = new();
        this.indentation = indentation;
        indentationLevel = 0;
        endsWithNewline = true;
    }



    private void AppendIndentation()
    {
        for (var i = 0; i < indentationLevel; i++)
        {
            builder.Append(indentation);
        }
    }

    public CodeStringBuilder Append(string? text, bool appendNewline = false)
    {
        if (text is not null)
        {
            var lines = text.Split('\n');

            for (var i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                if (endsWithNewline || i != 0)
                    AppendIndentation();
                builder.Append(line);
                if (i != lines.Length - 1)
                    builder.AppendLine();
            }
        }

        if (appendNewline)
            builder.AppendLine();
        endsWithNewline = appendNewline;

        return this;
    }

    public CodeStringBuilder AppendLine(string? text) =>
        Append(text, true);

    public CodeStringBuilder AppendLine() =>
        AppendLine(null);

    public CodeStringBuilder Indent(int amount = 1)
    {
        indentationLevel += amount;
        return this;
    }

    public CodeStringBuilder Unindent(int amount = 1)
    {
        indentationLevel -= amount;
        return this;
    }

    public CodeStringBuilder Sections<T>(string? separator, IEnumerable<T> sections, Action<T> buildAction)
    {
        var first = true;

        foreach (var section in sections)
        {
            if (!first)
                Append(separator);
            first = false;

            buildAction(section);
        }

        return this;
    }

    public IDisposable IndentedBlock(string? start = null, string? end = null, bool appendNewline = false)
    {
        Append(start, appendNewline).Indent();
        return new Block(this, end, appendNewline);
    }

    public override string ToString() =>
        builder.ToString();



    private sealed class Block : IDisposable
    {
        private readonly CodeStringBuilder builder;
        private readonly string? end;
        private readonly bool appendNewline;

        public Block(CodeStringBuilder builder, string? end, bool appendNewline)
        {
            this.builder = builder;
            this.end = end;
            this.appendNewline = appendNewline;
        }

        public void Dispose() =>
            builder.Unindent().Append(end, appendNewline);
    }
}
