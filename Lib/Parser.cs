namespace Lib;

public class Parser {
    readonly List<Token> _tokens;
    int _current = 0;

    public Parser(List<Token> tokens) {
        _tokens = tokens;
    }

    public List<Stmt> Parse() {
        List<Stmt> statements = [];
        while (!IsAtEnd()) {
            try {
                statements.Add(Declaration());
            } catch (Exception e) {
                Machine.RuntimeError(e);
                Synchronize();
            }
        }
        return statements;
    }
    Stmt Declaration() {
        if (Match(TokenType.FUN))
            return Function("function");
        if (Match(TokenType.VAR))
            return VarDeclaration();
        return Statement();
    }

    Stmt.Function Function(string kind) {
        Token name = Consume(TokenType.IDENTIFIER, $"Expect {kind} name.");
        Consume(TokenType.LEFT_PAREN, $"Expect '(' after {kind} name.");
        List<Token> parameters = [];
        if (!Check(TokenType.RIGHT_PAREN)) {
            do {
                if (parameters.Count >= 255)
                    Error(Peek(), "Can't have more than 255 parameters.");
                parameters.Add(Consume(TokenType.IDENTIFIER, "Expect parameter name."));
            } while (Match(TokenType.COMMA));
        }
        Consume(TokenType.RIGHT_PAREN, "Expect ')' after parameters.");
        Consume(TokenType.LEFT_BRACE, $"Expect '{{' before {kind} body.");
        List<Stmt> body = Block();
        return new Stmt.Function(name, parameters, body);
    }

    Stmt.Var VarDeclaration() {
        Token name = Consume(TokenType.IDENTIFIER, "Expect variable name.");
        Expr? initializer = Match(TokenType.EQUAL) ? Expression() : null;
        Consume(TokenType.SEMICOLON, "Expect ';' after variable declaration.");
        return new Stmt.Var(name, initializer);
    }

    Stmt Statement() {
        if (Match(TokenType.FOR)) return ForStatement();
        if (Match(TokenType.IF)) return IfStatement();
        if (Match(TokenType.RETURN)) return ReturnStatement();
        if (Match(TokenType.WHILE)) return WhileStatement();
        if (Match(TokenType.LEFT_BRACE)) return new Stmt.Block(Block());
        return ExpressionStatement();
    }

    Stmt ForStatement() {
        Consume(TokenType.LEFT_PAREN, "Expect '(' after 'for'.");
        Stmt? initializer = null;
        if (Match(TokenType.VAR)) {
            initializer = VarDeclaration();
        } else if (!Match(TokenType.SEMICOLON))
            initializer = ExpressionStatement();
        Expr condition = !Check(TokenType.SEMICOLON)
                         ? Expression()
                         : new Expr.Literal(true);
        Consume(TokenType.SEMICOLON, "Expect ';' after loop condition.");
        Expr? increment = !Check(TokenType.RIGHT_PAREN)
                          ? Expression()
                          : null;
        Consume(TokenType.RIGHT_PAREN, "Expect ')' after for clauses.");
        Stmt body = Statement();
        if (increment != null)
            body = new Stmt.Block([body, new Stmt.Expression(increment)]);
        body = new Stmt.While(condition, body);
        if (initializer != null)
            body = new Stmt.Block([initializer, body]);
        return body;
    }

    Stmt.While WhileStatement() {
        Consume(TokenType.LEFT_PAREN, "Expect '(' after 'while'.");
        Expr condition = Expression();
        Consume(TokenType.RIGHT_PAREN, "Expect ')' after condition.");
        Stmt body = Statement();
        return new Stmt.While(condition, body);
    }

    Stmt.If IfStatement() {
        Consume(TokenType.LEFT_PAREN, "Expect '(' after 'if'.");
        Expr condition = Expression();
        Consume(TokenType.RIGHT_PAREN, "Expect ')' after condition.");
        Stmt thenBranch = Statement();
        Stmt? elseBranch = Match(TokenType.ELSE) ? Statement() : null;
        return new Stmt.If(condition, thenBranch, elseBranch);
    }

    private Stmt.Return ReturnStatement() {
        Token keyword = Previous();
        Expr? value = Check(TokenType.SEMICOLON) ? null : Expression();
        Consume(TokenType.SEMICOLON, "Expect ';' after return value.");
        return new Stmt.Return(keyword, value);
    }

    Stmt.Expression ExpressionStatement() {
        Expr value = Expression();
        Consume(TokenType.SEMICOLON, "Expect ';' after value.");
        return new Stmt.Expression(value);

    }

    List<Stmt> Block() {
        List<Stmt> statements = [];
        while (!Check(TokenType.RIGHT_BRACE) && !IsAtEnd()) {
            statements.Add(Declaration());
        }
        Consume(TokenType.RIGHT_BRACE, "Expect '}' after block.");
        return statements;
    }

    Expr Expression() {
        return Assignment();
    }

    Expr Assignment() {
        Expr expr = Or();
        if (Match(TokenType.EQUAL)) {
            Token equals = Previous();
            Expr value = Assignment();
            switch (expr) {
                case Expr.Variable v:
                    Token name = v._name;
                    return new Expr.Assign(name, value);
                default:
                    Error(equals, "Invalied assignment target");
                    break;
            }
        }
        return expr;
    }

    Expr Or() {
        Expr expr = And();
        while (Match(TokenType.OR)) {
            Token op = Previous();
            Expr right = And();
            expr = new Expr.Logical(expr, op, right);
        }
        return expr;
    }

    Expr And() {
        Expr expr = Equality();
        while (Match(TokenType.AND)) {
            Token op = Previous();
            Expr right = Equality();
            expr = new Expr.Logical(expr, op, right);
        }
        return expr;
    }

    Expr Equality() {
        Expr expr = Comparison();
        while (Match([TokenType.BANG_EQUAL, TokenType.EQUAL_EQUAL])) {
            Token op = Previous();
            Expr right = Comparison();
            expr = new Expr.Binary(expr, op, right);
        }
        return expr;
    }

    Expr Comparison() {
        Expr expr = Term();
        while (Match(
            [TokenType.GREATER, TokenType.GREATER_EQUAL, TokenType.LESS, TokenType.LESS_EQUAL])
        ) {
            Token op = Previous();
            Expr right = Term();
            expr = new Expr.Binary(expr, op, right);
        }
        return expr;
    }

    Expr Term() {
        Expr expr = Factor();
        while (Match([TokenType.MINUS, TokenType.PLUS])) {
            Token op = Previous();
            Expr right = Factor();
            expr = new Expr.Binary(expr, op, right);
        }
        return expr;
    }

    Expr Factor() {
        Expr expr = Unary();
        while (Match([TokenType.SLASH, TokenType.STAR])) {
            Token op = Previous();
            Expr right = Unary();
            expr = new Expr.Binary(expr, op, right);
        }
        return expr;
    }

    Expr Unary() {
        if (Match([TokenType.BANG, TokenType.MINUS])) {
            Token op = Previous();
            Expr right = Unary();
            return new Expr.Unary(op, right);
        }
        return Call();
    }

    Expr Call() {
        Expr expr = Primary();
        while (true) {
            if (Match(TokenType.LEFT_PAREN))
                expr = FinishCall(expr);
            else
                break;
        }
        return expr;
    }

    Expr.Call FinishCall(Expr callee) {
        List<Expr> args = [];
        if (!Check(TokenType.RIGHT_PAREN)) {
            do {
                if (args.Count >= 255)
                    Error(Peek(), "Can't have more than 255 arguments.");
                args.Add(Expression());
            } while (Match(TokenType.COMMA));
        }
        Token paren = Consume(
            TokenType.RIGHT_PAREN, "Expect ')' after arguments."
        );
        return new Expr.Call(callee, paren, args);
    }

    Expr Primary() {
        if (Match(TokenType.IDENTIFIER)) return new Expr.Variable(Previous());
        if (Match(TokenType.FALSE)) return new Expr.Literal(false);
        if (Match(TokenType.TRUE)) return new Expr.Literal(true);
        if (Match(TokenType.NIL)) return new Expr.Literal(null);
        if (Match([TokenType.NUMBER, TokenType.STRING])) {
            return new Expr.Literal(Previous()._literal);
        }
        if (Match(TokenType.LEFT_PAREN)) {
            Expr expr = Expression();
            Consume(TokenType.RIGHT_PAREN, "Expect ')' after expression.");
            return new Expr.Grouping(expr);
        }
        throw Error(Peek(), "Expect expression");
    }

    Token Consume(TokenType type, string message) {
        if (Check(type))
            return Advance();
        throw Error(Peek(), message);
    }

    bool Match(TokenType type) {
        if (Check(type)) {
            Advance();
            return true;
        }
        return false;
    }

    bool Match(TokenType[] types) {
        foreach (TokenType type in types) {
            if (Check(type)) {
                Advance();
                return true;
            }
        }
        return false;
    }

    bool Check(TokenType type) {
        if (IsAtEnd())
            return false;
        return Peek()._type == type;
    }

    Token Advance() {
        if (!IsAtEnd())
            _current++;
        return Previous();
    }

    bool IsAtEnd() {
        return Peek()._type == TokenType.EOF;
    }

    Token Peek() {
        return _tokens[_current];
    }

    Token Previous() {
        return _tokens[_current - 1];
    }

    void Synchronize() {
        Advance();
        while (!IsAtEnd()) {
            if (Previous()._type == TokenType.SEMICOLON)
                return;
            switch (Peek()._type) {
                case TokenType.CLASS:
                case TokenType.FUN:
                case TokenType.VAR:
                case TokenType.FOR:
                case TokenType.IF:
                case TokenType.WHILE:
                case TokenType.RETURN:
                    return;
            }
            Advance();
        }
    }

    static ParseException Error(Token token, string message) {
        Machine.Error(token, message);
        return new ParseException();
    }


    public class ParseException : Exception {
        public ParseException() { }
        public ParseException(string message) : base(message) { }
        public ParseException(string message, Exception inner) :
            base(message, inner) { }
    }
}