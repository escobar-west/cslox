namespace Lib;

public class Env {
    public readonly Env? _enclosing;
    readonly Dictionary<string, object?> _values = [];

    public Env() {
        _enclosing = null;
    }

    public Env(Env? enclosing) {
        _enclosing = enclosing;
    }

    public void Define(string name, object? value) {
        _values[name] = value;
    }

    public void Assign(string name, object? value) {
        if (_values.ContainsKey(name)) {
            _values[name] = value;
            return;
        }
        if (_enclosing != null) {
            _enclosing.Assign(name, value);
            return;
        }
        throw new InvalidOperationException($"Undefined variable '{name}'.");
    }

    public void AssignAt(string name, object? value, int distance) {
        Env? ancestor = Ancestor(distance);
        if (ancestor != null)
            ancestor._values[name] = value;
    }

    public object? Get(string name) {
        if (_values.TryGetValue(name, out object? obj))
            return obj;
        if (_enclosing != null)
            return _enclosing.Get(name);
        throw new InvalidOperationException($"Undefined variable '{name}'.");
    }

    public object? GetAt(string name, int distance) {
        object? value = null;
        Ancestor(distance)?._values.TryGetValue(name, out value);
        return value;
    }

    Env? Ancestor(int distance) {
        Env? env = this;
        for (int i = 0; i < distance; i++)
            env = env?._enclosing;
        return env;
    }
}