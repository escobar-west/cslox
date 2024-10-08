namespace Lib;

public interface ILoxCallable {
    public int Arity();
    public object? Call(Interpreter interpreter, List<object?> args);
}


public abstract class BaseLoxFunc : ILoxCallable {
    public string _name;
    protected BaseLoxFunc(string name) {
        _name = name;
    }
    public abstract int Arity();
    public abstract object? Call(Interpreter interpreter, List<object?> args);
    public override string? ToString() {
        return $"<fn {_name}>";
    }
}


public class LoxFunc : BaseLoxFunc {
    readonly Stmt.Function _declaration;
    readonly Env _closure;
    readonly bool _isInitializer;

    public LoxFunc(Stmt.Function declaration, Env closure, bool isInitializer)
        : base(declaration._name._lexeme) {
        _declaration = declaration;
        _closure = closure;
        _isInitializer = isInitializer;
    }

    public override int Arity() {
        return _declaration._parameters.Count;
    }

    public override object? Call(Interpreter interpreter, List<object?> args) {
        Env env = new(_closure);
        for (int i = 0; i < _declaration._parameters.Count; i++)
            env.Define(_declaration._parameters[i]._lexeme, args[i]);
        try {
            interpreter.ExecuteBlock(_declaration._body, env);
        } catch (Return return_val) {
            if (_isInitializer)
                return _closure.GetAt("this", 0);
            return return_val._value;
        }
        if (_isInitializer)
            return _closure.GetAt("this", 0);
        return null;
    }

    public LoxFunc Bind(LoxInstance instance) {
        Env environment = new(_closure);
        environment.Define("this", instance);
        return new LoxFunc(_declaration, environment, _isInitializer);
    }
}


public class ClockFunc : BaseLoxFunc {
    static readonly DateTime _start_date = new(2024, 8, 22, 0, 0, 0);
    static readonly DateTimeOffset _offset = new(_start_date);
    public ClockFunc() : base("clock") { }

    public override int Arity() {
        return 0;
    }

    public override object? Call(Interpreter interpreter, List<object?> args) {
        return (double)_offset.ToUnixTimeSeconds();
    }
}


public class PrintFunc : BaseLoxFunc {
    public PrintFunc() : base("print") { }

    public override int Arity() {
        return 1;
    }

    public override object? Call(Interpreter interpreter, List<object?> args) {
        Console.WriteLine(args[0]?.ToString() ?? "nil");
        return null;
    }
}


public class ToStringFunc : BaseLoxFunc {
    public ToStringFunc() : base("str") { }

    public override int Arity() {
        return 1;
    }

    public override object? Call(Interpreter interpreter, List<object?> args) {
        return args[0]?.ToString() ?? "nil";
    }
}