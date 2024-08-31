using Interpreter;

class Program {
    static void Main(string[] args) {
        Machine shell = new Machine();
        if (args.Length > 1) {
            Console.WriteLine("Usage: jlox [script]");
            Environment.Exit(1);
        } else if (args.Length == 1) {
            shell.runFile(args[0]);
        } else {
            shell.runPrompt();
        }
    }
}