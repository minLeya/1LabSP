using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SystemProgramming1
{

   class FirstSecondPass
    {
       string NameProg;
       public string ErrorMessage = "";
       int EndAddress = 0;   //Адрес точки входа в программу
       int StartAddress = 0; // Адрес начала программы
       int AddressCount = 0;
       const int MAX_MemmoryAdr = 16777215; //максимальное значение адреса  оперативной памяти
         
       //| Метка| МКОП | Операнд1 | Операнд2 | Вспомогательная таблица
       List<List<string>> SupportTable = new List<List<string>>();
       
       //| Метка| Адрес | Таблица символических имен
       List<List<string>> SymbolNameTable = new List<List<string>>();
       
       //Таблица исходного кода
       List<string> BinaryCode = new List<string>();
       
       public void AddStringToSupportTable(string n1,string n2,string n3,string n4)
       {
           SupportTable[0].Add(n1);
           SupportTable[1].Add(n2);
           SupportTable[2].Add(n3);
           SupportTable[3].Add(n4);
       }

       public bool MemoryCheck()
       {
           if (AddressCount < 0 || AddressCount > MAX_MemmoryAdr)
           {
               ErrorMessage = "Выход за границы доступной памяти";
               return false;
           }
           return true;
       }

       //передается массив, номер строки который надо разложить по переменным
       //проверяется допустимость введеных символов
       public bool GetRow(string[,] mas, int number, out string label, out string command, out string dir1, out string dir2)
        {
            label = mas[number, 0];
            command = mas[number, 1].ToUpper();
            dir1 = mas[number, 2];
            dir2 = mas[number, 3];

            if (Check.IsDirective(label)||Check.OnlyRegisters(label))
            {
                return false;
            }
            if (number > 0 && NameProg == label.ToUpper())
                return false;

           
            if ((Check.OnlySymbolsAndNumbers(label)||label.Length==0) && 
                (Check.OnlySymbolsAndNumbers(command)) && 
                (Check.OnlySymbolsAndNumbers(dir2)||dir2.Length==0))
            {
                if (label.Length > 0)
                {
                    //метка должна начинаться с символа
                    if (Check.OnlySymbols(Convert.ToString(label[0])))
                        return true;
                    else
                        return false;
                }
                return true;
            }
            else
            {
                return false;
            }              
        }
       
       //Процедура проверяет таблицу кодов операций 
       public bool CheckOperationCodeTable(ref string[,] OperationCodeTable) //проверяем таблицу  операций
        {
           int rows = OperationCodeTable.GetLength(0);

           for (int i = 0; i < rows; i++)
           {
                           
               if (OperationCodeTable[i, 0] == "" || OperationCodeTable[i, 1] == "" || OperationCodeTable[i, 2] == "")
               {
                   ErrorMessage = "Строка " + (i + 1) + ". Пустая ячейка в таблице кодов операций недопустима";
                   return false;
               }

               if (OperationCodeTable[i, 0].Length > 6 || OperationCodeTable[i, 1].Length > 2 || OperationCodeTable[i, 2].Length > 1)
               {
                   ErrorMessage = "Строка " + (i + 1) + ". Ошибка в размере строки в таблице кодов операций (Команда (от 1 до 6), мнемонический код операции (от 1 до 2), Длина(не более 1))";
                   return false;
               }

                if (!Check.OnlySymbolsAndNumbers(OperationCodeTable[i, 0]))
               {
                   ErrorMessage = "Строка " + (i + 1) + ". В поле команды недопустимый символ";
                   return false;
               }


               //проверяем поле МКОП команды, что там были только 16чные цифры
               if (Check.IsAdressPossible(OperationCodeTable[i, 1]))
               {

                   if (Check.IsDirective(OperationCodeTable[i, 0]) || Check.OnlyRegisters(OperationCodeTable[i, 0]))
                   {
                       ErrorMessage = "Строка " + (i + 1) + ". Поле код команды является зарезервированным словом";
                       return false;
                   }
                   //преобразуем их в число
                   int count = Converts.HexToDec(OperationCodeTable[i, 1]);
                   if (count > 63)
                   {
                       ErrorMessage = "Строка " + (i + 1) + ". Поле код команды не должен превышать 3F";
                       return false;
                   }
                   else
                   {
                       //если оно в пределах байта, но записано просто как ... "F" поправляем его на "0F"
                       if (OperationCodeTable[i, 1].Length == 1)
                           OperationCodeTable[i, 1] = Converts.ToTwoChars(OperationCodeTable[i, 1]);
                   }
               }
               else
               {
                       ErrorMessage = "Строка " + (i + 1) + ". Посторонние символы в поле мнемонического кода операции";
                       return false;
               }

               int value = 0;
               if (Check.OnlyNumbers(OperationCodeTable[i, 2]))
               {
                   bool result = Int32.TryParse(OperationCodeTable[i, 2], out value);
                   if (result)
                   {
                       if (value <= 0 || value > 4)
                       {
                           ErrorMessage = "Строка " + (i + 1) + ". Проверьте размер команды (от 1 до 4)";
                           return false;
                       }
                   }
               }
               else
               {
                  ErrorMessage = "Строка " + (i + 1) + ". В поле размер операции недопустимый символ";
                  return false;
               }  
               
               //проверяем уникальность поля названия команды
               for (int k = i + 1; k < rows; k++)
               {  
                   string cmp_str1 = OperationCodeTable[i, 0];
                   string cmp_str2 = OperationCodeTable[k, 0];
                   if (Equals(cmp_str1, cmp_str2))
                   {
                       ErrorMessage = "Строка " + (i + 1) + ". Найдены совпадения в команде";
                       return false;
                   }
               }
               //проверяем уникальность поля код операции
               for (int k = i + 1; k < rows; k++)
               {
                   string str1 = Convert.ToString(Converts.HexToDec(OperationCodeTable[i, 1]));
                   string str2 = Convert.ToString(Converts.HexToDec(OperationCodeTable[k,1]));
                   if (Equals(str1, str2))
                   {
                       ErrorMessage = "Строка " + (i + 1) + ". Найдены совпадения в коде команды";
                       return false;
                   }
               }
           }

           return true;
        }

       //Метод проверяет каждую ячейку таблицы, имеются ли в ней записи, если в строке нет данных, то она удаляется
       public void DeleteEmptyRows(DataGridView DBGrid_source_code)
       {
           for (int i = 0; i < DBGrid_source_code.Rows.Count - 1; i++)
           {
               bool empty = true;
               for (int j = 0; j < DBGrid_source_code.Rows[i].Cells.Count; j++)
                   if ((DBGrid_source_code.Rows[i].Cells[j].Value != null) && (DBGrid_source_code.Rows[i].Cells[j].Value.ToString() != ""))
                       empty = false;

               if (empty)
               {
                   DBGrid_source_code.Rows.Remove(DBGrid_source_code.Rows[i]);
               }
           }
       }

       //проверяет есть ли такая команда в массиве команд
       public int FindCodeInCodeTable(string label,string[,] code_table)
       {
           for (int i = 0; i < code_table.GetLength(0); i++)
           {
               if (label == code_table[i, 0])
                    return i;
           }
           return -1;
       }

       //проверяет есть ли такая метка в массиве меток
       public int FindLabelInLabelTable(string label)
       {
           for (int i = 0; i < SymbolNameTable[0].Count; i++)
           {
               if (label == SymbolNameTable[0][i])
                   return i;
           }
           return -1;
       }
       

       //Процедура проверяет таблицу исходного кода, ищет директиву START,END. 
       //Проверяет адрес памяти, не выходим ли за границу  и т.д.
       //какие символы используются для названия программы
       public bool FirstPass(string[,] SourceCodeTable, string[,] OperCodeTable, 
           DataGridView DG_SupportTable,DataGridView DG_SymbolTable)
       {
           AddressCount = 0;
           StartAddress = 0;
           EndAddress = 0;
           //Добавляем "столбцы" в списки списков
           SymbolNameTable.Add(new List<string>());
           SymbolNameTable.Add(new List<string>());

           SupportTable.Add(new List<string>());
           SupportTable.Add(new List<string>());
           SupportTable.Add(new List<string>());
           SupportTable.Add(new List<string>());

           int rows = SourceCodeTable.GetLength(0)-1;

           int StartFlag = 0;         //флаг найденной директивы START
           int EndFlag   = 0;         //флаг найденной директивы END

            
           string[,] arr_CodeTable = OperCodeTable;

           string label, MKOP, Operand1, Operand2;
      

            // Организуем цикл по обработке строк ИСХОДНОГО ТЕКСТА ПРОГРАММЫ
            for (int i = 0; i <= rows; i++)
           {
               //если директива старт найдена
               if (StartFlag == 1)
               {
                   //то адрес уже записан в переменную 
                   //и надо проверить чтобы он не выходил за диапазон
                   if (AddressCount > MAX_MemmoryAdr)
                   {
                       ErrorMessage = "Строка " + (i + 1) + ". Произошло переполнение";
                       return false;
                   }
               }

               //Проверяем, если директива END найдена, то можно выходить из цикла
               if (EndFlag == 1){
                     break;
               }
               
               //берем строку из массива и сразу же проверяем корректность данных
               //строка состоит из Label MKOP Operand1 Operand2
               if (!GetRow(SourceCodeTable, i, out label, out MKOP, out Operand1, out Operand2))
               {
                   ErrorMessage = "Синтаксическая ошибка в строке " + (i+1);
                   return false;
               }
               
               // Смотрим сперва  на метку, есть ли она в таблице меток
               //number_in_LabelTable  - укажет на строку в которой она находится
               int number_in_LabelTable = FindLabelInLabelTable(label);

               if (number_in_LabelTable != -1)
               {
                   ErrorMessage = "Строка " + (i + 1) + ". Найдена уже существующая метка " + label;
                   return false;
               }
               // если не найдена,то добавляем её и смотрим на МКОП
               else
               {
                   //если метка это не пустая строка и встречена после директивы старт
                   //то добавляем её в таблицу меток
                   if (label != "" && StartFlag == 1)
                   {
                       SymbolNameTable[0].Add(label);
                       SymbolNameTable[1].Add(Converts.ToSixChars(Converts.DecToHex(AddressCount)));
                   }



                   if (Check.IsDirective(MKOP))
                   {
                       

                        switch (MKOP)
                        {
                            case "START":
                                {
                                    //если у нас старт не в начале массива, и найден в массиве еще раз то ошибка
                                    if (i == 0 && StartFlag == 0)
                                    {
                                        StartFlag = 1;
                                        //смотрим на операнд, символы соответствуют 16ричной сс
                                        if (Check.IsAdressPossible(Operand1))
                                        {
                                            //если да то преобразуем 16ричное число в 10чное
                                            AddressCount = Converts.HexToDec(Operand1);

                                            StartAddress = AddressCount;
                                            //адрес начала программы не может быть равен 0

                                            if (AddressCount == 0)
                                            {
                                                ErrorMessage = "Строка " + (i + 1) + ". Адрес начала программы не может быть равен 0";
                                                return false;
                                            }

                                            //адрес начала программы не может превышать объем памяти
                                            if (AddressCount > MAX_MemmoryAdr || AddressCount < 0)
                                            {
                                                ErrorMessage = "Строка " + (i + 1) + ". Неправильный адрес загрузки.";
                                                return false;
                                            }

                                            if (label == "")
                                            {
                                                ErrorMessage = "Строка " + (i + 1) + ". Не задано имя программы";
                                                return false;
                                            }
                                            if (label.Length > 10)
                                            {
                                                ErrorMessage = "Строка " + (i + 1) + ". Превышена длина имени программы( > 10 символов)";
                                                return false;
                                            }

                                          
                                            //теперь помещаем это в выходной массив
                                            AddStringToSupportTable(label, MKOP, Converts.ToSixChars(Operand1), "");
                                            NameProg = label;
                                            //выводим предупреждение если такое имеется
                                            if (Operand2.Length > 0)
                                                ErrorMessage = ErrorMessage + "Строка " + (i + 1) + ". Второй операнд директивы START не рассматривается\n";

                                        }
                                        else
                                        {
                                            ErrorMessage = "Строка " + (i + 1) + ". Неверный адрес начала программы";
                                            return false;
                                        }
                                    }
                                    else
                                    {
                                        ErrorMessage = "Строка " + (i + 1) + ". Ошибка в директиве START";
                                        return false;
                                    }
                                }
                                break;

                            case "WORD":
                                {
                                    int number;
                                    //В WORD у нас могут быть записаны только числа (в данной проге только положительные)
                                    //преобразовываем операнд в число
                                    if (int.TryParse(Operand1, out number))
                                    {
                                        if (number >= 0 && number <= MAX_MemmoryAdr)
                                        {
                                            if (AddressCount + 3 > MAX_MemmoryAdr)
                                            {
                                                ErrorMessage = "Произошло переполнение";
                                                return false;
                                            }
                                            if (!MemoryCheck())
                                            {
                                                return false;
                                            }

                                            AddStringToSupportTable(Converts.ToSixChars(Converts.DecToHex(AddressCount)), 
                                                MKOP, Convert.ToString(number), "");

                                            AddressCount = AddressCount + 3;

                                            if (!MemoryCheck()) 
                                            { return false; }

                                            if (AddressCount < 0 || AddressCount > MAX_MemmoryAdr)
                                            {
                                                ErrorMessage = "Строка " + (i + 1) + ". Выход за границы доступной памяти";
                                                return false;
                                            }


                                            if (Operand2.Length > 0)
                                                ErrorMessage = ErrorMessage + "Строка " + (i + 1) + ". Второй операнд директивы WORD не рассматривается\n";
                                        }
                                        else
                                        {
                                            ErrorMessage = "Строка " + (i + 1) + ". Отрицательное число или превышено максимальное значение.";
                                            return false;
                                        }
                                    }
                                    else
                                    {
                                        //символ вопроса, резервирует 1 слово в памяти
                                        if (Operand1.Length == 1 && Operand1 == "?")
                                        {

                                            if (AddressCount + 3 > MAX_MemmoryAdr)
                                            {
                                                ErrorMessage = "Произошло переполнение";
                                                return false;
                                            }

                                            AddStringToSupportTable(Converts.ToSixChars(Converts.DecToHex(AddressCount)),
                                                MKOP, Operand1, "");

                                            AddressCount = AddressCount + 3;

                                            if (!MemoryCheck()) { return false; }

                                            if (Operand2.Length > 0)
                                                ErrorMessage = ErrorMessage + "Строка " + (i + 1) + ". Второй операнд директивы WORD не рассматривается\n";
                                        }
                                        else
                                        {
                                            ErrorMessage = "Строка " + (i + 1) + ". Невозможно выполнить преобразование в число " + Operand1;
                                            return false;
                                        }
                                    }
                                }
                                break;
                            case "BYTE":
                                {
                                    int number;
                                    //пытаемся преобразовать операнд в число (разрешено только положительное 0 до 255)

                                    if (int.TryParse(Operand1, out number))
                                    {
                                        if (number >= 0 && number <= 255)
                                        {

                                            if (AddressCount + 1 > MAX_MemmoryAdr)
                                            {
                                                ErrorMessage = "Произошло переполнение";
                                                return false;
                                            }

                                            //BYTE = 1 байт, увеличиваем адрес
                                            AddStringToSupportTable(Converts.ToSixChars(Converts.DecToHex(AddressCount)), 
                                                MKOP, Convert.ToString(number), "");

                                            AddressCount = AddressCount + 1;

                                            if (!MemoryCheck()) 
                                            { return false; }


                                            if (Operand2.Length > 0)
                                                ErrorMessage = ErrorMessage + "Строка " + (i + 1) + ". Второй операнд директивы BYTE не рассматривается\n";
                                        }

                                        else
                                        {
                                            ErrorMessage = "Строка " + (i + 1) + ". Отрицательное число, либо превышено максимальное значение числа";
                                            return false;
                                        }
                                    }
                                    //если преобразование в число не получилось, значит разбираем строку
                                    else
                                    {
                                        //первый символ 'C' второй и последний символ это кавычки и длина строки >3
                                        string symbols = Check.String(Operand1);
                                        if (symbols != "")
                                        {

                                            if (AddressCount + symbols.Length > MAX_MemmoryAdr)
                                            {
                                                ErrorMessage = "Произошло переполнение";
                                                return false;
                                            }

                                            AddStringToSupportTable(Converts.ToSixChars(Converts.DecToHex(AddressCount)), 
                                                MKOP, Operand1, "");

                                            AddressCount = AddressCount + symbols.Length;

                                            if (!MemoryCheck()) 
                                            { return false; }


                                            if (Operand2.Length > 0)
                                                ErrorMessage = ErrorMessage + "Строка " + (i + 1) + ". Второй операнд директивы BYTE не рассматривается\n";
                                            continue;
                                        }

                                        //первый символ 'X' второй и последний символ это кавычки и длина строки >3
                                        symbols = "";
                                        symbols = Check.ByteString(Operand1);

                                        if (symbols != "")
                                        {
                                            int lenght = symbols.Length;
                                            //1 символ = 1 байт = 2 цифры в 16ричной системе = четное число символов
                                            if ((lenght % 2) == 0)
                                            {
                                                if (AddressCount + symbols.Length / 2 > MAX_MemmoryAdr)
                                                {
                                                    ErrorMessage = "Произошло переполнение";
                                                    return false;
                                                }

                                                AddStringToSupportTable(Converts.ToSixChars(Converts.DecToHex(AddressCount)), 
                                                    MKOP, Operand1, "");

                                                AddressCount = AddressCount + symbols.Length / 2;

                                                if (!MemoryCheck()) 
                                                { return false; }

                                                if (Operand2.Length > 0)
                                                    ErrorMessage = ErrorMessage + "Строка " + (i + 1) + ". Второй операнд директивы BYTE не рассматривается \n";
                                                continue;
                                            }
                                            else
                                            {
                                                ErrorMessage = "Строка " + (i + 1) + ". Невозможно преобразовать BYTE нечетное количество символов";
                                                return false;
                                            }
                                        }

                                        //если там всего один символ "?"
                                        if (Operand1.Length == 1 && Operand1 == "?")
                                        {
                                            if (AddressCount + 1 > MAX_MemmoryAdr)
                                            {
                                                ErrorMessage = "Произошло переполнение";
                                                return false;
                                            }

                                            AddStringToSupportTable(Converts.ToSixChars(Converts.DecToHex(AddressCount)), 
                                                MKOP, Operand1, "");

                                            AddressCount = AddressCount + 1;

                                            if (!MemoryCheck()) { return false; }

                                            if (Operand2.Length > 0)
                                                ErrorMessage = ErrorMessage + "Строка " + (i + 1) + ". Второй операнд директивы BYTE не рассматривается \n";
                                            continue;
                                        }
                                        else
                                        {
                                            ErrorMessage = "Строка " + (i + 1) + ".  Неверный формат строки " + Operand1;
                                            return false;
                                        }
                                    }
                                }
                                break;

                            case "RESB":
                                {
                                    int number;
                                    if (int.TryParse(Operand1, out number))
                                    {
                                        if (number > 0)
                                        {

                                            if (AddressCount > MAX_MemmoryAdr)
                                            {
                                                ErrorMessage = "ереполнение памяти";
                                                return false;
                                            }
                                            else
                                            {
                                                if (AddressCount + number > MAX_MemmoryAdr)
                                                {
                                                    ErrorMessage = "Произошло переполнение";
                                                    return false;
                                                }

                                                AddStringToSupportTable(Converts.ToSixChars(Converts.DecToHex(AddressCount)), 
                                                    MKOP, Convert.ToString(number), "");

                                                AddressCount = AddressCount + number;   //WORD = 3 байта, увеличиваем адрес

                                                if (!MemoryCheck()) 
                                                { return false; }

                                                if (Operand2.Length > 0)
                                                    ErrorMessage = ErrorMessage + "Строка " + (i + 1) + ". Второй операнд директивы RESB не рассматривается \n";
                                            }

                                        }
                                        else
                                        {
                                            ErrorMessage = "Строка " + (i + 1) + ". Количество байт равно нулю или меньше нуля";
                                            return false;
                                        }
                                    }
                                    else
                                    {
                                        ErrorMessage = "Строка " + (i + 1) + ". Невозможно выполнить преобразование в число " + Operand1;
                                        return false;
                                    }
                                }
                                break;
                            case "RESW":
                                {
                                    int number;
                                    if (int.TryParse(Operand1, out number))
                                    {
                                        if (number > 0)
                                        {

                                            if (AddressCount + number * 3 > MAX_MemmoryAdr)
                                            {
                                                ErrorMessage = "Произошло переполнение";
                                                return false;
                                            }

                                            AddStringToSupportTable(Converts.ToSixChars(Converts.DecToHex(AddressCount)), 
                                                MKOP, Convert.ToString(number), "");

                                            //WORD = 3 байта, увеличиваем адрес
                                            AddressCount = AddressCount + number * 3;

                                            if (!MemoryCheck()) 
                                            { return false; }

                                            if (AddressCount < 0 || AddressCount > MAX_MemmoryAdr)
                                            {
                                                ErrorMessage = "Строка " + (i + 1) + ". Выход за границы доступной памяти";
                                                return false;
                                            }

                                            if (Operand2.Length > 0)
                                                ErrorMessage = ErrorMessage + "Строка " + (i + 1) + ". Второй операнд директивы RESW не рассматривается \n";
                                        }
                                        else
                                        {
                                            ErrorMessage = "Строка " + (i + 1) + ". Количество слов равно нулю или меньше нуля";
                                            return false;
                                        }
                                    }
                                    else
                                    {
                                        ErrorMessage = "Строка " + (i + 1) + ". Невозможно выполнить преобразование в число " + Operand1;
                                        return false;
                                    }
                                }
                                break;

                            case "END":
                                {
                                    if (StartFlag == 1 && EndFlag == 0)
                                    {
                                        EndFlag = 1;
                                        if (Operand1.Length == 0)
                                        {
                                            EndAddress = StartAddress;

                                            // Добавляем строку в таблицу
                                            AddStringToSupportTable(
                                                Converts.ToSixChars(Converts.DecToHex(AddressCount)),
                                                MKOP,
                                                Converts.ToSixChars(Converts.DecToHex(StartAddress)),
                                                ""
                                            );
                                        }
                                        else
                                        {
                                            if (Check.IsAdressPossible(Operand1))
                                            {
                                                //если да то преобразуем 16ричное число в 10чное
                                                EndAddress = Converts.HexToDec(Operand1);
                                                AddStringToSupportTable(Converts.ToSixChars(Converts.DecToHex(AddressCount)),
                                                MKOP, Converts.ToSixChars(Operand1), "");
                                                if (EndAddress >= StartAddress && EndAddress <= AddressCount)
                                                {
                                                    break;
                                                }
                                                else
                                                {
                                                    ErrorMessage = "Строка " + (i + 1) + ". Адрес точки входа неверен";
                                                    return false;
                                                }
                                            }
                                            
                                            else
                                            {
                                                ErrorMessage = "Строка " + (i + 1) + ". Неверный адрес входа в программу";
                                                return false;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        ErrorMessage = "Строка " + (i + 1) + ". Ошибка в директиве END";
                                        return false;
                                    }
                                }
                                break;
                        }
                   }
                       
                   //значит в строке команда, обрабатываем тут
                   else
                   {
                       //Если в строке МКОП что-то написано
                       if (MKOP.Length > 0)
                       {
                           //Смотрим есть такой МКОП в таблице
                           int num = FindCodeInCodeTable(MKOP,arr_CodeTable);
                           if (num > -1)
                           {
                               //если он есть, то смотрим на длину команды
                               //ДЛИНА КОМАНДЫ = 1
                               //например NOP, операндов нет, а если и есть то не смотрим на них
                               if (arr_CodeTable[num,2]=="1")
                               {
                                   if (AddressCount +1> MAX_MemmoryAdr)
                                   {
                                       ErrorMessage = "Произошло переполнение";
                                       return false;
                                   }
                                   int AddressationType = Converts.HexToDec(arr_CodeTable[num, 1]) * 4;

                                   AddStringToSupportTable(Converts.ToSixChars(Converts.DecToHex(AddressCount)), 
                                                           Converts.ToTwoChars(Converts.DecToHex(AddressationType)), "", "");

                                   AddressCount = AddressCount + 1;

                                   if (!MemoryCheck()) 
                                   { return false; }

                                   if (Operand1.Length > 0 || Operand2.Length > 0)
                                       ErrorMessage = ErrorMessage + "Строка " + (i + 1) + ". Операнды не рассматриваются в команде " + arr_CodeTable[num, 0] + "\n";
                               }
                               else
                                   //ДЛИНА КОМАНДЫ = 2
                                   //ADD r1,r2   операнды это регистры, либо число //INT 200
                                   if (arr_CodeTable[num,2]=="2")
                                   {
                                       int number;
                                       //сначала пытаемся преобразовать первый операнд в число
                                       if (int.TryParse(Operand1, out number))
                                       {
                                           if (number >= 0 && number <= 255)
                                           {
                                               //так как операнд является числом, то это непосредственная адресация
                                               //просто  сдвигаем на два разряда влево
                                               int AddressationType = Converts.HexToDec(arr_CodeTable[num, 1]) * 4;

                                               if (AddressCount+2 > MAX_MemmoryAdr)
                                               {
                                                   ErrorMessage = "Произошло переполнение";
                                                   return false;
                                               }

                                               AddStringToSupportTable(Converts.ToSixChars(Converts.DecToHex(AddressCount)), 
                                                                       Converts.ToTwoChars(Converts.DecToHex(AddressationType)), Operand1, "");

                                               AddressCount = AddressCount + 2;

                                               if (!MemoryCheck()) 
                                               { return false; }

                                               if (Operand2.Length > 0)
                                                   ErrorMessage = ErrorMessage + "Строка " + (i + 1) + ". Второй операнд команды" + arr_CodeTable[num, 0] + " не рассматривается \n";
                                           }

                                           else
                                           {
                                               ErrorMessage = "Строка " + (i + 1) + ". Отрицательное число, либо превышено максимальное значение числа";
                                               return false;
                                           }
                                       }
                                       else
                                       {
                                           //если первый и второй операнд - регистры
                                           if (Check.OnlyRegisters(Operand1) && Check.OnlyRegisters(Operand2))
                                           {
                                               //так как оба операнда регистры то это регистровая(регистровой==непосредственной) адресация
                                               //просто  сдвигаем на два разряда влево
                                               int AddressationType = Converts.HexToDec(arr_CodeTable[num, 1]) * 4;
                                               if (AddressCount +2> MAX_MemmoryAdr)
                                               {
                                                   ErrorMessage = "Произошло переполнение";
                                                   return false;
                                               }
                                               AddStringToSupportTable(Converts.ToSixChars(Converts.DecToHex(AddressCount)), 
                                                                       Converts.ToTwoChars(Converts.DecToHex(AddressationType)), Operand1, Operand2);

                                               AddressCount = AddressCount + 2;

                                               if (!MemoryCheck())
                                               { return false; }
                                           }
                                           else
                                           {
                                               ErrorMessage = "Строка " + (i + 1) + ". Ошибка в команде " + arr_CodeTable[num, 0];
                                               return false;
                                           }

                                       }

                               }

                               else
                                       //ДЛИНА КОМАНДЫ = 3
                                  if (arr_CodeTable[num,2]=="3")
                                    {
                                        if (AddressCount +3> MAX_MemmoryAdr)
                                        {
                                            ErrorMessage = "Произошло переполнение";
                                            return false;
                                        }

                                        int AddressationType = Converts.HexToDec(arr_CodeTable[num, 1]) * 4 + 1;

                                        AddStringToSupportTable(Converts.ToSixChars(Converts.DecToHex(AddressCount)), 
                                                                Converts.ToTwoChars(Converts.DecToHex(AddressationType)), Operand1, Operand2);

                                        AddressCount = AddressCount + 3;

                                        if (!MemoryCheck()) 
                                        { return false; }

                                        if (Operand2.Length > 0)
                                            ErrorMessage = ErrorMessage + "Строка " + (i + 1) + ". Второй операнд команды" + arr_CodeTable[num, 0] + "  не рассматривается\n";

                               }
                                else
                                    //ДЛИНА КОМАНДЫ = 4
                                    if (arr_CodeTable[num, 2] == "4")
                                    {
                                        if (AddressCount+4 > MAX_MemmoryAdr)
                                        {
                                            ErrorMessage = "Произошло переполнение";
                                            return false;
                                        }

                                        int AddressationType = Converts.HexToDec(arr_CodeTable[num, 1]) * 4 + 1;

                                        AddStringToSupportTable(Converts.ToSixChars(Converts.DecToHex(AddressCount)), 
                                                                Converts.ToTwoChars(Converts.DecToHex(AddressationType)), Operand1, Operand2);

                                        AddressCount = AddressCount + 4;

                                        if (!MemoryCheck()) 
                                        { return false; }

                                        if (Operand2.Length > 0)
                                            ErrorMessage = ErrorMessage + "Строка " + (i + 1) + ". Второй операнд команды" + arr_CodeTable[num, 0] + " не рассматривается\n";
                                }
                                else
                                {
                                    ErrorMessage = "Строка " + (i + 1) + ". Размер команды больше установленного";
                                    return false;
                                }

                           }
                           else
                           {
                               ErrorMessage = "Строка " + (i + 1) + ". Мнемонический код операции не найден в таблице кодов операции. " + MKOP;
                               return false;
                           }
                       }
                       else
                       {
                           ErrorMessage = "Строка " + (i + 1) + ". Ошибка в мнемоническом коже операции";
                           return false;
                       }
                   }
               }
           }

           if (EndFlag == 0)
           {
               ErrorMessage = ErrorMessage + "Не найдена точка входа в программу \n";
               return false;
           }

           //Помещаем сформированную Вспомогательную таблицу в датагрид
           for (int i = 0; i < SupportTable[0].Count ; i++)
           {
               DG_SupportTable.Rows.Add();
               DG_SupportTable.Rows[i].Cells[0].Value = SupportTable[0][i];
               DG_SupportTable.Rows[i].Cells[1].Value = SupportTable[1][i];
               DG_SupportTable.Rows[i].Cells[2].Value = SupportTable[2][i];
               DG_SupportTable.Rows[i].Cells[3].Value = SupportTable[3][i];
           }

           //Помещаем сформированную Таблицу вспомогательных имен в датагрид
               for (int j = 0; j < SymbolNameTable[1].Count; j++)
               {
                   DG_SymbolTable.Rows.Add();
                   DG_SymbolTable.Rows[j].Cells[0].Value = SymbolNameTable[0][j];
                   DG_SymbolTable.Rows[j].Cells[1].Value = SymbolNameTable[1][j];
               }


               return true;
       }

        //проверка опреанда во втором проходе
        //Если в операнде метка - возращает адрес метки
        //Если в операнде регистр - возвращает номер регистра
        //Если там строка типа C"????" - возвращает ASCII код
        //Если там строка типа X"????" - возвращает строку
        //Если что-то в 10ричном формате - то вернет это же число в 16ричном формате
        //иначе возращает пустую строку
        public string CheckingOperandSecondPass(string operand1, out int error, out int label)
        {
            label = 0;
            string result = "";
            error = 0;
            if (operand1 != "")
            {
                //если там метка - то возвращаем адрес метки
                int LabelStringNum = FindLabelInLabelTable(operand1);
                if (LabelStringNum > -1)
                {
                    label = 1;
                    return result = SymbolNameTable[1][LabelStringNum];
                }
                else
                {
                    //если в операнде регистр
                    int regnum = Check.RegisterNumber(operand1);
                    if (regnum > -1)
                    {
                        return result = Converts.DecToHex(regnum);
                    }
                    else 
                    {
                        //если в операнде только цифры
                        if (Check.OnlyNumbers(operand1))
                        {
                            return result = Converts.DecToHex(Convert.ToInt32(operand1));
                        }
                        else
                        {
                            string sentence = "";
                            sentence = Check.String(operand1);
                            if (sentence != "")
                            {
                                return Converts.ToASCII(sentence);
                            }
                            sentence = Check.ByteString(operand1);
                            if (sentence != "")
                            {
                                return sentence;
                            }

                            //Если перепробованы все комбинации, значит ошибка
                            error = 1;
                        }
                    }      
                }
            }

            return result;
        }

        //Второй проход
        public bool Second_pass(ListBox BinaryCode)
       {
           ErrorMessage = "";
           //запускаем его для каждой строки Вспомогательной таблицы
           for (int i = 0; i < SupportTable[0].Count;i++ )
           {
              string address  = SupportTable[0][i];
              string MKOP     = SupportTable[1][i];
              string operand1 = SupportTable[2][i];
              string operand2 = SupportTable[3][i];
              
               //Если строка первая, то это директива Старт
               if (i == 0)
               {
                   string str = Converts.EditingString("H", SupportTable[0][0], SupportTable[2][0],"", 
                                                          Convert.ToString(AddressCount - StartAddress), "");
                   BinaryCode.Items.Add(str);
               }
               //Если строка не первая, то снова смотрим, команда там или директива. Интересуют RESB и  RESW, т.к. из значение операндов отражается только в длине записи
               //Если строка не первая, то снова смотрим, команда там или директива. Интересуют RESB и  RESW, т.к. из значение операндов отражается только в длине записи
               else
               {
                   int error,label1,label2;

                   string result1 = CheckingOperandSecondPass(operand1,out error,out label1);

                   if (error == 1) 
                   { ErrorMessage = ErrorMessage + "Строка " + (i + 1) + ". Ошибка в операнде, код отсутствует в таблице символических имен. " + operand1; break; }

                   string result2 = CheckingOperandSecondPass(operand2, out error, out label2);

                   if (error == 1) 
                   { ErrorMessage = ErrorMessage + "Строка " + (i + 1) + ". Ошибка в операнде, код отсутствует в таблице символических имен. " + operand2; break; }

                   if (Check.IsDirective(MKOP) == true)
                   {
                       if (MKOP == "RESB")
                       {
                           MKOP = "";
                           string str1 = Converts.EditingString("T", address, MKOP, "00", "", "");
                           BinaryCode.Items.Add(str1);
                           continue;
                       }

                       if (MKOP == "RESW")
                       {
                           MKOP = "";
                          // string str2 = Converting.EditingString("T", address, MKOP, Converting.ToTwoChars(Converting.DecToHex(Convert.ToInt32(operand1) * 3)), "", "");
                            string str2 = Converts.EditingString("T", address, MKOP, "00", "", "");
                            BinaryCode.Items.Add(str2);
                           continue;
                       }

                       if (MKOP == "BYTE")
                       {
                           MKOP = "";
                           string str2 = Converts.EditingString("T", address, MKOP, Converts.ToTwoChars(Converts.DecToHex(result1.Length + result2.Length)), result1, result2);
                           BinaryCode.Items.Add(str2);
                           continue;
                       }

                       if (MKOP == "WORD")
                       {
                           MKOP = "";
                           result1 = Converts.ToSixChars(result1);
                           string str2 = Converts.EditingString("T", address, MKOP, Converts.ToTwoChars(Converts.DecToHex(result1.Length + result2.Length)), result1, result2);
                           BinaryCode.Items.Add(str2);
                           continue;
                       }

                       if (MKOP == "BYTE" && operand1 == "?")
                       {
                           MKOP = "";
                           string str2 = Converts.EditingString("T", address, MKOP, Converts.ToTwoChars(Converts.DecToHex(1)), "", "");
                           BinaryCode.Items.Add(str2);
                           continue;
                       }

                       if (MKOP == "WORD" && operand1 == "?")
                       {
                           MKOP = "";
                           string str2 = Converts.EditingString("T", address, MKOP, Converts.ToTwoChars(Converts.DecToHex(3)), "", "");
                           BinaryCode.Items.Add(str2);
                           continue;
                       }
                   }
                   else
                   {
                        // Проверяем что команда работает с тем, что разрешено адресацией
                        // сначала смотрим на тип адресации, если там  01 , значит это прямая
                        //и в операндах может быть только метка
                       int Type_of_adr = (byte)Converts.HexToDec(MKOP) & 0x03;
                       if (Type_of_adr == 1)
                       {
                           if (label1 != 1)
                           {
                               ErrorMessage = ErrorMessage + "Строка " + (i + 1) + ". Для данного типа адресации операнд должен быть меткой " + MKOP;
                               BinaryCode.Items.Clear();
                               return false;
                               
                           }
                                if (result2 != "")
                                {
                                    ErrorMessage=ErrorMessage + "Строка " + (i + 1) + ". Данный тип адрессации поддерживает 1 операнд";
                                    BinaryCode.Items.Clear();
                                    return false;
                                }
                       }

                       String RecordLength = Converts.ToTwoChars(Converts.DecToHex(MKOP.Length + result1.Length + result2.Length));
                        // при инт b1 - и
                       string str5 = Converts.EditingString("T", address, MKOP, RecordLength, result1, result2);
                       BinaryCode.Items.Add(str5);
                   }
               }
           }
           string str3 = Converts.EditingString("E",Converts.ToSixChars(Converts.DecToHex(EndAddress)),"", "", "","");
           BinaryCode.Items.Add(str3);

           if (ErrorMessage!="")
           BinaryCode.Items.Clear();

           return true;
       }

    }
}
