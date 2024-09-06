namespace Lib;

public class Machine {
    static readonly Interpreter _interpreter = new();
    static bool _hadError = false;

    public static void RunFile(string path) {
        string text = File.ReadAllText(path);
        Run(text);
        if (_hadError) {
            throw new InvalidOperationException("Runtime encountered an invalid operation");
        }
    }

    public static void RunPrompt() {
        while (true) {
            Console.Write("> ");
            string? line = Console.ReadLine();
            if (line == null)
                break;
            Run(line);
            _hadError = false;
        }
    }

    static void Run(string source) {
        var scanner = new Scanner(source);
        List<Token> tokens = scanner.ScanTokens();
        var parser = new Parser(tokens);
        List<Stmt> statements = parser.Parse();
        if (_hadError)
            return;
        Resolver resolver = new(_interpreter);
        resolver.Resolve(statements);
        if (_hadError)
            return;
        _interpreter.Interpret(statements);
    }

    public static void Error(int line, string message) {
        Report(line, null, message);
    }

    public static void RuntimeError(Exception e) {
        Report(null, null, e.Message);
    }

    public static void Error(Token token, string message) {
        if (token._type == TokenType.EOF) {
            Report(token._line, "at end", message);
        } else {
            Report(token._line, $"at '{token._lexeme}'", message);
        }
    }

    static void Report(int? line, string? where, string message) {
        var line_str = (line != null) ? $"[line {line}] " : "";
        if (where != null)
            where = $" {where}";
        Console.Error.WriteLine($"{line_str}Error{where}: {message}\n");
        _hadError = true;
    }
}
