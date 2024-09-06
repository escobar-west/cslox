namespace Lib;

public class Scanner {
    static readonly Dictionary<string, TokenType> _keywords =
    new() {
        {"and", TokenType.AND},
        {"class", TokenType.CLASS},
        {"else", TokenType.ELSE},
        {"false", TokenType.FALSE},
        {"for", TokenType.FOR},
        {"fun", TokenType.FUN},
        {"if", TokenType.IF},
        {"nil", TokenType.NIL},
        {"or", TokenType.OR},
        {"return", TokenType.RETURN},
        {"super", TokenType.SUPER},
        {"this", TokenType.THIS},
        {"true", TokenType.TRUE},
        {"var", TokenType.VAR},
        {"while", TokenType.WHILE},
    };
    readonly string _source;
    readonly List<Token> _tokens = [];
    int _start = 0;
    int _current = 0;
    int _line = 1;

    public Scanner(string source) {
        _source = source;
    }

    public List<Token> ScanTokens() {
        while (!IsAtEnd()) {
            _start = _current;
            ScanToken();
        }
        _tokens.Add(new Token(TokenType.EOF, "", null, _line));
        return _tokens;
    }

    bool IsAtEnd() {
        return _current >= _source.Count();
    }

    void ScanToken() {
        char c = Advance();
        switch (c) {
            case ' ':
            case '\r':
            case '\t': break;
            case '\n':
                _line++;
                break;
            case '(': AddToken(TokenType.LEFT_PAREN); break;
            case ')': AddToken(TokenType.RIGHT_PAREN); break;
            case '{': AddToken(TokenType.LEFT_BRACE); break;
            case '}': AddToken(TokenType.RIGHT_BRACE); break;
            case ',': AddToken(TokenType.COMMA); break;
            case '.': AddToken(TokenType.DOT); break;
            case '-': AddToken(TokenType.MINUS); break;
            case '+': AddToken(TokenType.PLUS); break;
            case ';': AddToken(TokenType.SEMICOLON); break;
            case '*': AddToken(TokenType.STAR); break;
            case '!': AddToken(Match('=') ? TokenType.BANG_EQUAL : TokenType.BANG); break;
            case '=': AddToken(Match('=') ? TokenType.EQUAL_EQUAL : TokenType.EQUAL); break;
            case '<': AddToken(Match('=') ? TokenType.LESS_EQUAL : TokenType.LESS); break;
            case '>': AddToken(Match('=') ? TokenType.GREATER_EQUAL : TokenType.GREATER); break;
            case '/':
                if (Match('/')) {
                    while (Peek() != '\n' && !IsAtEnd()) Advance();
                } else {
                    AddToken(TokenType.SLASH);
                }
                break;
            case '"': AddString(); break;
            case char o when IsDigit(o): AddNumber(); break;
            case char o when IsAlpha(o): AddIdentifier(); break;
            default: Machine.Error(_line, $"Unexpected character: {c}\n"); break;
        }
    }

    static bool IsAlpha(char c) {
        return (c >= 'a' && c <= 'z') ||
               (c >= 'A' && c <= 'Z') ||
               c == '_';
    }

    static bool IsDigit(char c) {
        return '0' <= c && c <= '9';
    }

    static bool IsAlphaDigit(char c) {
        return IsAlpha(c) || IsDigit(c);
    }

    void AddIdentifier() {
        while (IsAlphaDigit(Peek())) Advance();
        var text = _source.Substring(_start, _current - _start);
        if (_keywords.TryGetValue(text, out TokenType value)) {
            AddToken(value);
        } else {
            AddToken(TokenType.IDENTIFIER);
        }
    }

    void AddNumber() {
        while (IsDigit(Peek())) Advance();
        if (Peek() == '.' && IsDigit(PeekNext())) {
            Advance();
            while (IsDigit(Peek()))
                Advance();
        }
        var value = double.Parse(_source.Substring(_start, _current - _start));
        AddToken(TokenType.NUMBER, value);
    }

    void AddString() {
        while (Peek() != '"' && !IsAtEnd()) {
            if (Peek() == '\n')
                _line++;
            Advance();
        }
        if (IsAtEnd()) {
            Machine.Error(_line, "Unterminated string\n");
            return;
        }
        Advance();
        var value = _source.Substring(_start + 1, _current - _start - 2);
        AddToken(TokenType.STRING, value);
    }

    bool Match(char expected) {
        bool is_not_match = IsAtEnd() || (_source[_current] != expected);
        if (is_not_match)
            return false;
        _current++;
        return true;
    }

    char Peek() {
        if (IsAtEnd())
            return '\0';
        return _source[_current];
    }

    char PeekNext() {
        if (_current + 1 >= _source.Count())
            return '\0';
        return _source[_current + 1];
    }

    char Advance() {
        return _source[_current++];
    }

    void AddToken(TokenType type) {
        AddToken(type, null);
    }

    void AddToken(TokenType type, object? literal) {
        var text = _source.Substring(_start, _current - _start);
        _tokens.Add(new Token(type, text, literal, _line));
    }
}


public class Token {
    public readonly TokenType _type;
    public readonly string _lexeme;
    public readonly object? _literal;
    public readonly int _line;

    public Token(TokenType type, string lexeme, object? literal, int line) {
        _type = type;
        _lexeme = lexeme;
        _literal = literal;
        _line = line;
    }

    public override string ToString() {
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
    RETURN, SUPER, THIS, TRUE, VAR, WHILE,
    EOF
}