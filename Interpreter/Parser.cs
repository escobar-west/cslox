namespace Interpreter;

public class Parser {
    readonly List<Token> _tokens;
    int _current = 0;

    public Parser(List<Token> tokens) {
        _tokens = tokens;
    }

    public Expr? parse() {
        try {
            return expression();
        } catch (ParseException e) {
            return null;
        }
    }

    Expr expression() {
        return equality();
    }

    Expr equality() {
        Expr expr = comparison();
        while (match([TokenType.BANG_EQUAL, TokenType.EQUAL_EQUAL])) {
            Token op = previous();
            Expr right = comparison();
            expr = new Expr.Binary(expr, op, right);
        }
        return expr;
    }

    Expr comparison() {
        Expr expr = term();
        while (match(
            [TokenType.GREATER, TokenType.GREATER_EQUAL, TokenType.LESS, TokenType.LESS_EQUAL])
        ) {
            Token op = previous();
            Expr right = term();
            expr = new Expr.Binary(expr, op, right);
        }
        return expr;
    }

    Expr term() {
        Expr expr = factor();
        while (match([TokenType.MINUS, TokenType.PLUS])) {
            Token op = previous();
            Expr right = factor();
            expr = new Expr.Binary(expr, op, right);
        }
        return expr;
    }

    Expr factor() {
        Expr expr = unary();
        while (match([TokenType.SLASH, TokenType.STAR])) {
            Token op = previous();
            Expr right = unary();
            expr = new Expr.Binary(expr, op, right);
        }
        return expr;
    }

    Expr unary() {
        if (match([TokenType.BANG, TokenType.MINUS])) {
            Token op = previous();
            Expr right = unary();
            return new Expr.Unary(op, right);
        }
        return primary();
    }

    Expr primary() {
        if (match([TokenType.FALSE])) return new Expr.Literal(false);
        if (match([TokenType.TRUE])) return new Expr.Literal(true);
        if (match([TokenType.NIL])) return new Expr.Literal(null);
        if (match([TokenType.NUMBER, TokenType.STRING])) {
            return new Expr.Literal(previous()._literal);
        }
        if (match([TokenType.LEFT_PAREN])) {
            Expr expr = expression();
            consume(TokenType.RIGHT_PAREN, "Expect ')' after expression.");
            return new Expr.Grouping(expr);
        }
        throw error(peek(), "Expect expression");
    }

    Token consume(TokenType type, string message) {
        if (check(type)) return advance();
        throw error(peek(), message);
    }

    bool match(TokenType[] types) {
        foreach (TokenType type in types) {
            if (check(type)) {
                advance();
                return true;
            }
        }
        return false;
    }

    bool check(TokenType type) {
        if (isAtEnd()) return false;
        return peek()._type == type;
    }

    Token advance() {
        if (!isAtEnd()) _current++;
        return previous();
    }

    bool isAtEnd() {
        return peek()._type == TokenType.EOF;
    }

    Token peek() {
        return _tokens[_current];
    }

    Token previous() {
        return _tokens[_current - 1];
    }

    void synchronize() {
        advance();
        while (!isAtEnd()) {
            if (previous()._type == TokenType.SEMICOLON) return;
            switch (peek()._type) {
                case TokenType.CLASS:
                case TokenType.FUN:
                case TokenType.VAR:
                case TokenType.FOR:
                case TokenType.IF:
                case TokenType.WHILE:
                case TokenType.PRINT:
                case TokenType.RETURN:
                    return;
            }
            advance();
        }
    }

    ParseException error(Token token, string message) {
        Machine.error(token, message);
        return new ParseException();
    }


    public class ParseException : Exception {
        public ParseException() { }
        public ParseException(string message) : base(message) { }
        public ParseException(string message, Exception inner) :
            base(message, inner) { }
    }
}