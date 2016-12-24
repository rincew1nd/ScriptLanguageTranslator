/*
<Язык> = "Программа" <Заголовок> <Опер> ";" ... <Опер> "Конец программа"
<Заголовок> = <Метки> <Знак> ... <Знак>
<Знак> = <Перем> ! <Цел>
<Опер> = <Метка> ":" <Перем> "=" <Прав.часть>
<Прав.часть> = <Блок> <Знак> ... <Блок>
<Метка> = <Число>

<Знак> = <Знак1>!<Знак2>!<Знак3>
<Знак> = "+"!"-"!"*"!"/"
<ЗнакБуль> = "|"!"&"
<Буква> = "A"!"Б"!"В"!...!"Я"
<Число> = "0"!"1"!"2"!...!"9"
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using CodeTranslator.Exceptions;

namespace CodeTranslator
{
    public class Translator
    {
        private Dictionary<string, Regex> _regexs;
        private Dictionary<string, string> _errorsDic;

        private List<string> _marks;
        private List<string> _usedMarks;
        private Dictionary<string, double> _variables;

        private int _currentLine;

        public Translator()
        {
            _regexs = new Dictionary<string, Regex>()
            {
                {"Числа", new Regex(@"^\d+$")},
                {"Операторы", new Regex("[&|*/+-]") },

                {"БульевыЧисла", new Regex("[!01]+")},
                {"БульевыЗнаки", new Regex("[!&|]")},
                {"БулеваОперация", new Regex("^[)(!&|01]+$")},

                {"ЯзыкНачало", new Regex("^Программа$")},
                {"ЯзыкКонец", new Regex("^Конец программа$")},
                {"Заголовок", new Regex(@"^Метки( [\d]+)+$")},
                {"Операция", new Regex(@"^([\d]+) : ([\dа-яА-Я]+) = ([\d()!&|+*/ -]+);$")},
                {"ОперацияЛеваяЧасть", new Regex(@"^([\d]+) : ([\dа-яА-Я]+) = ")},
                {"ПраваяЧасть", new Regex(@"^([\d()!&|+*/,-]+)([&|+*/-])([\d()!&|+*/,-]+)$")},
                {"ПраваяЧастьПростая", new Regex(@"^(-?\d+,?\d*?)(([+*/-])(-?\d+,?\d*?))+$")},
                {"ПраваяЧастьБуль", new Regex(@"^(!?[0-1]+)(([|&])(!?[0-1]+))+$")},
                
                {")Первая", new Regex(@"^[^(.]+\)")}
            };

            const string exceptionBase = "Ошибка в строке {0}.";
            const string kostil = "'{1}'";
            _errorsDic = new Dictionary<string, string>()
            {
                {"^Язык", $"{exceptionBase} Неверное ключевое слово {kostil}. Ожидается: Программа"},
                {"Язык$", $"{exceptionBase} Неверное ключевое слово {kostil}. Ожидается: Конец программа"},
                {"Заголовок", $"{exceptionBase} Неверное ключевое слово {kostil}." +
                              $" Ожидается: Метки <Знак> ... <Знак>"},
                {"ДубльМарка", $"{exceptionBase} Попытка добавить одинаковую марку 2 раза {kostil}"},
                {"Операция", $"{exceptionBase} Неверное ключевое слово {kostil}." +
                             $" Ожидается: <Метка> \":\" <Перем> \"=\" <Прав.часть>;"},
                {"НетОперация", $"{exceptionBase} Нет ни одного блока <Операция>"},
                {"НеизвестнаяМарка", $"{exceptionBase} Марка {kostil} не была объявлена!"},
                {"ПовторяющаясяМарка", $"{exceptionBase} Марка {kostil} была использована более двух раз!"},
                {"ПовторяющаясяПеременная", $"{exceptionBase} Такая переменная уже была объявлена ранее\n{kostil}"},
                {"ПлохаяПраваяЧасть", $"{exceptionBase} Правая часть содержит недопустимые символы\n{kostil}"},
                {"НепарнаяСкобка", $"{exceptionBase} Правая часть с непарными скобками\n{kostil}"},
                {"БульНеБуль", $"{exceptionBase} Правая часть содержит обычные и булевы операции\n{kostil}"},
                {"Цифра(", $"{exceptionBase} Цифра не может идти после открывающей скобки." +
                           $" Нужен соединяющий оператор\n{kostil}" },
                {")(", $"{exceptionBase} Закрывающая скобка не может идти после открывающей скобки." +
                       $" Нужен соединяющий оператор\n{kostil}" },
                {"-(", $"{exceptionBase} Отрицательня скобка в строке\n{kostil}" },
                {"()", $"{exceptionBase} Скобки ничего не содержат\n{kostil}" },
                {"Знак)", $"{exceptionBase} Знак не может соединять закрывающую скобку\n{kostil}" },
                {"^)", $"{exceptionBase} Закрывающая скобка как начало правой части\n{kostil}" },
                {")Первая", $"{exceptionBase} Закрывающая скобка без открывающей скобки\n{kostil}" },
                {"ПорядокПравойЧасти", $"{exceptionBase} Неверный порядок чисел и связующих операторов\n{kostil}" },
            };

            _variables = new Dictionary<string, double>();
            _marks = new List<string>();
            _usedMarks = new List<string>();
            _currentLine = 1;
        }

        public void CheckSyntax(string text)
        {
            _variables.Clear();
            _marks.Clear();
            _usedMarks.Clear();
            _currentLine = 1;

            var codeLines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            if (!_regexs["ЯзыкНачало"].IsMatch(codeLines[0]))
                throw new SyntaxException(_errorsDic["^Язык"], 1, codeLines[0]);

            if (!_regexs["Заголовок"].IsMatch(codeLines[1]))
                throw new SyntaxException(_errorsDic["Заголовок"], 2, codeLines[1]);

            foreach (var markCapture in _regexs["Заголовок"].Match(codeLines[1]).Groups[1].Captures)
            {
                var mark = markCapture.ToString().TrimStart(' ');
                if (_marks.Contains(mark))
                    throw new SyntaxException(_errorsDic["ДубльМарка"], 2, mark);
                _marks.Add(mark);
            }

            _currentLine = 2;
            while (_regexs["ОперацияЛеваяЧасть"].IsMatch(codeLines[_currentLine]))
            {
                var groups = _regexs["Операция"].Match(codeLines[_currentLine]).Groups;

                if (groups[0].Value == "")
                {
                    var rightPart = _regexs["ОперацияЛеваяЧасть"].Match(codeLines[_currentLine]).Value;
                    var wrongPart = codeLines[_currentLine].Replace(rightPart, "");
                    throw new SyntaxException(_errorsDic["ПлохаяПраваяЧасть"], _currentLine + 1, wrongPart);
                }

                if (!_marks.Contains(groups[1].Value))
                    throw new SyntaxException(_errorsDic["НеизвестнаяМарка"], _currentLine + 1, groups[1].Value);

                if (_usedMarks.Contains(groups[1].Value))
                    throw new SyntaxException(_errorsDic["ПовторяющаясяМарка"], _currentLine + 1, groups[1].Value);

                _usedMarks.Add(groups[1].Value);

                if (_variables.ContainsKey(groups[2].Value))
                    throw new SyntaxException(_errorsDic["ПовторяющаясяПеременная"], _currentLine + 1, groups[2].Value);

                if (_regexs["ПраваяЧасть"].IsMatch(groups[3].Value))
                    _variables.Add(groups[2].Value, double.Parse(ParseOperation(groups[3].Value)));
                else
                    throw new SyntaxException(_errorsDic["ПраваяЧасть"], _currentLine + 1);

                _currentLine++;
            }
            if (_currentLine == 2)
            {
                if (_regexs["ЯзыкКонец"].IsMatch(codeLines[_currentLine]))
                    throw new SyntaxException(_errorsDic["НетОперация"], 3, codeLines[2]);
                else
                    throw new SyntaxException(_errorsDic["Операция"], 3, codeLines[2]);
            }

            if (!_regexs["ЯзыкКонец"].IsMatch(codeLines[_currentLine]))
                throw new SyntaxException(_errorsDic["Язык$"], _currentLine + 1, codeLines[_currentLine]);
        }

        public Dictionary<string, double> GetResult()
        {
            if (_variables.Count != 0)
                return _variables;
            throw new Exception("Null");
        }

        private string ParseOperation(string text)
        {
            CheckBrackets(text);

            while (text.Contains('('))
            {
                var substring = new StringBuilder();

                var lastBracket = -1;
                var lastSuccessfulBracket = -1;
                for (var i = 0; i < text.Length; i++)
                {
                    if (text[i] == '(')
                        lastBracket = i;

                    if (text[i] == ')' && lastBracket != -1)
                    {
                        substring.Append(
                            text.Substring(
                                lastSuccessfulBracket + 1, lastBracket - lastSuccessfulBracket - 1));
                        substring.Append(
                            PerformCalculation(
                                text.Substring(lastBracket + 1, i - lastBracket - 1)));
                        lastSuccessfulBracket = i;
                        lastBracket = -1;
                    }

                    if (i != text.Length - 1) continue;

                    substring.Append(
                        text.Substring(
                            lastSuccessfulBracket + 1, text.Length - lastSuccessfulBracket - 1));
                }

                text = substring.ToString();
            }

            return PerformCalculation(text);
        }

        private void CheckBrackets(string text)
        {
            var brackets = new[] { '(', ')' };
            var operators = new[] { '&', '|', '*', '/', '+', '-' };
            var operationType = _regexs["БульевыЗнаки"].IsMatch(text) ?
                "ПраваяЧастьБуль" : "ПраваяЧастьПростая";

            if (text.Count(c => c == ')') != text.Count(c => c == '('))
                throw new OredrException(_errorsDic["НепарнаяСкобка"], _currentLine + 1, text);

            if (operationType == "ПраваяЧастьБуль")
                if (!_regexs["БулеваОперация"].IsMatch(text))
                    throw new OredrException(_errorsDic["БульНеБуль"], _currentLine + 1, text);

            if (_regexs[")Первая"].IsMatch(text))
                throw new OredrException(_errorsDic[")Первая"], _currentLine + 1, text);

            var i = 0;
            var lastSymbol = "";
            var secondOperatorInaRow = false;
            var openBracketCount = 0;
            var lastBracketContent = new StringBuilder();
            foreach (var chr in text)
            {
                i++;
                if (brackets.Contains(chr) || operators.Contains(chr))
                {
                    if (chr == '(')
                    {
                        openBracketCount++;
                        if (lastBracketContent.Length != 0)
                        {
                            if (lastSymbol == "-" && (
                                secondOperatorInaRow || (
                                    i-3 >= 0 && text[i-3] == '(')
                                ))
                                throw new OredrException(_errorsDic["-("], _currentLine + 1, text.Substring(0, i));

                            if (lastSymbol == "(" || _regexs["Операторы"].IsMatch(lastSymbol))
                            {
                                lastSymbol = chr.ToString();
                                continue;
                            }
                            
                            if (_regexs["Числа"].IsMatch(lastSymbol))
                                throw new OredrException(_errorsDic["Цифра("], _currentLine + 1, text.Substring(0, i));
                            if (lastSymbol == ")")
                                throw new OredrException(_errorsDic[")("], _currentLine + 1, text.Substring(0, i));
                        }
                        lastSymbol = chr.ToString();
                    }
                    else if (chr == ')')
                    {
                        if (openBracketCount == 0 && i == 1)
                            throw new OredrException(_errorsDic["^)"], _currentLine + 1, text);

                        openBracketCount--;
                        if (lastBracketContent.Length != 0)
                        {
                            if (lastSymbol == ")" || _regexs["Числа"].IsMatch(lastSymbol))
                                if (_regexs[operationType].IsMatch(lastBracketContent.ToString()))
                                {
                                    lastSymbol = ")";
                                    continue;
                                }
                                else
                                    throw new OredrException(_errorsDic["ПорядокПравойЧасти"],
                                        _currentLine + 1, lastBracketContent);

                            if (lastSymbol == "(")
                                throw new OredrException(_errorsDic["()"], _currentLine + 1, text.Substring(0, i));
                            if (_regexs["Операторы"].IsMatch(lastSymbol))
                                throw new OredrException(_errorsDic["Знак)"], _currentLine + 1, text.Substring(0, i));
                        }
                        else
                            throw new OredrException(_errorsDic["^)"], _currentLine + 1, text);

                        lastSymbol = chr.ToString();
                    } else if (operators.Contains(chr))
                    {
                        if (operators.Contains(lastSymbol[0]))
                            secondOperatorInaRow = true;
                        lastBracketContent.Append(chr);
                        lastSymbol = chr.ToString();
                        continue;
                    }
                }
                else
                {
                    lastSymbol = chr.ToString();
                    lastBracketContent.Append(chr);
                }
                secondOperatorInaRow = false;
            }

            if (!_regexs[operationType].IsMatch(lastBracketContent.ToString()))
                throw new OredrException(_errorsDic["ПорядокПравойЧасти"], _currentLine + 1, lastBracketContent);
        }

        private string PerformCalculation(string operation)
        {
            var operType1 = new[] { "/", "*" };
            var operType2 = new[] { "-", "+" };

            var opers = OperatorsArray(operation);
            while (opers.Length != 1)
            {
                if (opers.Count(c => operType1.Contains(c)) > 0)
                {
                    RecurciveReplacing(ref opers, operType1);
                }
                else if (opers.Count(c => operType2.Contains(c)) > 0)
                {
                    RecurciveReplacing(ref opers, operType2);
                }
                else if (opers.Count(c => c.StartsWith("!")) > 0)
                {
                    ReplaceBoolInversion(ref opers);
                }
                else if (opers.Contains("&"))
                {
                    RecurciveReplacing(ref opers, new[] { "&" });
                }
                else if (opers.Contains("|"))
                {
                    RecurciveReplacing(ref opers, new[] { "|" });
                }
                else if (opers.Length == 1 && opers[0] == "")
                {
                    opers[0] = operation;
                }
            }

            if (opers.Length == 1 && opers[0] == "")
                return operation;
            else
                return opers[0];
        }

        private string[] OperatorsArray(string operation)
        {
            var operationType = _regexs["БульевыЗнаки"].IsMatch(operation) ?
                "ПраваяЧастьБуль" : "ПраваяЧастьПростая";

            var opersRegex = _regexs[operationType].Match(operation);

            var opers = new string[1];
            opers[0] = opersRegex.Groups[1].Value;
            for (var i = 1; i <= opersRegex.Groups[4].Captures.Count; i++)
            {
                Array.Resize(ref opers, opers.Length + 2);
                opers[i * 2 - 1] = opersRegex.Groups[3].Captures[i - 1].Value;
                opers[i * 2] = opersRegex.Groups[4].Captures[i - 1].Value;
            }

            return opers;
        }

        private void RecurciveReplacing(ref string[] opers, string[] oper)
        {
            for (var i = 0; i < opers.Length; i++)
            {
                if (!oper.Contains(opers[i])) continue;

                var opersCopy = new string[opers.Length - 2];
                if (i != 1) Array.Copy(opers, opersCopy, i - 1);
                opersCopy[i - 1] = Calculate(opers[i - 1], opers[i], opers[i + 1]).ToString();
                if (opers.Length != i + 2)
                    Array.Copy(opers, i + 2, opersCopy, i, opers.Length - i - 2);
                opers = opersCopy;
                break;
            }
        }

        public void ReplaceBoolInversion(ref string[] opers)
        {
            for (var i = 0; i < opers.Length; i++)
                if (opers[i].StartsWith("!"))
                    opers[i] = opers[i] == "!1" ? "0" : "1";
        }

        private double Calculate(string leftSide, string oper, string rightSide)
        {
            if (new[] { "&", "|", "!" }.Contains(oper) &&
                !_regexs["БульевыЧисла"].IsMatch(leftSide) &&
                !_regexs["БульевыЧисла"].IsMatch(rightSide))
                throw new Exception("Булевые операции доступны только для булевых значений");

            if (leftSide.StartsWith("!"))
                leftSide = leftSide == "!1" ? "0" : "1";
            if (rightSide.StartsWith("!"))
                rightSide = rightSide == "!1" ? "0" : "1";

            switch (oper)
            {
                case "&":
                    return ((leftSide == "1" ? true : false) &&
                            (rightSide == "1" ? true : false))
                            ? 1 : 0;
                case "|":
                    return ((leftSide == "1" ? true : false) ||
                            (rightSide == "1" ? true : false))
                            ? 1 : 0;
                case "+":
                    return double.Parse(leftSide) + double.Parse(rightSide);
                case "-":
                    return double.Parse(leftSide) - double.Parse(rightSide);
                case "*":
                    return double.Parse(leftSide) * double.Parse(rightSide);
                case "/":
                    return double.Parse(leftSide) / double.Parse(rightSide);
            }
            return 0;
        }
    }
}
