using Core.Domain;

namespace Core;

public static class SampleData
{
    public static List<BookCopy> Books()
    {
        List<BookCopy> books = new List<BookCopy>();

        books.Add(BookCopy.Create(
            "C-001",
            "978-001",
            "Кобзар",
            "Тарас Шевченко",
            1840,
            "Поезія"
        ));

        books.Add(BookCopy.Create(
            "C-002",
            "978-002",
            "Лісова пісня",
            "Леся Українка",
            1911,
            "Драма"
        ));

        books.Add(BookCopy.Create(
            "C-003",
            "978-003",
            "Захар Беркут",
            "Іван Франко",
            1883,
            "Повість"
        ));

        books.Add(BookCopy.Create(
            "C-004",
            "978-004",
            "Тигролови",
            "Іван Багряний",
            1944,
            "Роман"
        ));

        books.Add(BookCopy.Create(
            "C-005",
            "978-005",
            "Місто",
            "Валер'ян Підмогильний",
            1928,
            "Роман"
        ));

        books.Add(BookCopy.Create(
            "C-006",
            "978-006",
            "Кайдашева сім'я",
            "Іван Нечуй-Левицький",
            1879,
            "Повість"
        ));

        books.Add(BookCopy.Create(
            "C-007",
            "978-007",
            "Тіні забутих предків",
            "Михайло Коцюбинський",
            1911,
            "Повість"
        ));

        books.Add(BookCopy.Create(
            "C-008",
            "978-008",
            "Маруся Чурай",
            "Ліна Костенко",
            1979,
            "Роман"
        ));

        books.Add(BookCopy.Create(
            "C-009",
            "978-009",
            "Енеїда",
            "Іван Котляревський",
            1798,
            "Поема"
        ));

        books.Add(BookCopy.Create(
            "C-010",
            "978-010",
            "Чорна рада",
            "Пантелеймон Куліш",
            1857,
            "Роман"
        ));

        books.Add(BookCopy.Create(
            "C-011",
            "978-011",
            "Три товариші",
            "Еріх Марія Ремарк",
            1936,
            "Роман"
        ));

        books.Add(BookCopy.Create(
            "C-012",
            "978-012",
            "Маленький принц",
            "Антуан де Сент-Екзюпері",
            1943,
            "Казка"
        ));

        books.Add(BookCopy.Create(
            "C-013",
            "978-013",
            "1984",
            "Джордж Орвелл",
            1949,
            "Роман"
        ));

        books.Add(BookCopy.Create(
            "C-014",
            "978-014",
            "Гобіт",
            "Джон Толкін",
            1937,
            "Фентезі"
        ));

        books.Add(BookCopy.Create(
            "C-015",
            "978-015",
            "Портрет Доріана Грея",
            "Оскар Вайлд",
            1890,
            "Роман"
        ));

        return books;
    }
}