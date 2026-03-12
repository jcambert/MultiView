using System.Globalization;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using MultiView.DynamicViews.Core.Abstractions;

namespace MultiView.DynamicViews.Core.RuleEvaluator;

public sealed class DefaultRuleEvaluator : IRuleEvaluator
{
    private readonly ConcurrentDictionary<string, Func<IRuleValueAccessor, bool>> _cache = new();

    public bool Evaluate(string? expression, IRuleValueAccessor context)
    {
        if (string.IsNullOrWhiteSpace(expression))
        {
            return false;
        }

        Func<IRuleValueAccessor, bool> evaluator = Compile(expression);
        return evaluator(context);
    }

    public Func<IRuleValueAccessor, bool> Compile(string expression)
    {
        return _cache.GetOrAdd(expression, CompileInternal);
    }

    private static Func<IRuleValueAccessor, bool> CompileInternal(string expression)
    {
        RuleNode root = new RuleExpressionParser(expression).Parse();
        return CompileBoolean(root);
    }

    private static Func<IRuleValueAccessor, bool> CompileBoolean(RuleNode node)
    {
        return node switch
        {
            ConstantRuleNode constant => constant.Value is bool boolValue ? (_ => boolValue) : (_ => constant.Value is not null),
            IdentifierRuleNode identifier => ctx => ToBoolean(ctx.TryGetValue(identifier.Path, out object? value) ? value : null),
            UnaryRuleNode unary when unary.Operator is "!" or "not" => ctx => !CompileBoolean(unary.Operand)(ctx),
            BinaryRuleNode binary when IsLogical(binary.Operator) =>
                BuildLogical(binary.Operator, CompileBoolean(binary.Left), CompileBoolean(binary.Right)),
            BinaryRuleNode binary => ctx => Compare(EvaluateObject(binary.Left, ctx), binary.Operator, EvaluateObject(binary.Right, ctx)),
            _ => throw new NotSupportedException("Noeud de règle non pris en charge.")
        };
    }

    private static bool IsLogical(string op)
    {
        return op.Equals("&&", StringComparison.Ordinal) || op.Equals("and", StringComparison.OrdinalIgnoreCase)
            || op.Equals("||", StringComparison.Ordinal) || op.Equals("or", StringComparison.OrdinalIgnoreCase);
    }

    private static Func<IRuleValueAccessor, bool> BuildLogical(string op, Func<IRuleValueAccessor, bool> left,
        Func<IRuleValueAccessor, bool> right)
    {
        return op is "&&" or "and"
            ? ctx => left(ctx) && right(ctx)
            : ctx => left(ctx) || right(ctx);
    }

    private static object? EvaluateObject(RuleNode node, IRuleValueAccessor context)
    {
        return node switch
        {
            ConstantRuleNode constant => constant.Value,
            IdentifierRuleNode identifier => context.TryGetValue(identifier.Path, out object? value) ? value : null,
            UnaryRuleNode unary when unary.Operator is "!" or "not" => !CompileBoolean(unary.Operand)(context),
            UnaryRuleNode unary => unary.Operand is not null ? EvaluateObject(unary.Operand, context) : null,
            _ => throw new NotSupportedException("Impossible d'évaluer ce noeud en objet.")
        };
    }

    private static bool Compare(object? left, string op, object? right)
    {
        return op switch
        {
            "==" => EqualsNormalized(left, right),
            "!=" => !EqualsNormalized(left, right),
            ">=" => CompareComparable(left, right) >= 0,
            ">" => CompareComparable(left, right) > 0,
            "<=" => CompareComparable(left, right) <= 0,
            "<" => CompareComparable(left, right) < 0,
            _ => throw new NotSupportedException($"Opérateur {op} non supporté")
        };
    }

    private static bool EqualsNormalized(object? left, object? right)
    {
        if (left is null && right is null)
        {
            return true;
        }

        if (left is null || right is null)
        {
            return false;
        }

        if (TryConvertToDecimal(left, out decimal leftDecimal) && TryConvertToDecimal(right, out decimal rightDecimal))
        {
            return leftDecimal == rightDecimal;
        }

        if (left is bool leftBool && right is bool rightBool)
        {
            return leftBool == rightBool;
        }

        return string.Equals(Convert.ToString(left, CultureInfo.InvariantCulture),
            Convert.ToString(right, CultureInfo.InvariantCulture), StringComparison.InvariantCultureIgnoreCase);
    }

    private static int CompareComparable(object? left, object? right)
    {
        if (left is null && right is null)
        {
            return 0;
        }

        if (left is null)
        {
            return -1;
        }

        if (right is null)
        {
            return 1;
        }

        if (TryConvertToDecimal(left, out decimal leftDecimal) && TryConvertToDecimal(right, out decimal rightDecimal))
        {
            return leftDecimal.CompareTo(rightDecimal);
        }

        if (DateTime.TryParse(Convert.ToString(left, CultureInfo.InvariantCulture), CultureInfo.InvariantCulture,
                DateTimeStyles.None, out DateTime leftDate)
            && DateTime.TryParse(Convert.ToString(right, CultureInfo.InvariantCulture), CultureInfo.InvariantCulture,
                DateTimeStyles.None, out DateTime rightDate))
        {
            return leftDate.CompareTo(rightDate);
        }

        string leftString = Convert.ToString(left, CultureInfo.InvariantCulture) ?? string.Empty;
        string rightString = Convert.ToString(right, CultureInfo.InvariantCulture) ?? string.Empty;
        return string.Compare(leftString, rightString, StringComparison.InvariantCultureIgnoreCase);
    }

    private static bool ToBoolean(object? value)
    {
        return value switch
        {
            null => false,
            bool booleanValue => booleanValue,
            decimal decimalValue => decimalValue != 0m,
            double doubleValue => doubleValue != 0,
            int intValue => intValue != 0,
            long longValue => longValue != 0L,
            string stringValue => bool.TryParse(stringValue, out bool parsed) && parsed,
            _ => !string.IsNullOrWhiteSpace(Convert.ToString(value, CultureInfo.InvariantCulture))
        };
    }

    private static bool TryConvertToDecimal(object value, out decimal result)
    {
        switch (value)
        {
            case decimal decimalValue:
                result = decimalValue;
                return true;
            case double doubleValue:
                result = Convert.ToDecimal(doubleValue);
                return true;
            case float floatValue:
                result = Convert.ToDecimal(floatValue);
                return true;
            case int intValue:
                result = intValue;
                return true;
            case long longValue:
                result = longValue;
                return true;
            case string stringValue when decimal.TryParse(stringValue, NumberStyles.Any, CultureInfo.InvariantCulture, out result):
                return true;
            default:
                result = 0m;
                return false;
        }
    }
}

internal sealed class RuleExpressionParser
{
    private readonly RuleTokenReader _reader;

    public RuleExpressionParser(string expression)
    {
        _reader = new RuleTokenReader(expression);
    }

    public RuleNode Parse()
    {
        RuleNode expression = ParseOrExpression();
        if (_reader.Current.Type != RuleTokenType.End)
        {
            throw new InvalidOperationException($"Token inattendu: {_reader.Current.Raw}");
        }

        return expression;
    }

    private RuleNode ParseOrExpression()
    {
        RuleNode left = ParseAndExpression();
        while (_reader.Current.IsOperator("or") || _reader.Current.IsOperator("||"))
        {
            string op = _reader.Current.Raw;
            _reader.Advance();
            RuleNode right = ParseAndExpression();
            left = new BinaryRuleNode(left, op, right);
        }

        return left;
    }

    private RuleNode ParseAndExpression()
    {
        RuleNode left = ParseComparisonExpression();
        while (_reader.Current.IsOperator("and") || _reader.Current.IsOperator("&&"))
        {
            string op = _reader.Current.Raw;
            _reader.Advance();
            RuleNode right = ParseComparisonExpression();
            left = new BinaryRuleNode(left, op, right);
        }

        return left;
    }

    private RuleNode ParseComparisonExpression()
    {
        RuleNode left = ParseUnaryOrPrimary();
        while (_reader.Current.IsOperator("==") || _reader.Current.IsOperator("!=") ||
               _reader.Current.IsOperator(">") || _reader.Current.IsOperator(">=") ||
               _reader.Current.IsOperator("<") || _reader.Current.IsOperator("<="))
        {
            string op = _reader.Current.Raw;
            _reader.Advance();
            RuleNode right = ParseUnaryOrPrimary();
            left = new BinaryRuleNode(left, op, right);
        }

        return left;
    }

    private RuleNode ParseUnaryOrPrimary()
    {
        if (_reader.Current.Type == RuleTokenType.Operator && (_reader.Current.IsOperator("!") || _reader.Current.IsOperator("not")))
        {
            string op = _reader.Current.Raw;
            _reader.Advance();
            return new UnaryRuleNode(op, ParseUnaryOrPrimary());
        }

        if (_reader.Current.Type == RuleTokenType.LeftParen)
        {
            _reader.Advance();
            RuleNode inside = ParseOrExpression();
            _reader.Expect(RuleTokenType.RightParen, ")");
            return inside;
        }

        return ParseLiteralOrIdentifier();
    }

    private RuleNode ParseLiteralOrIdentifier()
    {
        RuleToken token = _reader.Current;
        _reader.Advance();

        return token.Type switch
        {
            RuleTokenType.Identifier => new IdentifierRuleNode(token.Raw),
            RuleTokenType.String => new ConstantRuleNode(token.Value),
            RuleTokenType.Number => new ConstantRuleNode(token.Value),
            RuleTokenType.Boolean => new ConstantRuleNode(token.Value),
            RuleTokenType.Null => new ConstantRuleNode(null),
            _ => throw new InvalidOperationException($"Token inattendu: {token.Raw}")
        };
    }
}

internal sealed record RuleToken
{
    public RuleToken(RuleTokenType type, string raw, object? value = null)
    {
        Type = type;
        Raw = raw;
        Value = value;
    }

    public RuleTokenType Type { get; }
    public string Raw { get; }
    public object? Value { get; }

    public bool IsOperator(string expected)
    {
        return Type == RuleTokenType.Operator && string.Equals(Raw, expected, StringComparison.OrdinalIgnoreCase);
    }
}

enum RuleTokenType
{
    End,
    Identifier,
    String,
    Number,
    Boolean,
    Null,
    Operator,
    LeftParen,
    RightParen
}

internal sealed class RuleTokenReader
{
    private readonly List<RuleToken> _tokens;
    private int _index;

    public RuleTokenReader(string expression)
    {
        _tokens = Tokenize(expression);
        _index = 0;
    }

    public RuleToken Current => _tokens[_index];

    public void Advance()
    {
        if (_index < _tokens.Count - 1)
        {
            _index++;
        }
    }

    public void Expect(RuleTokenType type, string expected)
    {
        if (Current.Type != type)
        {
            throw new InvalidOperationException($"Token '{expected}' attendu.");
        }

        Advance();
    }

    private static List<RuleToken> Tokenize(string expression)
    {
        var tokens = new List<RuleToken>();
        int index = 0;

        while (index < expression.Length)
        {
            char current = expression[index];
            if (char.IsWhiteSpace(current))
            {
                index++;
                continue;
            }

            if (current == '(')
            {
                tokens.Add(new RuleToken(RuleTokenType.LeftParen, "("));
                index++;
                continue;
            }

            if (current == ')')
            {
                tokens.Add(new RuleToken(RuleTokenType.RightParen, ")"));
                index++;
                continue;
            }

            if (current == '\'' || current == '"')
            {
                var (value, next) = ReadQuoted(expression, index);
                tokens.Add(new RuleToken(RuleTokenType.String, value, value));
                index = next;
                continue;
            }

            if (TryReadTwoCharOperator(expression, index, out string twoCharOperator))
            {
                tokens.Add(new RuleToken(RuleTokenType.Operator, twoCharOperator));
                index += 2;
                continue;
            }

            if (current is '!' or '>' or '<')
            {
                tokens.Add(new RuleToken(RuleTokenType.Operator, current.ToString()));
                index++;
                continue;
            }

            if (char.IsDigit(current) || current == '-' || current == '.')
            {
                var (numberToken, next) = ReadNumber(expression, index);
                tokens.Add(new RuleToken(RuleTokenType.Number, numberToken, decimal.Parse(numberToken, CultureInfo.InvariantCulture)));
                index = next;
                continue;
            }

            if (char.IsLetter(current) || current == '_')
            {
                var (word, next) = ReadWord(expression, index);
                if (word.Equals("true", StringComparison.OrdinalIgnoreCase) || word.Equals("false", StringComparison.OrdinalIgnoreCase))
                {
                    tokens.Add(new RuleToken(RuleTokenType.Boolean, word, bool.Parse(word)));
                }
                else if (word.Equals("null", StringComparison.OrdinalIgnoreCase))
                {
                    tokens.Add(new RuleToken(RuleTokenType.Null, word));
                }
                else if (IsLogicalWord(word))
                {
                    tokens.Add(new RuleToken(RuleTokenType.Operator, word));
                }
                else
                {
                    tokens.Add(new RuleToken(RuleTokenType.Identifier, word));
                }

                index = next;
                continue;
            }

            throw new InvalidOperationException($"Caractère invalide: '{current}'.");
        }

        tokens.Add(new RuleToken(RuleTokenType.End, string.Empty));
        return tokens;
    }

    private static bool IsLogicalWord(string token)
    {
        return token.Equals("and", StringComparison.OrdinalIgnoreCase)
            || token.Equals("or", StringComparison.OrdinalIgnoreCase)
            || token.Equals("not", StringComparison.OrdinalIgnoreCase);
    }

    private static (string token, int nextIndex) ReadWord(string text, int start)
    {
        int index = start;
        while (index < text.Length)
        {
            char current = text[index];
            if (char.IsLetterOrDigit(current) || current == '_' || current == '.')
            {
                index++;
                continue;
            }

            break;
        }

        return (text.Substring(start, index - start), index);
    }

    private static bool TryReadTwoCharOperator(string text, int index, out string op)
    {
        if (index + 1 >= text.Length)
        {
            op = string.Empty;
            return false;
        }

        string candidate = text.Substring(index, 2);
        if (candidate is "==" or "!=" or ">=" or "<=" or "&&" or "||")
        {
            op = candidate;
            return true;
        }

        op = string.Empty;
        return false;
    }

    private static (string token, int nextIndex) ReadQuoted(string text, int start)
    {
        char delimiter = text[start];
        int index = start + 1;
        bool escaped = false;
        StringBuilder builder = new();

        while (index < text.Length)
        {
            char current = text[index];
            if (escaped)
            {
                builder.Append(current);
                escaped = false;
                index++;
                continue;
            }

            if (current == '\\')
            {
                escaped = true;
                index++;
                continue;
            }

            if (current == delimiter)
            {
                index++;
                return (builder.ToString(), index);
            }

            builder.Append(current);
            index++;
        }

        throw new InvalidOperationException("Chaîne non terminée.");
    }

    private static (string token, int nextIndex) ReadNumber(string text, int start)
    {
        int index = start;
        bool hasDot = false;

        while (index < text.Length)
        {
            char current = text[index];
            if (char.IsDigit(current))
            {
                index++;
                continue;
            }

            if (current == '.')
            {
                if (hasDot)
                {
                    throw new InvalidOperationException("Nombre invalide.");
                }

                hasDot = true;
                index++;
                continue;
            }

            break;
        }

        return (text.Substring(start, index - start), index);
    }
}

internal abstract record RuleNode;

internal sealed record ConstantRuleNode(object? Value) : RuleNode;

internal sealed record IdentifierRuleNode(string Path) : RuleNode;

internal sealed record UnaryRuleNode(string Operator, RuleNode Operand) : RuleNode;

internal sealed record BinaryRuleNode(RuleNode Left, string Operator, RuleNode Right) : RuleNode;
