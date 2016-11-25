using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeTranslator;
using CodeTranslator.Exceptions;
using NUnit.Framework;

namespace UnitTests
{
    class ExceptionTest
    {
        #region SyntaxExceptions

        [Test]
        public void ProgramStartSyntax()
        {
            var translator = new Translator();
            try { translator.CheckSyntax("Программа1"); }
            catch (SyntaxException ex)
            {
                Assert.AreEqual(
                    "Ошибка в строке 1. Неверное ключевое слово 'Программа1'. Ожидается: Программа",
                    ex.Message);
            }
            try { translator.CheckSyntax("1Программа"); }
            catch (SyntaxException ex)
            {
                Assert.AreEqual(
                    "Ошибка в строке 1. Неверное ключевое слово '1Программа'. Ожидается: Программа",
                    ex.Message);
            }
            try { translator.CheckSyntax("программа"); }
            catch (SyntaxException ex)
            {
                Assert.AreEqual(
                    "Ошибка в строке 1. Неверное ключевое слово 'программа'. Ожидается: Программа",
                    ex.Message);
            }
        }

        [Test]
        public void HeaderSyntax()
        {
            var translator = new Translator();
            var text = "Программа\r\n";

            try { translator.CheckSyntax($"{text}Метка 12 13 14"); }
            catch (SyntaxException ex)
            {
                Assert.AreEqual(
                    "Ошибка в строке 2. Неверное ключевое слово 'Метка 12 13 14'. Ожидается: Метки <Знак> ... <Знак>",
                    ex.Message);
            }
            try { translator.CheckSyntax($"{text}метки 12 13 14"); }
            catch (SyntaxException ex)
            {
                Assert.AreEqual(
                    "Ошибка в строке 2. Неверное ключевое слово 'метки 12 13 14'. Ожидается: Метки <Знак> ... <Знак>",
                    ex.Message);
            }
            try { translator.CheckSyntax($"{text}Метки ф3 13 14"); }
            catch (SyntaxException ex)
            {
                Assert.AreEqual(
                    "Ошибка в строке 2. Неверное ключевое слово 'Метки ф3 13 14'. Ожидается: Метки <Знак> ... <Знак>",
                    ex.Message);
            }
        }

        [Test]
        public void NotUniqueMarkSyntax()
        {
            var translator = new Translator();
            var text = "Программа\r\n";

            try { translator.CheckSyntax($"{text}Метки 12 13 14 13"); }
            catch (SyntaxException ex)
            {
                Assert.AreEqual(
                    "Ошибка в строке 2. Попытка добавить одинаковую марку 2 раза '13'",
                    ex.Message);
            }
            try { translator.CheckSyntax($"{text}Метки 12 12"); }
            catch (SyntaxException ex)
            {
                Assert.AreEqual(
                    "Ошибка в строке 2. Попытка добавить одинаковую марку 2 раза '12'",
                    ex.Message);
            }
            try { translator.CheckSyntax($"{text}Метки 154 487 123 488 48 11 488"); }
            catch (SyntaxException ex)
            {
                Assert.AreEqual(
                    "Ошибка в строке 2. Попытка добавить одинаковую марку 2 раза '488'",
                    ex.Message);
            }
        }

        [Test]
        public void LeftSideOperationSyntax()
        {
            var translator = new Translator();
            var text = "Программа\r\nМетки 1 2 3\r\n";

            try { translator.CheckSyntax($"{text}10 :  тест = 123-123"); }
            catch (SyntaxException ex)
            {
                Assert.AreEqual(
                    "Ошибка в строке 3. Неверное ключевое слово '10 :  тест = 123-123'." +
                    " Ожидается: <Метка> \":\" <Перем> \"=\" <Прав.часть>",
                    ex.Message);
            }
            try { translator.CheckSyntax($"{text}а10 : тест = 123-123"); }
            catch (SyntaxException ex)
            {
                Assert.AreEqual(
                    "Ошибка в строке 3. Неверное ключевое слово 'а10 : тест = 123-123'." +
                    " Ожидается: <Метка> \":\" <Перем> \"=\" <Прав.часть>",
                    ex.Message);
            }
            try { translator.CheckSyntax($"{text}10 : тест  = 123-123"); }
            catch (SyntaxException ex)
            {
                Assert.AreEqual(
                    "Ошибка в строке 3. Неверное ключевое слово '10 : тест  = 123-123'." +
                    " Ожидается: <Метка> \":\" <Перем> \"=\" <Прав.часть>",
                    ex.Message);
            }
        }

        [Test]
        public void RightSideOperationSyntax()
        {
            var translator = new Translator();
            var text = "Программа\r\nМетки 1 2 3\r\n10 : тест = ";

            try { translator.CheckSyntax($"{text}12-(43-43)\\"); }
            catch (SyntaxException ex)
            {
                Assert.AreEqual(
                    "Ошибка в строке 3. Правая часть содержит недопустимые символы\n'12-(43-43)\\'",
                    ex.Message);
            }
            try { translator.CheckSyntax($"{text}12.34-434"); }
            catch (SyntaxException ex)
            {
                Assert.AreEqual(
                    "Ошибка в строке 3. Правая часть содержит недопустимые символы\n'12.34-434'",
                    ex.Message);
            }
            try { translator.CheckSyntax($"{text}124-43,4"); }
            catch (SyntaxException ex)
            {
                Assert.AreEqual(
                    "Ошибка в строке 3. Правая часть содержит недопустимые символы\n'124-43,4'",
                    ex.Message);
            }
        }

        [Test]
        public void UnknownMark()
        {
            var translator = new Translator();
            var text = "Программа\r\nМетки 1 2 3\r\n34 : тест = 324-43";

            try { translator.CheckSyntax($"{text}"); }
            catch (SyntaxException ex)
            {
                Assert.AreEqual(
                    "Ошибка в строке 3. Марка '34' не была объявлена!",
                    ex.Message);
            }
        }

        [Test]
        public void UniqueMark()
        {
            var translator = new Translator();
            var text = "Программа\r\nМетки 12 23 31\r\n12 : тест = 324-43\r\n12 : тест = 324-43";

            try { translator.CheckSyntax($"{text}"); }
            catch (SyntaxException ex)
            {
                Assert.AreEqual(
                    "Ошибка в строке 4. Марка '12' была использована более двух раз!",
                    ex.Message);
            }
        }

        [Test]
        public void SameVariable()
        {
            var translator = new Translator();
            var text = "Программа\r\nМетки 1 2 3\r\n1 : тест = 324-43";

            try { translator.CheckSyntax($"{text}\r\n2 : тест = 324-413"); }
            catch (SyntaxException ex)
            {
                Assert.AreEqual(
                    "Ошибка в строке 4. Такая переменная уже была объявлена ранее\n'тест'",
                    ex.Message);
            }
        }
        
        [Test]
        public void NoOperations()
        {
            var text = "Программа\r\nМетки 1 2 3\r\nКонец программа";

            var translator = new Translator();
            try { translator.CheckSyntax($"{text}"); }
            catch (SyntaxException ex)
            {
                Assert.AreEqual(
                    "Ошибка в строке 3. Нет ни одного блока <Операция>",
                    ex.Message);
            }
        }

        [Test]
        public void ProgramEndSyntax()
        {
            var text = "Программа\r\nМетки 1 2 3\r\n1 : тест = 324-43\r\n";

            var translator = new Translator();
            try { translator.CheckSyntax($"{text}Конец программа1"); }
            catch (SyntaxException ex)
            {
                Assert.AreEqual(
                    "Ошибка в строке 4. Неверное ключевое слово 'Конец программа1'. Ожидается: Конец программа",
                    ex.Message);
            }
            try { translator.CheckSyntax($"{text}!Конец программа"); }
            catch (SyntaxException ex)
            {
                Assert.AreEqual(
                    "Ошибка в строке 4. Неверное ключевое слово '!Конец программа'. Ожидается: Конец программа",
                    ex.Message);
            }
            try { translator.CheckSyntax($"{text}Конецпрограмма"); }
            catch (SyntaxException ex)
            {
                Assert.AreEqual(
                    "Ошибка в строке 4. Неверное ключевое слово 'Конецпрограмма'. Ожидается: Конец программа",
                    ex.Message);
            }
            try { translator.CheckSyntax($"{text}конец программа"); }
            catch (SyntaxException ex)
            {
                Assert.AreEqual(
                    "Ошибка в строке 4. Неверное ключевое слово 'конец программа'. Ожидается: Конец программа",
                    ex.Message);
            }
        }
        #endregion

        #region OrderExceptions

        [Test]
        public void NotPairedBrackets()
        {
            var translator = new Translator();
            var text = "Программа\r\nМетки 1 2 3\r\n1 : тест = ((324-43)";

            try { translator.CheckSyntax($"{text}"); }
            catch (OredrException ex)
            {
                Assert.AreEqual(
                    "Ошибка в строке 3. Правая часть с непарными скобками\n'((324-43)'",
                    ex.Message);
            }
        }

        [Test]
        public void BoolNotBool()
        {
            var translator = new Translator();
            var text = "Программа\r\nМетки 1 2 3\r\n1 : тест = ((324-43)|1)&1";

            try { translator.CheckSyntax($"{text}"); }
            catch (OredrException ex)
            {
                Assert.AreEqual(
                    "Ошибка в строке 3. Правая часть содержит обычные и булевы операции\n'((324-43)|1)&1'",
                    ex.Message);
            }
        }

        [Test]
        public void NumberBeforeOpenBrackets()
        {
            var translator = new Translator();
            var text = "Программа\r\nМетки 1 2 3\r\n1 : тест = ((324-43)-43(3*2))-2";

            try { translator.CheckSyntax($"{text}"); }
            catch (OredrException ex)
            {
                Assert.AreEqual(
                    "Ошибка в строке 3. Цифра не может идти после открывающей скобки. " +
                    "Нужен соединяющий оператор\n'((324-43)-43('",
                    ex.Message);
            }
        }

        [Test]
        public void CloseBracketBeforeOpenBrackets()
        {
            var translator = new Translator();
            var text = "Программа\r\nМетки 1 2 3\r\n1 : тест = 12-((2*3)(3*2))-2";

            try { translator.CheckSyntax($"{text}"); }
            catch (OredrException ex)
            {
                Assert.AreEqual(
                    "Ошибка в строке 3. Закрывающая скобка не может идти после открывающей скобки." +
                    $" Нужен соединяющий оператор\n'12-((2*3)('",
                    ex.Message);
            }
        }

        [Test]
        public void OpenBracketsBeforeCloseBracket()
        {
            var translator = new Translator();
            var text = "Программа\r\nМетки 1 2 3\r\n1 : тест = 12-((2*3)-()(3*2))-2";

            try { translator.CheckSyntax($"{text}"); }
            catch (OredrException ex)
            {
                Assert.AreEqual(
                    "Ошибка в строке 3. Скобки ничего не содержат\n'12-((2*3)-()'",
                    ex.Message);
            }
        }

        [Test]
        public void LineStartFromCloseBracket()
        {
            var translator = new Translator();
            var text = "Программа\r\nМетки 1 2 3\r\n1 : тест = )12-((2*3)-((3*2))-2";

            try { translator.CheckSyntax($"{text}"); }
            catch (OredrException ex)
            {
                Assert.AreEqual(
                    "Ошибка в строке 3. Закрывающая скобка как начало правой части\n')12-((2*3)-((3*2))-2'",
                    ex.Message);
            }
        }

        [Test]
        public void CloseBracketWithoutOpen()
        {
            var translator = new Translator();
            var text = "Программа\r\nМетки 1 2 3\r\n1 : тест = 2*3)-(3*2)-2(";

            try { translator.CheckSyntax($"{text}"); }
            catch (OredrException ex)
            {
                Assert.AreEqual(
                    "Ошибка в строке 3. Закрывающая скобка без открывающей скобки\n'2*3)-(3*2)-2('",
                    ex.Message);
            }
        }

        #endregion
    }
}
