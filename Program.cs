
using System.Data;
using System.Data.SqlClient;
using System.Text;


internal class Program
{
    private static async Task Main(string[] args)
    {
        var userInput = "";
        var connectionString = @"Data Source=.\;Initial Catalog=testdb;Integrated Security=True";

        await GenerateFiles();

        // цикл жизни программы
        while (userInput != "0")
        {
            Console.WriteLine("2 - Объединить файлы, 3 - Импорт в БД, 4 - Выполнение хранимой процедуры, 0 - Выход");
            userInput = Console.ReadLine();

            // объединение файлов в один, с возможностью удаления строк из них
            if (userInput == "2")
            {
                await MergingFiles();
            }

            // процедура импорта файлов в таблицу в БД
            if (userInput == "3")
            {
                ImportToDB(connectionString);
            }

            // выполнение хранимой процедуры с нахождением суммы и медианы
            if (userInput == "4")
            {
                ExStoredProcedure(connectionString);
            }
        }

        // генерация 100 файлов
        async Task GenerateFiles()
        {
            try
            {
                var rnd = new Random();

                Console.WriteLine("Начало генерации " + DateTime.Now);

                // создание 100 текстовых файлов
                for (int fileNumber = 1; fileNumber <= 100; fileNumber++)
                {

                    var genString = new StringBuilder();

                    // создание 100 000 строк
                    for (int ctr = 0; ctr < 100000; ctr++)
                    {

                        // генерация даты
                        var thisDay = DateTime.Today;
                        genString.Append(thisDay.AddDays(rnd.Next((-365 * 5)-1, 0)).ToString("d") + "||"); 

                        // генерация латинских символов
                        var latSymbols = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
                        for (int i = 0; i < 10; i++)
                        {

                            genString.Append(latSymbols[rnd.Next(latSymbols.Length)]);

                        }
                        genString.Append("||");

                        // генерация русских символов
                        var ruSymbols = "абвгдеёжзийклмнопрстуфхцчшщъыьэюяАБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯ";
                        for (int i = 0; i < 10; i++)
                        {

                            genString.Append(ruSymbols[rnd.Next(ruSymbols.Length)]);

                        }
                        genString.Append("||");

                        // генерация четного целочисленного числа от 1 до 1 000 000 000
                        genString.Append(rnd.Next(0, 50000000) * 2 + "||");

                        // генерация положительного числа с 8 знаками после запятой в диапазоне от 1 до 20
                        genString.AppendLine(string.Format("{0:f8}", rnd.NextDouble() * (20.0 - 1.0) + 1.0) + "||");

                    }

                    // запись в файл 100 000 строк
                    using (StreamWriter writer = new StreamWriter(fileNumber + ".txt"))
                    {
                        await writer.WriteAsync(genString);
                    }

                }
                Console.WriteLine("Конец генерации " + DateTime.Now);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }

        // объединение файлов в один, с возможностью удаления строк из них
        async Task MergingFiles()
        {
            try
            {
                Console.Write("Введите строки, которые хотите удалить: ");
                var deleteCol = Console.ReadLine();
                var delRowsCount = 0;
                var stringBuilder = new StringBuilder();

                for (int fileNumber = 1; fileNumber <= 100; fileNumber++)
                {
                    using (StreamReader reader = new StreamReader(fileNumber + ".txt"))
                    {
                        string[] lines = File.ReadAllLines(fileNumber + ".txt", Encoding.Default);

                        foreach (string line in lines)
                        {
                            if (deleteCol !="")
                            {
                                if (!line.Contains(deleteCol))
                                    stringBuilder.AppendLine(line);
                                else
                                    delRowsCount++;
                            }
                        }
                    }

                }

                // создание нового файла
                using (StreamWriter writer = new StreamWriter("United.txt"))
                {
                    await writer.WriteAsync(stringBuilder);
                }

                Console.WriteLine($"Количество удаленных строк: {delRowsCount}");
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        // процедура импорта файлов в таблицу в БД
        void ImportToDB(string connectionString)
        {
            try
            {
                int rowsCount = 0;

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // формирование sql запроса
                    SqlCommand command = new SqlCommand();
                    command.Connection = connection;
                    command.CommandText = @"INSERT INTO Task1Output VALUES (@dateGen, @latSymb, @ruSymb, @evenNumb, @divisNumb)";
                    command.Parameters.Add("@dateGen", SqlDbType.NVarChar, 50);
                    command.Parameters.Add("@latSymb", SqlDbType.NVarChar, 50);
                    command.Parameters.Add("@ruSymb", SqlDbType.NVarChar, 50);
                    command.Parameters.Add("@evenNumb", SqlDbType.BigInt);
                    command.Parameters.Add("@divisNumb", SqlDbType.Float);

                    for (int fileNumber = 1; fileNumber <= 100; fileNumber++)
                    {
                        // получение данных из каждого файла
                        using (StreamReader reader = new StreamReader(fileNumber + ".txt"))
                        {
                            string[] lines = File.ReadAllLines(fileNumber + ".txt", Encoding.Default);

                            foreach (string line in lines)
                            {
                                string[] elements = line.Split("||");

                                //command.Connection = connection;
                                command.Parameters["@dateGen"].Value = elements[0];
                                command.Parameters["@latSymb"].Value = elements[1];
                                command.Parameters["@ruSymb"].Value = elements[2];
                                command.Parameters["@evenNumb"].Value = elements[3];
                                command.Parameters["@divisNumb"].Value = elements[4];

                                command.ExecuteNonQuery();
                                rowsCount++;
                                Console.WriteLine($"Строк обработано: {rowsCount}. Строк осталось: {100000 * 100 - rowsCount}");
                                Console.WriteLine();
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        // выполнение хранимой процедуры с нахождением суммы и медианы
        void ExStoredProcedure(string connectionString)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {

                    connection.Open();
                    string sqlExpression = "GetSumAndMediane";

                    SqlCommand command = new SqlCommand(sqlExpression, connection);

                    command.CommandType = CommandType.StoredProcedure;

                    SqlParameter sumParam = new SqlParameter
                    {
                        ParameterName = "@sum",
                        SqlDbType = SqlDbType.BigInt,
                        Direction = ParameterDirection.Output
                    };
                    command.Parameters.Add(sumParam);

                    SqlParameter medianeParam = new SqlParameter
                    {
                        ParameterName = "@mediane",
                        SqlDbType = SqlDbType.Float,
                        Direction = ParameterDirection.Output
                    };
                    command.Parameters.Add(medianeParam);

                    command.ExecuteNonQuery();
                    var sum = command.Parameters["@sum"].Value;
                    var mediane = command.Parameters["@mediane"].Value;
                    Console.WriteLine($"Сумма всех целых чисел: {sum}");
                    Console.WriteLine($"Медиана всех дробных чисел: {mediane}");
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

    }
}