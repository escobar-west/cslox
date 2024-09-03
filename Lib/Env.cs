namespace Lib;

public class Env {
    readonly Env? _enclosing;
    readonly Dictionary<string, object?> _values = [];

    public Env() {
        _enclosing = null;
    }

    public Env(Env? enclosing) {
        _enclosing = enclosing;
    }

    public void Define(Token name, object? value) {
        _values[name._lexeme] = value;
    }

    public void Assign(Token name, object? value) {
        if (_values.ContainsKey(name._lexeme)) {
            _values[name._lexeme] = value;
            return;
        }
        if (_enclosing != null) {
            _enclosing.Assign(name, value);
            return;
        }
        throw new InvalidOperationException($"Undefined variable '{name._lexeme}'.");
    }

    public object? Get(Token name) {
        if (_values.TryGetValue(name._lexeme, out object? obj))
            return obj;
        if (_enclosing != null) return _enclosing?.Get(name);
        throw new InvalidOperationException($"Undefined variable '{name._lexeme}'.");
    }
}