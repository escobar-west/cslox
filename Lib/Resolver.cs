namespace Lib;

public class Resolver : Expr.IVisitor<object?>, Stmt.IVisitor {
    readonly Interpreter _interpreter;
    readonly Stack<Dictionary<string, bool>> _scopes = new();
    FunctionType _currentFunc = FunctionType.NONE;
    ClassType _currentClass = ClassType.NONE;

    public Resolver(Interpreter interpreter) {
        _interpreter = interpreter;
    }

    public void VisitBlockStmt(Stmt.Block stmt) {
        BeginScope();
        Resolve(stmt._statements);
        EndScope();
    }

    public void VisitClassStmt(Stmt.Class stmt) {
        ClassType enclosingClass = _currentClass;
        _currentClass = ClassType.CLASS;
        Declare(stmt._name);
        Define(stmt._name);
        if (stmt._superclass?._name._lexeme == stmt._name._lexeme)
            Machine.Error(stmt._superclass._name, "A class can't inherit from itself.");
        if (stmt._superclass != null) {
            _currentClass = ClassType.SUBCLASS;
            Resolve(stmt._superclass);
            BeginScope();
            _scopes.Peek()["super"] = true;
        }
        BeginScope();
        _scopes.Peek()["this"] = true;
        foreach (var method in stmt._methods) {
            var declaration = method._name._lexeme == "init" ?
                FunctionType.INIT :
                FunctionType.METHOD;
            ResolveFunction(method, declaration);
        }
        EndScope();
        if (stmt._superclass != null)
            EndScope();
        _currentClass = enclosingClass;
    }

    public object? VisitSuperExpr(Expr.Super expr) {
        if (_currentClass != ClassType.SUBCLASS)
            Machine.Error(expr._keyword, "Cant use 'super' outside a subclass'");
        ResolveLocal(expr, expr._keyword);
        return null;
    }

    public void VisitVarStmt(Stmt.Var stmt) {
        Declare(stmt._name);
        Resolve(stmt._initializer);
        Define(stmt._name);
    }

    public object? VisitThisExpr(Expr.This expr) {
        if (_currentClass == ClassType.NONE) {
            Machine.Error(expr._keyword, "Can't read local variable in its own initializer.");
            return null;
        }
        ResolveLocal(expr, expr._keyword);
        return null;
    }

    public object? VisitSetExpr(Expr.Set expr) {
        Resolve(expr._value);
        Resolve(expr._instance);
        return null;
    }

    public object? VisitGetExpr(Expr.Get expr) {
        Resolve(expr._instance);
        return null;
    }

    public object? VisitVariableExpr(Expr.Variable expr) {
        if (_scopes.Count != 0
                && _scopes.Peek().TryGetValue(expr._name._lexeme, out bool value)
                && value == false)
            Machine.Error(expr._name, "Can't read local variable in its own initializer.");
        ResolveLocal(expr, expr._name);
        return null;
    }

    public object? VisitAssignExpr(Expr.Assign expr) {
        Resolve(expr._value);
        ResolveLocal(expr, expr._name);
        return null;
    }

    public void VisitFunctionStmt(Stmt.Function stmt) {
        Declare(stmt._name);
        Define(stmt._name);
        ResolveFunction(stmt, FunctionType.FUNCTION);
    }

    public void VisitExpressionStmt(Stmt.Expression stmt) {
        Resolve(stmt._expression);
    }

    public void VisitIfStmt(Stmt.If stmt) {
        Resolve(stmt._condition);
        Resolve(stmt._thenBranch);
        Resolve(stmt._elseBranch);
    }

    public void VisitReturnStmt(Stmt.Return stmt) {
        if (_currentFunc == FunctionType.NONE)
            Machine.Error(stmt._keyword, "Can't return from top-level code.");
        if (stmt._value != null) {
            if (_currentFunc == FunctionType.INIT)
                Machine.Error(stmt._keyword, "Can't return a value from an initializer.");
            Resolve(stmt._value);
        }
    }

    public void VisitWhileStmt(Stmt.While stmt) {
        Resolve(stmt._condition);
        Resolve(stmt._body);
    }

    public object? VisitBinaryExpr(Expr.Binary expr) {
        Resolve(expr._left);
        Resolve(expr._right);
        return null;
    }

    public object? VisitCallExpr(Expr.Call expr) {
        Resolve(expr._callee);
        foreach (var arg in expr._args)
            Resolve(arg);
        return null;
    }

    public object? VisitEmptyExpr(Expr.Empty expr) {
        return null;
    }

    public object? VisitGroupingExpr(Expr.Grouping expr) {
        Resolve(expr._expression);
        return null;
    }

    public object? VisitLiteralExpr(Expr.Literal expr) {
        return null;
    }

    public object? VisitLogicalExpr(Expr.Logical expr) {
        Resolve(expr._left);
        Resolve(expr._right);
        return null;
    }

    public object? VisitUnaryExpr(Expr.Unary expr) {
        Resolve(expr._right);
        return null;
    }

    void ResolveFunction(Stmt.Function func, FunctionType type) {
        FunctionType enclosingFunction = _currentFunc;
        _currentFunc = type;
        BeginScope();
        foreach (var p in func._parameters) {
            Declare(p);
            Define(p);
        }
        Resolve(func._body);
        EndScope();
        _currentFunc = enclosingFunction;
    }

    void ResolveLocal(Expr expr, Token name) {
        int i = 0;
        foreach (var scope in _scopes) {
            if (scope.TryGetValue(name._lexeme, out _)) {
                _interpreter.Resolve(expr, i);
                return;
            }
            i++;
        }
    }

    void Declare(Token name) {
        if (_scopes.Count == 0)
            return;
        var scope = _scopes.Peek();
        if (scope.ContainsKey(name._lexeme))
            Machine.Error(name, "Already a variable with this name in this scope.");
        scope[name._lexeme] = false;
    }

    void Define(Token name) {
        if (_scopes.Count == 0)
            return;
        _scopes.Peek()[name._lexeme] = true;
    }

    void BeginScope() {
        _scopes.Push([]);
    }

    void EndScope() {
        _scopes.Pop();
    }

    public void Resolve(List<Stmt> statements) {
        foreach (var stmt in statements) {
            Resolve(stmt);
        }
    }

    public void Resolve(Stmt? stmt) {
        stmt?.Accept(this);
    }

    public void Resolve(Expr? expr) {
        expr?.Accept(this);
    }
}

enum FunctionType {
    NONE,
    FUNCTION,
    INIT,
    METHOD,
}

enum ClassType {
    NONE,
    CLASS,
    SUBCLASS,
}