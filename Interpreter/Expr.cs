namespace Interpreter;

public abstract class Expr {
    public interface Visitor<T> {
        T visitBinaryExpr(Binary expr);
        T visitGroupingExpr(Grouping expr);
        T visitLiteralExpr(Literal expr);
        T visitUnaryExpr(Unary expr);
    }

    public abstract T accept<T>(Visitor<T> visitor);

    public class Binary : Expr {
        public readonly Expr _left;
        public readonly Token _op;
        public readonly Expr _right;

        public Binary(Expr left, Token op, Expr right) {
            _left = left;
            _op = op;
            _right = right;
        }

        public override T accept<T>(Visitor<T> visitor) {
            return visitor.visitBinaryExpr(this);
        }
    }

    public class Grouping : Expr {
        public readonly Expr _expression;

        public Grouping(Expr expression) {
            _expression = expression;
        }

        public override T accept<T>(Visitor<T> visitor) {
            return visitor.visitGroupingExpr(this);
        }
    }

    public class Literal : Expr {
        public readonly Object? _value;

        public Literal(Object? value) {
            _value = value;
        }

        public override T accept<T>(Visitor<T> visitor) {
            return visitor.visitLiteralExpr(this);
        }
    }

    public class Unary : Expr {
        public readonly Token _op;
        public readonly Expr _right;

        public Unary(Token op, Expr right) {
            _op = op;
            _right = right;
        }

        public override T accept<T>(Visitor<T> visitor) {
            return visitor.visitUnaryExpr(this);
        }
    }
}
