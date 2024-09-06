namespace Lib;

public class LoxClass {
    string _name;

    public LoxClass(string name) {
        _name = name;
    }

    public override string ToString() {
        return _name;
    }
}