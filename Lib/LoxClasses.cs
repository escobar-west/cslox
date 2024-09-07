using System.Reflection;

namespace Lib;

public class LoxClass : ILoxCallable {
    public readonly string _name;
    readonly Dictionary<string, LoxFunc> _methods;

    public object? Call(Interpreter interpreter, List<object?> args) {
        LoxInstance instance = new(this);
        FindMethod("init")?
            .Bind(instance)?
            .Call(interpreter, args);
        return instance;
    }

    public int Arity() {
        return FindMethod("init")?.Arity() ?? 0;
    }

    public LoxClass(string name, Dictionary<string, LoxFunc> methods) {
        _name = name;
        _methods = methods;
    }

    public LoxFunc? FindMethod(string name) {
        if (_methods.TryGetValue(name, out LoxFunc? value)) {
            return value;
        }
        return null;
    }

    public override string ToString() {
        return _name;
    }
}

public class LoxInstance {
    readonly LoxClass _klass;
    readonly Dictionary<string, object?> _fields = [];

    public LoxInstance(LoxClass klass) {
        _klass = klass;
    }

    public object? Get(string name) {
        if (_fields.TryGetValue(name, out object? value))
            return value;
        LoxFunc method = _klass.FindMethod(name) ??
            throw new InvalidOperationException($"Undefined property {name}.");
        return method.Bind(this);
    }

    public void Set(string name, object? value) {
        _fields[name] = value;
    }

    public override string ToString() {
        return $"{_klass._name} instance";
    }
}