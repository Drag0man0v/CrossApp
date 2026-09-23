
using System.Runtime.InteropServices;
using System.Text.Json;

var info = new
{
    OSDescription = RuntimeInformation.OSDescription,
    OSVersion = Environment.OSVersion.ToString(),
    Architecture = RuntimeInformation.ProcessArchitecture.ToString(),
    DotnetVersion = Environment.Version.ToString(),
    Runtime = RuntimeInformation.FrameworkDescription,
    BaseDirectory = AppContext.BaseDirectory,
    CurrentDirectory = Environment.CurrentDirectory
};

if (args.Contains("--json"))
{
    Console.WriteLine(JsonSerializer.Serialize(info));
}
else
{
 Console.OutputEncoding = System.Text.Encoding.UTF8;
 Console.WriteLine("CrossApp – практикум з крос-платформного програмування");
 Console.WriteLine("Студент: Суховерський Ростислав, група ФЕІ-36");
 Console.WriteLine(new string('-', 52));
 Console.WriteLine($"ОС (OSDescription) : {RuntimeInformation.OSDescription}");
 Console.WriteLine($"ОС (Environment) : {Environment.OSVersion}");
 Console.WriteLine($"Архітектура процесу : {RuntimeInformation.ProcessArchitecture}");
 Console.WriteLine($"Версія .NET (CLR) : {Environment.Version}");
 Console.WriteLine($"Runtime : {RuntimeInformation.FrameworkDescription}");
 Console.WriteLine($"Каталог застосунку : {AppContext.BaseDirectory}");
 Console.WriteLine($"Поточний каталог : {Environment.CurrentDirectory}");
 Console.WriteLine(new string('-', 52));
 Console.WriteLine("Предметна область: Бібліотека (Book (книга), BookCopy (примірник), Reader (читач), Loan (видача))");
}