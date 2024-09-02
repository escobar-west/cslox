namespace Lib;

public class AstPrinter : Expr.IVisitor<string> {
    public string Print(Expr expr) {
        return expr.Accept(this);
    }

    public string VisitBinaryExpr(Expr.Binary expr) {
        return PolishNotation(expr._op._lexeme, [expr._left, expr._right]);
    }

    public string VisitGroupingExpr(Expr.Grouping expr) {
        return expr._expression.Accept(this);
    }

    public string VisitLiteralExpr(Expr.Literal expr) {
        return expr._value?.ToString() ?? "nil";
    }

    public string VisitUnaryExpr(Expr.Unary expr) {
        return PolishNotation(expr._op._lexeme, [expr._right]);
    }

    public string VisitEmptyExpr(Expr.Empty expr) {
        return "";
    }

    string PolishNotation(string name, Expr[] exprs) {
        var builder = new System.Text.StringBuilder();
        builder.Append(name);
        foreach (var expr in exprs) {
            builder.Append($" {expr.Accept(this)}");
        }
        return builder.ToString();
    }
}