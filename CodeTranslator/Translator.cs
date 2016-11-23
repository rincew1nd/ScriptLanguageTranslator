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
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CodeTranslator
{
    public class Translator
    {
        private Dictionary<string, Regex> _regexs;
        private Dictionary<string, string> _errorsDic;

        private List<string> _marks;
        private Dictionary<string, double> _variables;

        public Translator()
        {
            _regexs = new Dictionary<string, Regex>()
            {
                {"Числa", new Regex("^[0-9]+$")},
                {"БульевыЧислa", new Regex("^[!0-9]+$")},
                {"Буквы", new Regex("^[а-яА-Я]+$")},
                {"Переменная", new Regex($"^[0-9а-яА-Я]+$")},
                {"Скобки", new Regex(@"^\(.+\)$")},

                {"ЯзыкНачало", new Regex("^Программа$")},
                {"ЯзыкКонец", new Regex("^Конец программа$")},
                {"Заголовок", new Regex($"^Метки( [0-9]+)+$")},
                {"Операция", new Regex("^([0-9]+) : ([0-9а-яА-Я]+) = ([0-9()!&|+*/ -]+)$")},
                {"ПравЧасть", new Regex("^([0-9()!&|+*/ -]+) ([!&|+*/-]) ([0-9()!&|+*/ -]+)$")},
                {"ПравЧастьТочка", new Regex("^([0-9()!&|+*/, -]+) ([!&|+*/-]) ([0-9()!&|+*/, -]+)$")}
            };

            _errorsDic = new Dictionary<string, string>()
            {
                {"Язык", "^Программа(.+)Конец программа$"},
                {"Заголовок", $"Метки( [0-9]+)+"},
                {"Операция", "[0-9]+ : [0-9а-яА-Я]+ = test"},
                {"НетОперация", "Нет ни одного блока 'Операция'"},
                {"НеизвестнаяМарка", "Марка {0} не была обхявлена!"},
            };

            _variables = new Dictionary<string, double>();
            _marks = new List<string>();
        }

        public string CheckSyntax(string text)
        {
            _variables.Clear();
            _marks.Clear();

            var codeLines = text.Split(new[] {'\r','\n'}, StringSplitOptions.RemoveEmptyEntries);
            
            if (!_regexs["ЯзыкНачало"].IsMatch(codeLines[0]))
                return _errorsDic["Язык"];

            if (!_regexs["Заголовок"].IsMatch(codeLines[1]))
                return _errorsDic["Заголовок"];

            foreach (var markCapture in _regexs["Заголовок"].Match(codeLines[1]).Groups[1].Captures)
                _marks.Add(markCapture.ToString().TrimStart(' '));

            var codeLineNum = 2;
            while (_regexs["Операция"].IsMatch(codeLines[codeLineNum]))
            {
                var groups = _regexs["Операция"].Match(codeLines[codeLineNum]).Groups;

                if (!_marks.Contains(groups[1].Value))
                    return string.Format(_errorsDic["НеизвестнаяМарка"], groups[1].Value);

                if (_variables.ContainsKey(groups[2].Value))
                    return "Такая переменная уже была объявлена ранее!";

                if (_regexs["ПравЧастьТочка"].IsMatch(groups[3].Value))
                    _variables.Add(groups[2].Value, double.Parse(ParseOperation(groups[3].Value)));
                codeLineNum++;
            }
            if (codeLineNum == 2) return _errorsDic["НетОперация"];

            if (!_regexs["ЯзыкКонец"].IsMatch(codeLines[codeLineNum]))
                return _errorsDic["Язык"];

            return "";
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
                            lastSuccessfulBracket+1, text.Length-lastSuccessfulBracket-1));
                }

                text = substring.ToString();
            }

            return text;
        }

        private void CheckBrackets(string text)
        {
            if (text.Count(c => c == ')') != text.Count(c => c == '('))
                throw new ArgumentException("Есть непарные скобки");
        }

        private string PerformCalculation(string operation)
        {
            var opers = OperatorsArray(operation);
            while (opers.Length != 1)
            {
                if (opers.Contains("/") || opers.Contains("*"))
                {
                    RecurciveReplacing(ref opers, new[] { "*", "/" });
                }
                else
                {
                    RecurciveReplacing(ref opers, new [] {"+", "-"});
                }
            }
            return opers[0];
        }
        
        private string[] OperatorsArray(string operation)
        {
            var opersRegex = Regex.Match(operation, @"([0-9]+)(([+*/-])([0-9]+))+");

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

        private double Calculate(string leftSide, string oper, string rightSide)
        {
            if (new [] { "&", "|", "!" }.Contains(oper) &&
                !_regexs["БульевыЧислa"].IsMatch(leftSide) &&
                !_regexs["БульевыЧислa"].IsMatch(rightSide))
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
