namespace Lib;

public class Parser {
    readonly List<Token> _tokens;
    int _current = 0;

    public Parser(List<Token> tokens) {
        _tokens = tokens;
    }

    public Expr Parse() {
        try {
            return Expression();
        } catch (ParseException e) {
            Console.WriteLine(e.ToString());
            return new Expr.Empty();
        }
    }

    Expr Expression() {
        return Equality();
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
        return Primary();
    }

    Expr Primary() {
        if (Match([TokenType.FALSE])) return new Expr.Literal(false);
        if (Match([TokenType.TRUE])) return new Expr.Literal(true);
        if (Match([TokenType.NIL])) return new Expr.Literal(null);
        if (Match([TokenType.NUMBER, TokenType.STRING])) {
            return new Expr.Literal(Previous()._literal);
        }
        if (Match([TokenType.LEFT_PAREN])) {
            Expr expr = Expression();
            Consume(TokenType.RIGHT_PAREN, "Expect ')' after expression.");
            return new Expr.Grouping(expr);
        }
        throw Error(Peek(), "Expect expression");
    }

    Token Consume(TokenType type, string message) {
        if (Check(type)) return Advance();
        throw Error(Peek(), message);
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
        if (IsAtEnd()) return false;
        return Peek()._type == type;
    }

    Token Advance() {
        if (!IsAtEnd()) _current++;
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
            if (Previous()._type == TokenType.SEMICOLON) return;
            switch (Peek()._type) {
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
            Advance();
        }
    }

    ParseException Error(Token token, string message) {
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