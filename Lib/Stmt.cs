namespace Lib;

public abstract class Stmt {
    public interface IVisitor {
        void VisitBlockStmt(Block stmt);
        void VisitExpressionStmt(Expression stmt);
        void VisitIfStmt(If stmt);
        void VisitPrintStmt(Print stmt);
        void VisitVarStmt(Var stmt);
        void VisitWhileStmt(While stmt);
    }

    public abstract void Accept(IVisitor visitor);

    public class Block : Stmt {
        public readonly List<Stmt> _statements;

        public Block(List<Stmt> statements) {
            _statements = statements;
        }

        public override void Accept(IVisitor visitor) {
            visitor.VisitBlockStmt(this);
        }
    }

    public class Expression : Stmt {
        public readonly Expr _expression;

        public Expression(Expr expression) {
            _expression = expression;
        }

        public override void Accept(IVisitor visitor) {
            visitor.VisitExpressionStmt(this);
        }
    }

    public class If : Stmt {
        public readonly Expr _condition;
        public readonly Stmt _thenBranch;
        public readonly Stmt? _elseBranch;

        public If(Expr condition, Stmt thenBranch, Stmt? elseBranch) {
            _condition = condition;
            _thenBranch = thenBranch;
            _elseBranch = elseBranch;
        }

        public override void Accept(IVisitor visitor) {
            visitor.VisitIfStmt(this);
        }
    }

    public class Print : Stmt {
        public readonly Expr _expression;

        public Print(Expr expression) {
            _expression = expression;
        }

        public override void Accept(IVisitor visitor) {
            visitor.VisitPrintStmt(this);
        }
    }

    public class Var : Stmt {
        public readonly Token _name;
        public readonly Expr? _initializer;

        public Var(Token name, Expr? initializer) {
            _name = name;
            _initializer = initializer;
        }

        public override void Accept(IVisitor visitor) {
            visitor.VisitVarStmt(this);
        }
    }

    public class While : Stmt {
        public readonly Expr _condition;
        public readonly Stmt _body;

        public While(Expr condition, Stmt body) {
            _condition = condition;
            _body = body;
        }

        public override void Accept(IVisitor visitor) {
            visitor.VisitWhileStmt(this);
        }
    }
}
