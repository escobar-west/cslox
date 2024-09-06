namespace Lib;

class Return : Exception {
    public readonly object? _value;

    public Return(object? value) {
        _value = value;
    }
}