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
            Console.WriteLine(token.toString());
        }
    }

    public static void error(int line, string message) {
        report(line, "", message);
    }

    static void report(int line, string where, string message) {
        Console.Error.Write($"[line {line}] Error{where}: {message}");
        Machine._hadError = true;
    }
}
