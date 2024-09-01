namespace Interpreter;

public class Scanner {
    static readonly Dictionary<string, TokenType> keywords =
    new Dictionary<string, TokenType> {
        {"and", TokenType.AND},
        {"class", TokenType.CLASS},
        {"else", TokenType.ELSE},
        {"false", TokenType.FALSE},
        {"for", TokenType.FOR},
        {"fun", TokenType.FUN},
        {"if", TokenType.IF},
        {"nil", TokenType.NIL},
        {"or", TokenType.OR},
        {"print", TokenType.PRINT},
        {"return", TokenType.RETURN},
        {"super", TokenType.SUPER},
        {"this", TokenType.THIS},
        {"true", TokenType.TRUE},
        {"var", TokenType.VAR},
        {"while", TokenType.WHILE},
    };
    readonly string _source;
    readonly List<Token> _tokens = new List<Token>();
    int _start = 0;
    int _current = 0;
    int _line = 1;

    public Scanner(string source) {
        _source = source;
    }

    public List<Token> scanTokens() {
        while (!isAtEnd()) {
            _start = _current;
            scanToken();
        }
        _tokens.Add(new Token(TokenType.EOF, "", null, _line));
        return _tokens;
    }

    bool isAtEnd() {
        return _current >= _source.Count();
    }

    void scanToken() {
        char c = advance();
        switch (c) {
            case ' ':
            case '\r':
            case '\t': break;
            case '\n':
                _line++;
                break;
            case '(': addToken(TokenType.LEFT_PAREN); break;
            case ')': addToken(TokenType.RIGHT_PAREN); break;
            case '{': addToken(TokenType.LEFT_BRACE); break;
            case '}': addToken(TokenType.RIGHT_BRACE); break;
            case ',': addToken(TokenType.COMMA); break;
            case '.': addToken(TokenType.DOT); break;
            case '-': addToken(TokenType.MINUS); break;
            case '+': addToken(TokenType.PLUS); break;
            case ';': addToken(TokenType.SEMICOLON); break;
            case '*': addToken(TokenType.STAR); break;
            case '!': addToken(match('=') ? TokenType.BANG_EQUAL : TokenType.BANG); break;
            case '=': addToken(match('=') ? TokenType.EQUAL_EQUAL : TokenType.EQUAL); break;
            case '<': addToken(match('=') ? TokenType.LESS_EQUAL : TokenType.LESS); break;
            case '>': addToken(match('=') ? TokenType.GREATER_EQUAL : TokenType.GREATER); break;
            case '/':
                if (match('/')) {
                    while (peek() != '\n' && !isAtEnd()) advance();
                } else {
                    addToken(TokenType.SLASH);
                }
                break;
            case '"': add_string(); break;
            case char o when isDigit(o): add_number(); break;
            case char o when isAlpha(o): add_identifier(); break;
            default: Machine.error(_line, $"Unexpected character: {c}\n"); break;
        }
    }

    bool isAlpha(char c) {
        return (c >= 'a' && c <= 'z') ||
               (c >= 'A' && c <= 'Z') ||
               c == '_';
    }

    bool isDigit(char c) {
        return '0' <= c && c <= '9';
    }

    bool isAlphaDigit(char c) {
        return isAlpha(c) || isDigit(c);
    }

    void add_identifier() {
        while (isAlphaDigit(peek())) advance();
        var text = _source.Substring(_start, _current - _start);
        TokenType value;
        if (Scanner.keywords.TryGetValue(text, out value)) {
            addToken(value);
        } else {
            addToken(TokenType.IDENTIFIER);
        }
    }

    void add_number() {
        while (isDigit(peek())) advance();
        if (peek() == '.' && isDigit(peekNext())) {
            advance();
            while (isDigit(peek())) advance();
        }
        var value = float.Parse(_source.Substring(_start, _current - _start));
        addToken(TokenType.NUMBER, value);
    }

    void add_string() {
        while (peek() != '"' && !isAtEnd()) {
            if (peek() == '\n') _line++;
            advance();
        }
        if (isAtEnd()) {
            Machine.error(_line, "Unterminated string\n");
            return;
        }
        advance();
        var value = _source.Substring(_start + 1, _current - _start - 2);
        addToken(TokenType.STRING, value);
    }

    bool match(char expected) {
        bool is_not_match = isAtEnd() || (_source[_current] != expected);
        if (is_not_match) return false;
        _current++;
        return true;
    }

    char peek() {
        if (isAtEnd()) return '\0';
        return _source[_current];
    }

    char peekNext() {
        if (_current + 1 >= _source.Count()) return '\0';
        return _source[_current + 1];
    }

    char advance() {
        return _source[_current++];
    }

    void addToken(TokenType type) {
        addToken(type, null);
    }

    void addToken(TokenType type, Object? literal) {
        var text = _source.Substring(_start, _current - _start);
        _tokens.Add(new Token(type, text, literal, _line));
    }
}


public class Token {
    public readonly TokenType _type;
    public readonly string _lexeme;
    public readonly Object? _literal;
    public readonly int _line;

    public Token(TokenType type, string lexeme, Object? literal, int line) {
        _type = type;
        _lexeme = lexeme;
        _literal = literal;
        _line = line;
    }

    public string toString() {
        return $"{_type} {_lexeme} {_literal}";
    }
}


public enum TokenType {
    // Single-character tokens.
    LEFT_PAREN, RIGHT_PAREN, LEFT_BRACE, RIGHT_BRACE,
    COMMA, DOT, MINUS, PLUS, SEMICOLON, SLASH, STAR,
    // One or two character tokens.
    BANG, BANG_EQUAL,
    EQUAL, EQUAL_EQUAL,
    GREATER, GREATER_EQUAL,
    LESS, LESS_EQUAL,
    // Literals.
    IDENTIFIER, STRING, NUMBER,
    // Keywords.
    AND, CLASS, ELSE, FALSE, FUN, FOR, IF, NIL, OR,
    PRINT, RETURN, SUPER, THIS, TRUE, VAR, WHILE,
    EOF
}