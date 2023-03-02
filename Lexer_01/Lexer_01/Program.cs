using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;

namespace Lexer_01
{
    // Программа: Лексер
    // Назначение: Разбивает входящий файл на лексемы
    // Версия: 1.0 от 02.03.2023
    // Автор: Gogik Ortey

    class Program
    {
        #region MyPrint
        // Реализую свои методы вывода в консоль (для удобства)
        public static void print<Type>(Type Input)
        {
            Console.WriteLine(Input);
        }

        public static void println<Type>(Type Input)
        {
            // Печать строки без переноса каретки
            Console.Write(Input);
        }
        #endregion

        static void Main()
        {
            Stopwatch st = new Stopwatch(); // Таймер
            st.Start();            
            
            // Основной код:
            {
                MainLexer MainLexer = new MainLexer();

                MainLexer.OpenInputFiles("");   // Загружаем файл в массивы программы
                MainLexer.processingLine();     // Построково обрабатываем
            }

            st.Stop();
            Console.Write("\nВремя выполнения программы: {0}", st.Elapsed.TotalSeconds); print(" секунд");
        }

        public class MainLexer
        {
            public string InputNameFile = "Input.txt";                      // Название входного файла
            public List<string> InputLine = new List<string>();             // Хранит все полученные строки входного файла

            public List<string> outpTable = new List<string>();
            // Массив, хранящий таблицу, которую мы выводим в конце работы программы

            // Словарь для хранения лексем, и их поиска
            Dictionary<string, string> dickOfLex = new Dictionary<string, string>
            {
                {"if", "IF"},
                {"defun", "DEF"},
                {"int", "INT"},
                {"float", "FLOAT"},
                {"return", "RET"},

                {"print", "RESERVED_NAME"},
                {"round", "RESERVED_NAME"},

                {"{", "START_VOID"},
                {"}", "END_VOID"},

                {"(", "START_ARGUMENT"},
                {")", "END_ARGUMENT"},

                {".", "DOT"},
                {",", "COMMA"},
                {";", "END_LINE"},

                {"=", "EQUALLY"},

                {"+", "OPERATION_TOKEN"},
                {"-", "OPERATION_TOKEN"},
                {"*", "OPERATION_TOKEN"},
                {"/", "OPERATION_TOKEN"},

                {"!", "LOGIC_TOKEN"},
                {"&", "LOGIC_TOKEN"},
                {"|", "LOGIC_TOKEN"},
                {">", "LOGIC_TOKEN"},
                {"<", "LOGIC_TOKEN"}

            };  // Вот тут их можно развернуть

            // Метод чтения строк из входного файла
            public bool OpenInputFiles(string nameInputFile)
            {
                if (nameInputFile == "") nameInputFile = InputNameFile; // Имя файла можно не указывать, при вызове этой процедуры

                try
                {
                    using (StreamReader fs = new StreamReader(nameInputFile))
                    {
                        string currentLine = "";
                        while ((currentLine = fs.ReadLine()) != null) // Построчно считывает
                        {
                            //Console.WriteLine(currentLine);
                            InputLine.Add(currentLine); // И построчно добавляет в двумерный лист
                        }

                        improvingFileReadability(); // Улучшает входной файл
                        OutOfInputFile(); // Выводит весь файл в консоль
                    }
                    return true;
                }
                catch (Exception e)
                {
                    Console.WriteLine("The file could not be read:");
                    Console.WriteLine(e.Message);
                    return false;
                }
            }

            // Улучшение читаемости файла, путём вставления пробелов между любыми скобками
            public void improvingFileReadability()
            {
                for (int i = 0; i < InputLine.Count; i++)
                {
                    for (int j = 0; j < InputLine[i].Length; j++)
                    {
                        if ((InputLine[i][j] == '(') || (InputLine[i][j] == ')'))
                        {
                            //print("Нашли скобку в " + i + "-й строке на " + j + "-м месте");

                            InputLine[i] = InputLine[i].Insert(j, " ");
                            InputLine[i] = InputLine[i].Insert(j+2, " ");

                            j += 2;
                        }
                    }
                }
            }

            // Выводим весь массив строк, считанный из входного файла
            public void OutOfInputFile()
            {
                print("Входной файл:");

                if (InputLine.Count != 0)
                    InputLine.ForEach(Console.WriteLine);
                else
                    print("Входной файл не был прочитан, или массив строк пуст.");
            }


            // Реализация простейшей процедуры поиска в словаре
            /*
            public void SearchOfDick()
            {
                string res = null;
                string inp = "if";
                dickOfLex.TryGetValue(inp, out res);
                if (res == null)
                {
                    print("Не нашли элемент " + inp);
                }
                else
                {
                    print("Код элемента " + inp + " = " + res);
                }
            }
            */

            // Нерекурсивная процедура поиска в массиве
            public string SearchOfDick(string inp)
            {
                string res = null;
                dickOfLex.TryGetValue(inp, out res); // Ищем элемент по ключу inp. В случае успеха, найденное значение положится в переменную res
                return res;
            }

            bool insertToEnd = false; // Флаг. Нужен для корректной вставки распознанных лексем в буферную очередь

            // Рекурсивная процедура поиска в массиве (основная)
            public string SearchOfDickOnRecurs(string inp) 
            {
                string res = null;
                dickOfLex.TryGetValue(inp, out res); // Ищем в массиве по ключу

                if (res == null) // Если не нашли
                {
                    string newInp = "";

                    // Обяснение, что тут за сложный код:

                    /*
                        Тут я реализовал алгоритм уменьшения строки. Т.е., если мы не смогли найти лексему в массиве при поиске,
                        это не означает, что в строке её нет. Их там может быть, кстати, много

                        Например, нас попалась такая строка: "(int("

                        Ниже, в первом for я посимвольно создаю новую строку, с левого конца
                        Вот так это выглядит:

                        i = 1: "("
                        i = 2: "(i"
                        i = 3: "(in"
                        ...

                        Далее, я на каждом шаге запускаю процедуру нерекурсивного поиска. Например, мы нашли лексему "(", при i = 1.
                        Тогда, я отправляю её в начало очереди буферных лексем

                        Это сделано потомучто по другому не сделать

                        Далее, я создаю новую строку, без тех символов, которые я опознал. В этом примере она будет выглядеть так: "int("
                        И запускаю эту рекурсивную процедуру ещё раз, уже с этой новой строкой

                        ---

                        Понятно, что строку "int(" не получится идентефицировать, если брать символы слева
                        И логично будет пойти с другой строрны - справа

                        Этим как раз и занимается второй for ниже
                        Вот так будет выглядеть его работа:

                        i = 1: "("
                        i = 2: "t("
                        i = 3: "nt("
                        i = 4: "int("
                        ...

                        Он опознает скобку, уже на первом шаге, и дальше строка "int" снова уйдёт в рекурсию, и верно идентефицируется.

                        В принципе это всё, что нужно знать, для понимания
                    */

                    for (int i = 1; i < inp.Length; i++)
                    {
                        newInp = new String(inp.ToCharArray(), 0, i);
                        string searchNewInp = SearchOfDick(newInp);

                        if (searchNewInp != null)
                        { 
                            string finalInp = new String(inp.ToCharArray(), i, (inp.Length - i));
                            //print("finalInp = " + finalInp);

                            buferOfLexemsDetectionForDick.Add(newInp);
                            buferOfLexemsDetectionForDick.Add(searchNewInp);

                            insertToEnd = true;
                            SearchOfDickOnRecurs(finalInp);

                            return null;
                        }
                    }

                    for (int i = 1; i < inp.Length; i++)
                    {
                        newInp = new String(inp.ToCharArray(), i, inp.Length-i);
                        string searchNewInp = SearchOfDick(newInp);

                        if (searchNewInp != null)
                        {
                            string finalInp = new String(inp.ToCharArray(), 0, i);

                            buferOfLexemsDetectionForDick.Add(newInp);
                            buferOfLexemsDetectionForDick.Add(searchNewInp);

                            insertToEnd = false;
                            SearchOfDickOnRecurs(finalInp);

                            return null;
                        }
                    }
                }
                else // Если нашли лексему при поиске в словаре
                {
                    if (buferOfLexemsDetectionForDick.Count != 0)
                    {
                        buferOfLexemsDetectionForDick.Add(inp);
                        buferOfLexemsDetectionForDick.Add(res);

                        return res;
                    }
                    else
                        return res;
                }

                if (buferOfLexemsDetectionForDick.Count != 0) 
                {
                    int adrInp;
                    if (int.TryParse(inp, out adrInp) == true) // Тут, если наша новая лексема - число
                    {
                        tokenDetection(inp);
                        return res;
                    }

                    if (insertToEnd == false) // Тут, либо добавление в начало очереди (если мы шли с правого конца строки, вторым for)
                    {
                        buferOfLexemsDetectionForDick.Insert(0, inp);
                        buferOfLexemsDetectionForDick.Insert(1, "ID");
                    }
                    else // Либо добавление в конец очереди (если мы шли с левого конца строки, первым фором)
                    {
                        buferOfLexemsDetectionForDick.Add(inp);
                        buferOfLexemsDetectionForDick.Add("ID");
                    }
                }
                else
                    return res;

                return res;
            }

            public List<string> buferOfLexemsDetectionForDick = new List<string>();
            // Буферный массив для распознанных лексем, которых было больше одной в строке, полученной процедурой SearchOfDickOnRecurs()

            // Это основная процедура построковой и посимвольной обработки файла
            // Она разделяет все входящие лексемы, которые отделены пробелами, и без разбора отправляет в tokenDetection
            public void processingLine()
            {
                for (int i = 0; i < InputLine.Count; i++)
                {
                    if (InputLine[i] != "")
                    {
                        char currentChar = InputLine[i][0];
                        string bufer = "";

                        for (int j = 0; j < InputLine[i].Length; j++)
                        {
                            if ((InputLine[i][j] != ' ') && (InputLine[i][j] != '	'))
                            {
                                bufer += InputLine[i][j];
                            }
                            else
                            {
                                if (bufer.Length > 0)
                                {
                                    tokenDetection(bufer);
                                    //print(bufer);
                                    bufer = "";
                                }
                            }
                        }
                        if(bufer.Length>0) tokenDetection(bufer);
                    }
                }

                printFinalTable(); // А в конце печатает финальную таблицу с распознанными лексемами
            }

            public void tokenDetection(string inp) // Принимает либо лексему, либо несколько лексем в одной строке, подряд
            {
                //print("Распознаём: " + inp);

                int adrInp;
                if (int.TryParse(inp, out adrInp) == true) // Если полученная строка является числом
                {
                    // То это число
                    int indexOfDott = inp.IndexOf('.');
                    if (indexOfDott != -1)
                    {
                        string h = inp.Remove(indexOfDott);
                        if (h.IndexOf('.') != -1)
                        {

                            outpTable.Add(inp);
                            outpTable.Add("ERROR"); // Если в строке больше одной точки - то это не число, это ошибка
                        }
                        else
                        {
                            outpTable.Add(inp);
                            outpTable.Add("NUM_FLOAT"); // Это не тестировал
                        }
                    }
                    else
                    {
                        outpTable.Add(inp);
                        outpTable.Add("NUM_INT");
                    }
                }
                else // Если строка не число
                {
                    string res = SearchOfDickOnRecurs(inp); // Распознаём лексему по словарю (рекурсивно)

                    if ((res == null) && (buferOfLexemsDetectionForDick.Count == 0)) // Если лексема была в строке одна, и её не нашлось в словаре - то это ID
                    {
                        outpTable.Add(inp);
                        outpTable.Add("ID");
                    }
                    else if ((res != null) && (buferOfLexemsDetectionForDick.Count == 0)) // Если лексема нашлась в словаре, и была одна
                    {
                        outpTable.Add(inp);
                        outpTable.Add(res);
                    }
                    else
                    {
                        detectingOfBuferLexems(); // Если лексем в строке было несколько
                    }
                }

                //printFinalTable();
            }

            public void detectingOfBuferLexems()
            {
                // Наверняка вы заметили, что я не создал 2 массива, или не создал вложенные массивы, для хранения значений [NAME, TOKEN]
                // Всё проще. Чётные значения в листе - это NAME, а нечётные - это TOKEN

                for (int i = 0; i < buferOfLexemsDetectionForDick.Count; i+=2)
                {
                    outpTable.Add(buferOfLexemsDetectionForDick[i]);
                    outpTable.Add(buferOfLexemsDetectionForDick[i + 1]);
                }

                buferOfLexemsDetectionForDick.Clear(); // После каждой итерации распознавания, не забываю чистить буферную очередь
            }

            public int maxLengthFexem = 0;   // Максимальная длинна распознанных лексем 

            // Находит самое длинное название лексемы
            public int get_maxLengthFexem()
            {
                if (maxLengthFexem != 0) return maxLengthFexem;
                else
                {
                    for (int i = 0; i < outpTable.Count; i+=2)
                    {
                        if (outpTable[i].Length > maxLengthFexem)
                        {
                            //print(">>> " + outpTable[i]);
                            maxLengthFexem = outpTable[i].Length;
                        }
                    }
                }

                // Кончено, можно было и эффективнее это сделать, но я не захотел заморачиваться.

                return maxLengthFexem;
            }

            // Печатает финальную таблицу с распознанными лексемами
            public void printFinalTable()
            {
                print("\nВсе распознанные лексемы: \n");

                get_maxLengthFexem();

                for (int i = 0; i < outpTable.Count; i += 2)
                {
                    // Тут опять немного сложного кода, для красивого вывода)

                    println(i / 2);

                    if ((i/2 < 10) && (outpTable.Count < 100)) println(" ");
                    else if ((i/2 < 10) && (outpTable.Count < 100)) println("  ");
                    else if ((i/2 < 100) && (outpTable.Count > 100)) println(" ");

                    println("  " + outpTable[i]);

                    int a_size = maxLengthFexem - outpTable[i].Length;
                    string newSpase = new String(' ', a_size);
                    println(newSpase);

                    print(" -  " + outpTable[i + 1]);
                }
            }
        }
    }
}

/*
    Это алгоритм работы всех процедур. Я расписывал его для себя, но наверно не буду удалять

    Идём посимвольно вперёд, пока не встретим проблел или таб
    Все символы сохраняем в буфер, и потом отправляем на опознание

    Если встретился символ:
        " "
        "   "
    то прекращаем ввод, и отправляем на опознание, если мы сохранили болше 0 символов

    ---- В опознании лексем:
    
    Если лексема начинается с цифры, то:
        Если он состоит только из цифр, то это целое число (NUM_INT)
        Иначе если у него встречается одна точка, то это не целое число (NUM_FLOAT)
        Иначе - это ошибка распознавания (ERROR)
    Иначе - отправляем в словарь, на распознавание
        Если в словаре распозналось - оставляем, то что пришло от него
        Иначе - это название переменной или метода (ID)

    ---- В распознании по словарю:

    Если не можем распознать строку, то:
        создаём новую пустую, и начинаем итерационно добавлять к неё символы из входной строки
            Если мы добавили всю строку, и ничего не распознали, то выкидываем ""
            Но, если мы хоть что-то распознали, то выкидываем это как ответ, но перед этим:
                Запускаем рекурсивный вызов распознавания в словаре, оставшегося кусочка
*/