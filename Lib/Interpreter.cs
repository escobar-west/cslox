namespace Lib;

public class Interpreter : Expr.IVisitor<object?>, Stmt.IVisitor {
    readonly Dictionary<Expr, int> _locals = [];
    readonly Env _globals;
    Env _environment;

    public Interpreter() {
        _globals = new();
        ClockFunc clock_func = new();
        PrintFunc print_func = new();
        ToStringFunc str_func = new();
        _globals.Define(clock_func._name, clock_func);
        _globals.Define(print_func._name, print_func);
        _globals.Define(str_func._name, str_func);
        _environment = _globals;
    }

    public void Interpret(List<Stmt> statements) {
        try {
            foreach (var stmt in statements)
                stmt.Accept(this);
        } catch (Exception e) {
            Machine.RuntimeError(e);
        }
    }

    public void Resolve(Expr expr, int depth) {
        _locals[expr] = depth;
    }

    public void VisitBlockStmt(Stmt.Block stmt) {
        ExecuteBlock(stmt._statements, new Env(_environment));
    }

    public void VisitClassStmt(Stmt.Class stmt) {
        _environment.Define(stmt._name._lexeme, null);
        LoxClass klass = new LoxClass(stmt._name._lexeme);
        _environment.Assign(stmt._name._lexeme, klass);
    }

    public void ExecuteBlock(List<Stmt> statements, Env block_env) {
        Env cached_env = _environment;
        _environment = block_env;
        try {
            foreach (var stmt in statements)
                stmt.Accept(this);
        } finally {
            _environment = cached_env;
        }
    }

    public void VisitReturnStmt(Stmt.Return stmt) {
        object? value = stmt._value?.Accept(this);
        throw new Return(value);
    }

    public void VisitExpressionStmt(Stmt.Expression stmt) {
        stmt._expression.Accept(this);
    }

    public void VisitFunctionStmt(Stmt.Function stmt) {
        LoxFunc func = new(stmt, _environment);
        _environment.Define(stmt._name._lexeme, func);
    }

    public void VisitVarStmt(Stmt.Var stmt) {
        object? value = stmt._initializer?.Accept(this);
        _environment.Define(stmt._name._lexeme, value);
    }

    public void VisitIfStmt(Stmt.If stmt) {
        object? condition = stmt._condition.Accept(this);
        if (ToBool(condition))
            stmt._thenBranch.Accept(this);
        else
            stmt._elseBranch?.Accept(this);
    }

    public void VisitWhileStmt(Stmt.While stmt) {
        while (ToBool(stmt._condition.Accept(this)))
            stmt._body.Accept(this);
    }

    public object? VisitCallExpr(Expr.Call expr) {
        object? callee = expr._callee.Accept(this);
        List<object?> args = [];
        foreach (var arg in expr._args)
            args.Add(arg.Accept(this));
        ILoxCallable func = callee as ILoxCallable ??
             throw new InvalidOperationException("Can only call functions and classes.");
        if (args.Count != func.Arity())
            throw new InvalidOperationException("Wrong arity");
        return func.Call(this, args);
    }

    public object? VisitLogicalExpr(Expr.Logical expr) {
        object? left = expr._left.Accept(this);
        return (ToBool(left), expr._op._type) switch {
            (true, TokenType.OR) => left,
            (false, TokenType.AND) => left,
            _ => expr._right.Accept(this),
        };
    }

    public object? VisitAssignExpr(Expr.Assign expr) {
        object? value = expr._value.Accept(this);
        if (_locals.TryGetValue(expr, out int distance))
            _environment.AssignAt(expr._name._lexeme, value, distance);
        else
            _globals.Assign(expr._name._lexeme, value);
        return value;
    }

    public object? VisitVariableExpr(Expr.Variable expr) {
        return LookUpVariable(expr._name, expr);
    }

    object? LookUpVariable(Token name, Expr expr) {
        if (_locals.TryGetValue(expr, out int distance)) {
            return _environment.GetAt(name._lexeme, distance);
        }
        return _globals.Get(name._lexeme);
    }

    public object? VisitLiteralExpr(Expr.Literal expr) {
        return expr._value;
    }

    public object? VisitGroupingExpr(Expr.Grouping expr) {
        return expr._expression.Accept(this);
    }

    public object? VisitUnaryExpr(Expr.Unary expr) {
        object? right = expr._right.Accept(this);
        return expr._op._type switch {
            TokenType.MINUS when right is double f => -f,
            TokenType.BANG => ToBool(right),
            _ => throw new InvalidOperationException($"{expr._op._type} couldnt be parsed"),
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

    public static bool ToBool(object? obj) {
        return obj switch {
            null => false,
            bool b => b,
            _ => true,
        };
    }
}