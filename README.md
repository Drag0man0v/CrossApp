# CrossApp

Наскрізний проєкт з крос-платформного програмування.

Предметна область: Бібліотека.
Сутності: Book (книга), BookCopy (примірник), Reader (читач), Loan (видача).
Призначення: облік видач примірників книг читачам і повернень.

## Структура solution
```
CrossApp/
├── CrossApp.slnx
├── README.md
├── .gitignore
└── src/
    ├── Core/
    │   ├── Core.csproj
    │   ├── EnvironmentInfo.cs
    │   ├── Dto/
    │   │   └── .gitkeep
    │   ├── Domain/
    │   │   └── .gitkeep
    │   └── Storage/
    │       └── .gitkeep
    └── Cli/
        ├── Cli.csproj
        └── Program.cs
```


## Різниця між self-contained і framework-dependent

**Framework-dependent** публікація містить лише код застосунку (`Cli.dll`, `Core.dll`) і його залежності, без середовища виконання .NET. Каталог дуже малий, але на машині користувача має бути встановлений сумісний .NET Runtime. Якщо його немає, застосунок не запуститься.

**Self-contained** публікація містить код застосунку разом із копією .NET Runtime. Застосунок працює на комп'ютері без встановленого .NET, але каталог значно більший (76.85 МБ, 194 файли).

## Запуск
dotnet build
dotnet run --project src/Cli

# publish: self-contained
dotnet publish src/Cli -c Release -r win-x64 --self-contained true -o out/sc

# publish: framework-dependent
dotnet publish src/Cli -c Release -r win-x64 --self-contained false -o out/fd

# запуск із каталогу publish
./out/sc/Cli.exe
./out/fd/Cli.exe

----------------------------------------------------------------------------------
| RID     | Режим               | Розмір publish | Потрібен встановлений runtime |
|---------|---------------------|----------------|-------------------------------|
| win-x64 | self-contained      | 76.85 МБ       | ні                            |
| win-x64 | framework-dependent | 0.19 МБ        | так (.NET 10)                 |

## Середовище
.NET SDK 10.0, Windows 11 x64 

## Порівняння self-contained публікацій
Ну шо можу сказати, по вазі каталогів вийшло, шо під лінукс воно займає трошки більше: 157 мб, у той час як під віндовс - 153
