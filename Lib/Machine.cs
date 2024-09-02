namespace Lib;

public class Machine {
    static bool _hadError = false;

    public static void RunFile(string path) {
        string text = System.IO.File.ReadAllText(path);
        Run(text);
        if (_hadError) {
            throw new InvalidOperationException("Runtime encountered an invalid operation");
        }
    }

    public static void RunPrompt() {
        while (true) {
            Console.Write("> ");
            string? line = Console.ReadLine();
            if (line == null) break;
            Run(line);
            _hadError = false;
        }
    }

    static void Run(string source) {
        var scanner = new Scanner(source);
        List<Token> tokens = scanner.ScanTokens();
        var parser = new Parser(tokens);
        Expr expression = parser.Parse();
        if (_hadError) return;
        Console.WriteLine(new AstPrinter().Print(expression));
    }

    public static void Error(int line, string message) {
        Report(line, null, message);
    }

    public static void Error(Token token, string message) {
        if (token._type == TokenType.EOF) {
            Report(token._line, "at end", message);
        } else {
            Report(token._line, $"at '{token._lexeme}'", message);
        }
    }

    static void Report(int line, string? where, string message) {
        if (where != null) where = $" {where}";
        Console.Error.WriteLine($"[line {line}] Error{where}: {message}\n");
        _hadError = true;
    }
}
