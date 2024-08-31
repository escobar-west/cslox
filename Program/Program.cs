using Interpreter;

class Program {
    static void Main(string[] args) {
        if (args.Length > 1) {
            Console.WriteLine("Usage: jlox [script]");
            Environment.Exit(1);
        } else if (args.Length == 1) {
            Machine.runFile(args[0]);
        } else {
            Machine.runPrompt();
        }
    }
}