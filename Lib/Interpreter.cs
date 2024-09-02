namespace Lib;

public class Interpreter : Expr.IVisitor<object?> {
    public object? Interpret(Expr expr) {
        try {
            return expr.Accept(this);
        } catch (Exception e) {
            Machine.RuntimeError(e);
            return null;
        }
    }
    public object? VisitLiteralExpr(Expr.Literal expr) {
        return expr._value;
    }

    public object? VisitGroupingExpr(Expr.Grouping expr) {
        return expr._expression.Accept(this);
    }

    public object? VisitUnaryExpr(Expr.Unary expr) {
        object? right = expr._right.Accept(this);
        if (right == null) return null;
        return expr._op._type switch {
            TokenType.MINUS => -(double)right,
            TokenType.BANG => (bool)right,
            _ => null,
        };
    }

    public object? VisitBinaryExpr(Expr.Binary expr) {
        object? left = expr._left.Accept(this);
        object? right = expr._right.Accept(this);
        // Only a subset of binary expressions are allowed for nulls
        if (left == null || right == null) {
            return expr._op._type switch {
                TokenType.EQUAL_EQUAL => left == right,
                TokenType.BANG_EQUAL => left != right,
                _ => throw new InvalidOperationException($"{expr._op._type} not supported for nil"),
            };
        }
        return expr._op._type switch {
            TokenType.MINUS => (double)left - (double)right,
            TokenType.SLASH => (double)left / (double)right,
            TokenType.STAR => (double)left * (double)right,
            TokenType.GREATER => (double)left > (double)right,
            TokenType.GREATER_EQUAL => (double)left >= (double)right,
            TokenType.LESS => (double)left < (double)right,
            TokenType.LESS_EQUAL => (double)left <= (double)right,
            // ==
            TokenType.EQUAL_EQUAL when left is string s => s == (string)right,
            TokenType.EQUAL_EQUAL when left is double f => f == (double)right,
            TokenType.EQUAL_EQUAL when left is bool b => b == (bool)right,
            TokenType.EQUAL_EQUAL =>
                throw new InvalidOperationException($"== Could not interpret LHS {left}"),
            // !=
            TokenType.BANG_EQUAL when left is string s => s != (string)right,
            TokenType.BANG_EQUAL when left is double f => f != (double)right,
            TokenType.BANG_EQUAL when left is bool b => b != (bool)right,
            TokenType.BANG_EQUAL =>
                throw new InvalidOperationException($"!= Could not interpret LHS {left}"),
            // +
            TokenType.PLUS when left is string s => s + (string)right,
            TokenType.PLUS when left is double f => f + (double)right,
            TokenType.PLUS =>
                throw new InvalidOperationException($"+ Could not interpret LHS {left}"),
            _ => null,
        };
    }

    public object? VisitEmptyExpr(Expr.Empty expr) {
        return null;
    }
}