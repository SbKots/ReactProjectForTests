# Test Management

CRUD-система для управления тестами и прохождения тестов с автоматическим подсчетом баллов.

## Стек

- Backend: ASP.NET Core 8, EF Core, SQLite
- Frontend: React + Vite
- Desktop: Tauri
- Tests: MSTest

## Что реализовано

- Создание, просмотр, редактирование и удаление тестов.
- Вопросы типов `SingleChoice` и `MultipleChoice`.
- DTO на границе API, сущности БД наружу не отдаются.
- Слои backend: Controllers -> Services -> Repositories.
- Repository pattern, DI-регистрация в `Program.cs`, async/await.
- SQLite создается автоматически при первом запуске.
- Подсчет результата в дробном и процентном форматах.

## Запуск backend

```bash
dotnet restore
dotnet run --project backend/TestManagement.Api
```

API будет доступен по адресу:

```text
http://localhost:5088/api/tests
```

## Быстрый запуск без npm

Если на компьютере нет Node.js/npm, можно запустить ASP.NET Core в fullstack-режиме. Он отдаст встроенную рабочую страницу и API с одного адреса:

```bash
dotnet run --project backend/TestManagement.Api --launch-profile fullstack
```

Открыть:

```text
http://127.0.0.1:1420
```

## Запуск frontend как веб-приложения

```bash
cd frontend
npm install
npm run dev
```

Frontend будет доступен по адресу:

```text
http://127.0.0.1:1420
```

Если backend запущен на другом адресе, задайте переменную:

```bash
VITE_API_BASE_URL=http://localhost:5088/api npm run dev
```

## Запуск через Tauri

Нужны Node.js, npm, Rust и Cargo.

```bash
cd frontend
npm install
npm run tauri dev
```

## Проверка backend

```bash
dotnet test
```

## Git flow

По требованиям задания:

1. Не коммитить напрямую в `main`.
2. Делать ветки вида `feature/*`.
3. Сливать изменения в `main` через pull request.
4. К дедлайну все нужные изменения должны быть в `main`.
