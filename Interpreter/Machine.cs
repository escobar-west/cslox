namespace Interpreter;

public class Machine{
    public void runFile(string path) {
        string text = System.IO.File.ReadAllText(path);
        run(text);
    }

    public void runPrompt() {
        while (true) {
            Console.Write("> ");
            string? line = Console.ReadLine();
            if (line is null) break;
            run(line);
        }
    }

    void run(string source) {
        Console.WriteLine(source);
    }
}
