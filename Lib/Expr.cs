namespace Lib;

public abstract class Expr {
    public interface IVisitor<T> {
        T VisitBinaryExpr(Binary expr);
        T VisitGroupingExpr(Grouping expr);
        T VisitLiteralExpr(Literal expr);
        T VisitUnaryExpr(Unary expr);
        T VisitEmptyExpr(Empty expr);
    }

    public abstract T Accept<T>(IVisitor<T> visitor);

    public class Binary : Expr {
        public readonly Expr _left;
        public readonly Token _op;
        public readonly Expr _right;

        public Binary(Expr left, Token op, Expr right) {
            _left = left;
            _op = op;
            _right = right;
        }

        public override T Accept<T>(IVisitor<T> visitor) {
            return visitor.VisitBinaryExpr(this);
        }
    }

    public class Grouping : Expr {
        public readonly Expr _expression;

        public Grouping(Expr expression) {
            _expression = expression;
        }

        public override T Accept<T>(IVisitor<T> visitor) {
            return visitor.VisitGroupingExpr(this);
        }
    }

    public class Literal : Expr {
        public readonly object? _value;

        public Literal(object? value) {
            _value = value;
        }

        public override T Accept<T>(IVisitor<T> visitor) {
            return visitor.VisitLiteralExpr(this);
        }
    }

    public class Unary : Expr {
        public readonly Token _op;
        public readonly Expr _right;

        public Unary(Token op, Expr right) {
            _op = op;
            _right = right;
        }

        public override T Accept<T>(IVisitor<T> visitor) {
            return visitor.VisitUnaryExpr(this);
        }
    }

    public class Empty : Expr {

        public Empty() {
        }

        public override T Accept<T>(IVisitor<T> visitor) {
            return visitor.VisitEmptyExpr(this);
        }
    }
}
