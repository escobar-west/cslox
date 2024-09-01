namespace Interpreter;

public class Machine {
    static bool _hadError = false;

    public static void runFile(string path) {
        string text = System.IO.File.ReadAllText(path);
        run(text);
        if (Machine._hadError) {
            throw new InvalidOperationException("Runtime encountered an invalid operation");
        }
    }

    public static void runPrompt() {
        while (true) {
            Console.Write("> ");
            string? line = Console.ReadLine();
            if (line is null) break;
            run(line);
            Machine._hadError = false;
        }
    }

    static void run(string source) {
        var scanner = new Scanner(source);
        List<Token> tokens = scanner.scanTokens();
        foreach (Token token in tokens) {
            //Console.WriteLine(token.toString());
        }
        var parser = new Parser(tokens);
        Expr? expression = parser.parse();
        if (Machine._hadError) return;
        Console.WriteLine(new AstPrinter().print(expression!)!);
    }

    public static void error(int line, string message) {
        report(line, null, message);
    }

    public static void error(Token token, String message) {
        if (token._type == TokenType.EOF) {
            report(token._line, "at end", message);
        } else {
            report(token._line, $"at '{token._lexeme}'", message);
        }
    }

    static void report(int line, string? where, string message) {
        string fmted_where;
        if (where != null) {
            fmted_where = $"_{where}";
        } else {
            fmted_where = "";
        }
        Console.Error.Write($"[line {line}] Error{fmted_where}: {message}\n");
        Machine._hadError = true;
    }
}
