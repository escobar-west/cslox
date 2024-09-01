namespace Interpreter;

public class AstPrinter : Expr.Visitor<string> {
    public string print(Expr expr) {
        return expr.accept(this);
    }

    public string visitBinaryExpr(Expr.Binary expr) {
        return polish_notation(expr._op._lexeme, [expr._left, expr._right]);
    }

    public string visitGroupingExpr(Expr.Grouping expr) {
        return expr._expression.accept(this);
    }

    public string visitLiteralExpr(Expr.Literal expr) {
        if (expr._value == null) return "nil";
        return expr._value!.ToString()!;
    }

    public string visitUnaryExpr(Expr.Unary expr) {
        return polish_notation(expr._op._lexeme, [expr._right]);
    }

    string polish_notation(string name, Expr[] exprs) {
        var builder = new System.Text.StringBuilder();
        builder.Append(name);
        foreach (var expr in exprs) {
            builder.Append($" {expr.accept(this)}");
        }
        return builder.ToString();
    }
}