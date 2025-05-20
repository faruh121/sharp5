using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Xsl;

namespace FileSystemAndXmlTasks
{
    // ===== Задание 9: Логгер =====
    public enum LogType { Error, Exception, Test, Info, Warning }

    public class Logger
    {
        private string _logFilePath;
        private string _configFilePath;
        private Dictionary<string, string> _config;

        public Logger(string configPath)
        {
            _configFilePath = configPath;
            _logFilePath = Path.Combine(Environment.CurrentDirectory, "app.log");
            LoadConfig();
        }

        private void LoadConfig()
        {
            _config = new Dictionary<string, string>();

            try
            {
                var lines = File.ReadAllLines(_configFilePath);
                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith(";")) continue;

                    var parts = line.Split('=');
                    if (parts.Length == 2)
                    {
                        _config[parts[0].Trim()] = parts[1].Trim();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки конфига: {ex.Message}");
                // Установки по умолчанию
                _config["DateTime"] = "true";
                _config["LogType"] = "true";
                _config["UserName"] = "true";
                _config["Message"] = "true";
                _config["Format"] = "[{DateTime}] {LogType} {UserName}: {Message}";
            }
        }

        public void Log(LogType type, string message)
        {
            try
            {
                var logEntry = _config["Format"];
                var userName = Environment.UserName;
                var now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                logEntry = logEntry.Replace("{DateTime}", _config["DateTime"] == "true" ? now : "")
                                 .Replace("{LogType}", _config["LogType"] == "true" ? $"[{type}]" : "")
                                 .Replace("{UserName}", _config["UserName"] == "true" ? userName : "")
                                 .Replace("{Message}", _config["Message"] == "true" ? message : "")
                                 .Replace("  ", " ") // Удаляем двойные пробелы
                                 .Trim();

                File.AppendAllText(_logFilePath, logEntry + Environment.NewLine);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка записи в лог: {ex.Message}");
            }
        }
    }

    // ===== Задание 10-11: Работа с XML =====
    public class BankExchangeRate
    {
        public string BankName { get; set; }
        public decimal BuyRate { get; set; }
        public decimal SellRate { get; set; }
    }

    public class Product
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }

    public class Order
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string Customer { get; set; }
        public List<Product> Products { get; set; } = new List<Product>();
    }

    public static class XmlTasks
    {
        // Задание 10.1: Экспорт курсов валют
        public static void ExportExchangeRates(string inputHtmlPath, string outputXmlPath)
        {
            try
            {
                // В реальности здесь будет парсинг HTML, но для примера создадим тестовые данные
                var rates = new List<BankExchangeRate>
                {
                    new BankExchangeRate { BankName = "ПриватБанк", BuyRate = 27.5m, SellRate = 28.1m },
                    new BankExchangeRate { BankName = "Ощадбанк", BuyRate = 27.6m, SellRate = 28.0m },
                    new BankExchangeRate { BankName = "Укргазбанк", BuyRate = 27.4m, SellRate = 28.2m }
                };

                using (var writer = XmlWriter.Create(outputXmlPath, new XmlWriterSettings { Indent = true }))
                {
                    writer.WriteStartDocument();
                    writer.WriteStartElement("ExchangeRates");

                    foreach (var rate in rates)
                    {
                        writer.WriteStartElement("Bank");
                        writer.WriteElementString("Name", rate.BankName);
                        writer.WriteElementString("BuyRate", rate.BuyRate.ToString());
                        writer.WriteElementString("SellRate", rate.SellRate.ToString());
                        writer.WriteEndElement();
                    }

                    writer.WriteEndElement();
                    writer.WriteEndDocument();
                }

                Console.WriteLine($"Курсы валют сохранены в {outputXmlPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка экспорта курсов: {ex.Message}");
            }
        }

        // Задание 10.2: Сохранение заказов в XML
        public static void SaveOrdersToXml(string outputXmlPath)
        {
            try
            {
                var orders = new List<Order>
                {
                    new Order
                    {
                        Id = 1,
                        Date = DateTime.Now,
                        Customer = "Иванов И.И.",
                        Products = new List<Product>
                        {
                            new Product { Name = "Ноутбук", Price = 25000, Quantity = 1 },
                            new Product { Name = "Мышь", Price = 500, Quantity = 2 }
                        }
                    },
                    new Order
                    {
                        Id = 2,
                        Date = DateTime.Now.AddDays(-1),
                        Customer = "Петров П.П.",
                        Products = new List<Product>
                        {
                            new Product { Name = "Монитор", Price = 8000, Quantity = 3 }
                        }
                    }
                };

                using (var writer = XmlWriter.Create(outputXmlPath, new XmlWriterSettings { Indent = true }))
                {
                    writer.WriteStartDocument();
                    writer.WriteStartElement("Orders");

                    foreach (var order in orders)
                    {
                        writer.WriteStartElement("Order");
                        writer.WriteAttributeString("Id", order.Id.ToString());
                        writer.WriteElementString("Date", order.Date.ToString("yyyy-MM-dd"));
                        writer.WriteElementString("Customer", order.Customer);

                        writer.WriteStartElement("Products");
                        foreach (var product in order.Products)
                        {
                            writer.WriteStartElement("Product");
                            writer.WriteElementString("Name", product.Name);
                            writer.WriteElementString("Price", product.Price.ToString());
                            writer.WriteElementString("Quantity", product.Quantity.ToString());
                            writer.WriteEndElement();
                        }
                        writer.WriteEndElement();

                        writer.WriteEndElement();
                    }

                    writer.WriteEndElement();
                    writer.WriteEndDocument();
                }

                Console.WriteLine($"Заказы сохранены в {outputXmlPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка сохранения заказов: {ex.Message}");
            }
        }

        // Задание 11.3: XSLT преобразование
        public static void TransformXmlToHtml(string xmlPath, string xsltPath, string outputHtmlPath)
        {
            try
            {
                var transform = new XslCompiledTransform();
                transform.Load(xsltPath);
                transform.Transform(xmlPath, outputHtmlPath);
                Console.WriteLine($"HTML документ создан: {outputHtmlPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка XSLT преобразования: {ex.Message}");
            }
        }

        // Задание 11.4: Чтение XML
        public static void ReadAndDisplayXml(string xmlPath)
        {
            try
            {
                Console.WriteLine("\nЧтение XML с помощью XmlDocument:");
                var xmlDoc = new XmlDocument();
                xmlDoc.Load(xmlPath);
                Console.WriteLine(xmlDoc.OuterXml);

                Console.WriteLine("\nЧтение XML с помощью XmlTextReader:");
                using (var reader = new XmlTextReader(xmlPath))
                {
                    while (reader.Read())
                    {
                        if (reader.NodeType == XmlNodeType.Element)
                        {
                            Console.WriteLine($"Элемент: {reader.Name}");
                            if (reader.HasAttributes)
                            {
                                while (reader.MoveToNextAttribute())
                                {
                                    Console.WriteLine($"  Атрибут: {reader.Name}={reader.Value}");
                                }
                            }
                        }
                        else if (reader.NodeType == XmlNodeType.Text)
                        {
                            Console.WriteLine($"  Значение: {reader.Value}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка чтения XML: {ex.Message}");
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // Создаем конфиг для логгера
            File.WriteAllText("logger.ini",
                @"[Settings]
                DateTime=true
                LogType=true
                UserName=true
                Message=true
                Format=[{DateTime}] {LogType} {UserName}: {Message}");

            // Инициализация логгера
            var logger = new Logger("logger.ini");

            Console.WriteLine("Выберите задание:");
            Console.WriteLine("1. Логгер (задание 9)");
            Console.WriteLine("2. Экспорт курсов валют (задание 10.1)");
            Console.WriteLine("3. Сохранение заказов в XML (задание 10.2)");
            Console.WriteLine("4. XSLT преобразование (задание 11.3)");
            Console.WriteLine("5. Чтение XML (задание 11.4)");
            Console.Write("Ваш выбор: ");

            var choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    logger.Log(LogType.Info, "Программа запущена");
                    logger.Log(LogType.Warning, "Это тестовое предупреждение");
                    logger.Log(LogType.Error, "Произошла ошибка");
                    Console.WriteLine("Логи записаны в app.log");
                    break;

                case "2":
                    XmlTasks.ExportExchangeRates("input.html", "exchange_rates.xml");
                    break;

                case "3":
                    XmlTasks.SaveOrdersToXml("orders.xml");
                    break;

                case "4":
                    // Создаем простой XSLT для преобразования
                    File.WriteAllText("orders.xslt",
                        @"<?xml version='1.0'?>
                        <xsl:stylesheet version='1.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
                        <xsl:template match='/'>
                            <html>
                            <body>
                                <h2>Список заказов</h2>
                                <xsl:for-each select='Orders/Order'>
                                <div style='border:1px solid #ccc; margin:10px; padding:10px;'>
                                    <h3>Заказ #<xsl:value-of select='@Id'/></h3>
                                    <p>Клиент: <xsl:value-of select='Customer'/></p>
                                    <p>Дата: <xsl:value-of select='Date'/></p>
                                    <table border='1'>
                                    <tr><th>Товар</th><th>Цена</th><th>Количество</th></tr>
                                    <xsl:for-each select='Products/Product'>
                                        <tr>
                                        <td><xsl:value-of select='Name'/></td>
                                        <td><xsl:value-of select='Price'/></td>
                                        <td><xsl:value-of select='Quantity'/></td>
                                        </tr>
                                    </xsl:for-each>
                                    </table>
                                </div>
                                </xsl:for-each>
                            </body>
                            </html>
                        </xsl:template>
                        </xsl:stylesheet>");

                    XmlTasks.TransformXmlToHtml("orders.xml", "orders.xslt", "orders.html");
                    break;

                case "5":
                    XmlTasks.ReadAndDisplayXml("orders.xml");
                    break;

                default:
                    Console.WriteLine("Неверный выбор!");
                    break;
            }

            logger.Log(LogType.Info, "Программа завершена");
        }
    }
}