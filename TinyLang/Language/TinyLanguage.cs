namespace TinyLang.Language;

public sealed record ExecutionResult(IReadOnlyDictionary<string, double> Variables, string Output);

public static class TinyLanguage
{
    public static ExecutionResult Execute(string source) => new Parser(source).Run();

    private enum Kind { Number, Identifier, Let, Print, Plus, Minus, Star, Slash, Equal, Left, Right, Semicolon, End }
    private readonly record struct Token(Kind Kind, string Text, double Number = 0);

    private sealed class Lexer(string source)
    {
        private int _position;
        public List<Token> Scan()
        {
            var result = new List<Token>();
            while (_position < source.Length)
            {
                var c = source[_position];
                if (char.IsWhiteSpace(c)) { _position++; continue; }
                if (char.IsDigit(c))
                {
                    var start = _position++;
                    while (_position < source.Length && (char.IsDigit(source[_position]) || source[_position] == '.')) _position++;
                    var text = source[start.._position];
                    if (!double.TryParse(text, System.Globalization.CultureInfo.InvariantCulture, out var number)) throw new FormatException($"Invalid number: {text}");
                    result.Add(new(Kind.Number, text, number));
                    continue;
                }
                if (char.IsLetter(c) || c == '_')
                {
                    var start = _position++;
                    while (_position < source.Length && (char.IsLetterOrDigit(source[_position]) || source[_position] == '_')) _position++;
                    var text = source[start.._position];
                    result.Add(new(text switch { "let" => Kind.Let, "print" => Kind.Print, _ => Kind.Identifier }, text));
                    continue;
                }
                result.Add(c switch
                {
                    '+' => new(Kind.Plus, "+"), '-' => new(Kind.Minus, "-"), '*' => new(Kind.Star, "*"), '/' => new(Kind.Slash, "/"),
                    '=' => new(Kind.Equal, "="), '(' => new(Kind.Left, "("), ')' => new(Kind.Right, ")"), ';' => new(Kind.Semicolon, ";"),
                    _ => throw new FormatException($"Unexpected character: {c}")
                });
                _position++;
            }
            result.Add(new(Kind.End, ""));
            return result;
        }
    }

    private sealed class Parser(string source)
    {
        private readonly List<Token> _tokens = new Lexer(source).Scan();
        private readonly Dictionary<string, double> _variables = new(StringComparer.Ordinal);
        private readonly List<string> _output = [];
        private int _current;

        public ExecutionResult Run()
        {
            while (!Check(Kind.End))
            {
                if (Match(Kind.Semicolon)) continue;
                if (Match(Kind.Let))
                {
                    var name = Consume(Kind.Identifier, "Expected a variable name.").Text;
                    Consume(Kind.Equal, "Expected '=' after the variable name.");
                    _variables[name] = Expression();
                }
                else if (Match(Kind.Print)) _output.Add(Format(Expression()));
                else throw Error("Expected 'let' or 'print'.");
                Match(Kind.Semicolon);
            }
            return new(new Dictionary<string, double>(_variables), string.Join(Environment.NewLine, _output));
        }

        private double Expression()
        {
            var value = Term();
            while (Match(Kind.Plus, Kind.Minus)) { var op = Previous().Kind; var rhs = Term(); value = op == Kind.Plus ? value + rhs : value - rhs; }
            return value;
        }

        private double Term()
        {
            var value = Primary();
            while (Match(Kind.Star, Kind.Slash))
            {
                var op = Previous().Kind; var rhs = Primary();
                if (op == Kind.Slash && rhs == 0) throw Error("Division by zero.");
                value = op == Kind.Star ? value * rhs : value / rhs;
            }
            return value;
        }

        private double Primary()
        {
            if (Match(Kind.Number)) return Previous().Number;
            if (Match(Kind.Identifier)) return _variables.TryGetValue(Previous().Text, out var value) ? value : throw Error($"Unknown variable: {Previous().Text}");
            if (Match(Kind.Minus)) return -Primary();
            if (Match(Kind.Left)) { var value = Expression(); Consume(Kind.Right, "Expected ')'."); return value; }
            throw Error("Expected a number, variable, or parenthesized expression.");
        }

        private static string Format(double value) => value.ToString("0.########", System.Globalization.CultureInfo.InvariantCulture);
        private bool Match(params Kind[] kinds) { foreach (var kind in kinds) if (Check(kind)) { _current++; return true; } return false; }
        private bool Check(Kind kind) => Peek().Kind == kind;
        private Token Peek() => _tokens[_current];
        private Token Previous() => _tokens[_current - 1];
        private Token Consume(Kind kind, string message) => Check(kind) ? _tokens[_current++] : throw Error(message);
        private FormatException Error(string message) => new($"{message} (token '{Peek().Text}')");
    }
}
