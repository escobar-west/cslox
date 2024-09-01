namespace Interpreter;

public abstract class Expr {
    protected interface Visitor<T> {
        T visitBinaryExpr(Binary expr);
        T visitGroupingExpr(Grouping expr);
        T visitLiteralExpr(Literal expr);
        T visitUnaryExpr(Unary expr);
    }

    protected abstract T accept<T>(Visitor<T> visitor);

    public class Binary : Expr {
        readonly Expr _left;
        readonly Token _op;
        readonly Expr _right;

        public Binary(Expr left, Token op, Expr right) {
            _left = left;
            _op = op;
            _right = right;
        }

        protected override T accept<T>(Visitor<T> visitor) {
            return visitor.visitBinaryExpr(this);
        }
    }

    public class Grouping : Expr {
        readonly Expr _expression;

        public Grouping(Expr expression) {
            _expression = expression;
        }

        protected override T accept<T>(Visitor<T> visitor) {
            return visitor.visitGroupingExpr(this);
        }
    }

    public class Literal : Expr {
        readonly Object _value;

        public Literal(Object value) {
            _value = value;
        }

        protected override T accept<T>(Visitor<T> visitor) {
            return visitor.visitLiteralExpr(this);
        }
    }

    public class Unary : Expr {
        readonly Token _op;
        readonly Expr _right;

        public Unary(Token op, Expr right) {
            _op = op;
            _right = right;
        }

        protected override T accept<T>(Visitor<T> visitor) {
            return visitor.visitUnaryExpr(this);
        }
    }
}
