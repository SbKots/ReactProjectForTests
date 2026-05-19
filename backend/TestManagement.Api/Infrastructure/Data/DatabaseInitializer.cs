using Microsoft.EntityFrameworkCore;
using TestManagement.Api.Domain.Entities;
using TestManagement.Api.Domain.Enums;

namespace TestManagement.Api.Infrastructure.Data;

public static class DatabaseInitializer
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await dbContext.Database.EnsureCreatedAsync();

        var legacySamples = await dbContext.Tests
            .Where(test => test.Title == "ASP.NET Core basics")
            .ToListAsync();

        if (legacySamples.Count > 0)
        {
            dbContext.Tests.RemoveRange(legacySamples);
        }

        var existingTitles = await dbContext.Tests
            .Select(test => test.Title)
            .ToListAsync();

        foreach (var sampleTest in CreateSampleTests())
        {
            if (!existingTitles.Contains(sampleTest.Title))
            {
                dbContext.Tests.Add(sampleTest);
            }
        }

        if (dbContext.ChangeTracker.HasChanges())
        {
            await dbContext.SaveChangesAsync();
        }
    }

    private static IReadOnlyList<TestDefinition> CreateSampleTests()
    {
        return
        [
            CreateSimpleTest(
                "Математика: разминка",
                "Короткий тест на простые вычисления и базовые числа.",
                ("Сколько будет 2 + 2?", QuestionType.SingleChoice, [("3", false), ("4", true), ("5", false)]),
                ("Какие числа являются четными?", QuestionType.MultipleChoice, [("2", true), ("3", false), ("4", true), ("5", false)]),
                ("Сколько минут в одном часе?", QuestionType.SingleChoice, [("30", false), ("60", true), ("100", false)])),

            CreateSimpleTest(
                "Общие знания",
                "Несколько легких вопросов для проверки прохождения тестов.",
                ("Столица России?", QuestionType.SingleChoice, [("Москва", true), ("Казань", false), ("Новосибирск", false)]),
                ("Какие слова обозначают цвета?", QuestionType.MultipleChoice, [("Красный", true), ("Стол", false), ("Синий", true), ("Бежать", false)]),
                ("Какая планета ближе всего к Солнцу?", QuestionType.SingleChoice, [("Венера", false), ("Меркурий", true), ("Марс", false)])),

            CreateSimpleTest(
                "Русский язык: слова",
                "Простые вопросы по словам и частям речи.",
                ("Какое слово является существительным?", QuestionType.SingleChoice, [("Дом", true), ("Быстро", false), ("Красивый", false)]),
                ("Какие слова являются глаголами?", QuestionType.MultipleChoice, [("Идти", true), ("Книга", false), ("Писать", true), ("Синий", false)]),
                ("Сколько букв в слове \"кот\"?", QuestionType.SingleChoice, [("2", false), ("3", true), ("4", false)])),

            CreateSimpleTest(
                "География: страны",
                "Легкий тест по странам, городам и материкам.",
                ("Столица Франции?", QuestionType.SingleChoice, [("Париж", true), ("Рим", false), ("Берлин", false)]),
                ("Какие города находятся в России?", QuestionType.MultipleChoice, [("Москва", true), ("Самара", true), ("Лондон", false), ("Париж", false)]),
                ("Самый большой океан?", QuestionType.SingleChoice, [("Тихий", true), ("Индийский", false), ("Северный Ледовитый", false)])),

            CreateSimpleTest(
                "История: простые даты",
                "Несколько базовых исторических вопросов.",
                ("В каком году началась Великая Отечественная война?", QuestionType.SingleChoice, [("1941", true), ("1939", false), ("1945", false)]),
                ("Какие предметы относятся к истории?", QuestionType.MultipleChoice, [("Документ", true), ("Летопись", true), ("Микроскоп", false), ("Монета", true)]),
                ("Кто был первым космонавтом?", QuestionType.SingleChoice, [("Юрий Гагарин", true), ("Алексей Леонов", false), ("Сергей Королев", false)])),

            CreateSimpleTest(
                "Биология: человек",
                "Простые вопросы о человеке и живой природе.",
                ("Какой орган качает кровь?", QuestionType.SingleChoice, [("Сердце", true), ("Легкое", false), ("Желудок", false)]),
                ("Что нужно растениям для роста?", QuestionType.MultipleChoice, [("Свет", true), ("Вода", true), ("Камень", false), ("Воздух", true)]),
                ("Чем человек дышит?", QuestionType.SingleChoice, [("Кислородом", true), ("Песком", false), ("Сахаром", false)])),

            CreateSimpleTest(
                "Информатика: основы",
                "Базовые вопросы про компьютер и файлы.",
                ("Что является устройством ввода?", QuestionType.SingleChoice, [("Клавиатура", true), ("Монитор", false), ("Колонка", false)]),
                ("Какие расширения обычно относятся к изображениям?", QuestionType.MultipleChoice, [(".png", true), (".jpg", true), (".exe", false), (".gif", true)]),
                ("Что хранит файлы на компьютере?", QuestionType.SingleChoice, [("Диск", true), ("Мышь", false), ("Клавиатура", false)])),

            CreateSimpleTest(
                "Английский язык: слова",
                "Проверка самых простых английских слов.",
                ("Как переводится слово cat?", QuestionType.SingleChoice, [("Кот", true), ("Дом", false), ("Солнце", false)]),
                ("Какие слова обозначают животных?", QuestionType.MultipleChoice, [("Dog", true), ("Book", false), ("Cat", true), ("Pen", false)]),
                ("Как переводится red?", QuestionType.SingleChoice, [("Красный", true), ("Синий", false), ("Белый", false)])),

            CreateSimpleTest(
                "Литература: герои",
                "Легкие вопросы по известным произведениям.",
                ("Кто написал сказку о рыбаке и рыбке?", QuestionType.SingleChoice, [("Пушкин", true), ("Толстой", false), ("Чехов", false)]),
                ("Какие персонажи бывают в сказках?", QuestionType.MultipleChoice, [("Царь", true), ("Волшебник", true), ("Программист", false), ("Богатырь", true)]),
                ("Что обычно есть у книги?", QuestionType.SingleChoice, [("Обложка", true), ("Колесо", false), ("Клавиша", false)])),

            CreateSimpleTest(
                "Физика: явления",
                "Простые вопросы о явлениях вокруг нас.",
                ("Что измеряют термометром?", QuestionType.SingleChoice, [("Температуру", true), ("Скорость", false), ("Вес", false)]),
                ("Какие явления связаны со светом?", QuestionType.MultipleChoice, [("Тень", true), ("Отражение", true), ("Эхо", false), ("Радуга", true)]),
                ("Что падает вниз из-за силы тяжести?", QuestionType.SingleChoice, [("Камень", true), ("Звук", false), ("Запах", false)])),

            CreateSimpleTest(
                "Химия: вещества",
                "Очень простой тест по веществам.",
                ("Формула воды?", QuestionType.SingleChoice, [("H2O", true), ("CO2", false), ("O2", false)]),
                ("Какие вещества можно встретить дома?", QuestionType.MultipleChoice, [("Вода", true), ("Соль", true), ("Сахар", true), ("Лава", false)]),
                ("Какой газ нужен человеку для дыхания?", QuestionType.SingleChoice, [("Кислород", true), ("Гелий", false), ("Азот", false)])),

            CreateSimpleTest(
                "Музыка: инструменты",
                "Простые вопросы про музыкальные инструменты.",
                ("На чем играют клавишами?", QuestionType.SingleChoice, [("Пианино", true), ("Барабан", false), ("Скрипка", false)]),
                ("Какие из них музыкальные инструменты?", QuestionType.MultipleChoice, [("Гитара", true), ("Флейта", true), ("Ложка", false), ("Барабан", true)]),
                ("Что обычно есть у гитары?", QuestionType.SingleChoice, [("Струны", true), ("Колеса", false), ("Экран", false)])),

            CreateSimpleTest(
                "Спорт: правила",
                "Небольшой тест по популярным видам спорта.",
                ("Сколько ворот на футбольном поле?", QuestionType.SingleChoice, [("2", true), ("1", false), ("4", false)]),
                ("Какие игры командные?", QuestionType.MultipleChoice, [("Футбол", true), ("Хоккей", true), ("Шахматы один на один", false), ("Волейбол", true)]),
                ("Чем играют в баскетбол?", QuestionType.SingleChoice, [("Мячом", true), ("Шайбой", false), ("Ракеткой", false)])),

            CreateSimpleTest(
                "Искусство: цвета",
                "Легкие вопросы о цветах и рисовании.",
                ("Какой цвет получится из красного и синего?", QuestionType.SingleChoice, [("Фиолетовый", true), ("Зеленый", false), ("Оранжевый", false)]),
                ("Какие предметы нужны для рисования?", QuestionType.MultipleChoice, [("Карандаш", true), ("Кисть", true), ("Краски", true), ("Кирпич", false)]),
                ("На чем обычно рисуют?", QuestionType.SingleChoice, [("Бумага", true), ("Вода", false), ("Песок в стакане", false)])),

            CreateSimpleTest(
                "Безопасность в интернете",
                "Простые вопросы о безопасном поведении онлайн.",
                ("Можно ли сообщать пароль незнакомцам?", QuestionType.SingleChoice, [("Нет", true), ("Да", false), ("Только вечером", false)]),
                ("Какие пароли лучше?", QuestionType.MultipleChoice, [("Длинные", true), ("С разными символами", true), ("12345", false), ("Уникальные", true)]),
                ("Что делать с подозрительной ссылкой?", QuestionType.SingleChoice, [("Не открывать", true), ("Сразу переслать всем", false), ("Ввести пароль", false)])),

            CreateSimpleTest(
                "Экология: природа",
                "Простые вопросы о природе и бережном отношении к ней.",
                ("Куда лучше выбрасывать батарейки?", QuestionType.SingleChoice, [("В специальный пункт сбора", true), ("В лес", false), ("В реку", false)]),
                ("Что помогает природе?", QuestionType.MultipleChoice, [("Сортировка мусора", true), ("Экономия воды", true), ("Мусор на улице", false), ("Посадка деревьев", true)]),
                ("Что выделяют растения?", QuestionType.SingleChoice, [("Кислород", true), ("Пластик", false), ("Стекло", false)])),

            CreateSimpleTest(
                "Логика: простые задачи",
                "Несколько легких логических вопросов.",
                ("Что идет после числа 9?", QuestionType.SingleChoice, [("10", true), ("8", false), ("19", false)]),
                ("Какие числа больше 5?", QuestionType.MultipleChoice, [("6", true), ("7", true), ("4", false), ("10", true)]),
                ("Если сегодня понедельник, завтра будет...", QuestionType.SingleChoice, [("Вторник", true), ("Среда", false), ("Воскресенье", false)])),

            CreateSimpleTest(
                "Кухня: продукты",
                "Легкий тест про продукты и кухню.",
                ("Из чего делают хлеб?", QuestionType.SingleChoice, [("Мука", true), ("Стекло", false), ("Металл", false)]),
                ("Какие продукты обычно хранят в холодильнике?", QuestionType.MultipleChoice, [("Молоко", true), ("Сыр", true), ("Мороженое", true), ("Книга", false)]),
                ("Что кипятят в чайнике?", QuestionType.SingleChoice, [("Воду", true), ("Песок", false), ("Масло для машины", false)])),

            CreateSimpleTest(
                "Кино: жанры",
                "Простые вопросы о фильмах.",
                ("Комедия обычно должна...", QuestionType.SingleChoice, [("Смешить", true), ("Пугать", false), ("Учить математике", false)]),
                ("Какие бывают жанры кино?", QuestionType.MultipleChoice, [("Комедия", true), ("Драма", true), ("Фантастика", true), ("Отвертка", false)]),
                ("Где обычно показывают фильмы?", QuestionType.SingleChoice, [("В кинотеатре", true), ("В аптеке", false), ("На заправке", false)])),

            CreateSimpleTest(
                "Здоровье: привычки",
                "Легкие вопросы о полезных привычках.",
                ("Что полезно делать утром?", QuestionType.SingleChoice, [("Умываться", true), ("Есть грязными руками", false), ("Не спать неделю", false)]),
                ("Какие привычки полезные?", QuestionType.MultipleChoice, [("Мыть руки", true), ("Пить воду", true), ("Двигаться", true), ("Не чистить зубы", false)]),
                ("Сколько раз в день обычно чистят зубы?", QuestionType.SingleChoice, [("2", true), ("0", false), ("10", false)])),

            CreateSimpleTest(
                "Транспорт: правила",
                "Простые вопросы о транспорте и дороге.",
                ("На какой сигнал светофора можно идти?", QuestionType.SingleChoice, [("Зеленый", true), ("Красный", false), ("Желтый мигающий всегда", false)]),
                ("Какие виды транспорта общественные?", QuestionType.MultipleChoice, [("Автобус", true), ("Трамвай", true), ("Метро", true), ("Личная кружка", false)]),
                ("Где пешеходу переходить дорогу?", QuestionType.SingleChoice, [("По переходу", true), ("Где угодно", false), ("На повороте без обзора", false)])),

            CreateSimpleTest(
                "Астрономия: планеты",
                "Небольшой тест о космосе.",
                ("На какой планете мы живем?", QuestionType.SingleChoice, [("Земля", true), ("Марс", false), ("Юпитер", false)]),
                ("Какие объекты есть в космосе?", QuestionType.MultipleChoice, [("Звезды", true), ("Планеты", true), ("Кометы", true), ("Деревянные стулья", false)]),
                ("Что является спутником Земли?", QuestionType.SingleChoice, [("Луна", true), ("Солнце", false), ("Венера", false)]))
        ];
    }

    private static TestDefinition CreateSimpleTest(
        string title,
        string description,
        params (string Text, QuestionType Type, (string Text, bool IsCorrect)[] Options)[] questions)
    {
        var test = new TestDefinition
        {
            Title = title,
            Description = description
        };

        for (var questionIndex = 0; questionIndex < questions.Length; questionIndex++)
        {
            var question = questions[questionIndex];
            test.Questions.Add(CreateQuestion(test, question.Text, question.Type, questionIndex, question.Options));
        }

        return test;
    }

    private static Question CreateQuestion(
        TestDefinition test,
        string text,
        QuestionType type,
        int sortOrder,
        params (string Text, bool IsCorrect)[] options)
    {
        var question = new Question
        {
            TestDefinition = test,
            Text = text,
            Type = type,
            SortOrder = sortOrder
        };

        for (var optionIndex = 0; optionIndex < options.Length; optionIndex++)
        {
            question.Options.Add(new AnswerOption
            {
                Question = question,
                Text = options[optionIndex].Text,
                IsCorrect = options[optionIndex].IsCorrect,
                SortOrder = optionIndex
            });
        }

        return question;
    }
}
