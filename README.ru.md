<div align="center">

<img src="https://img.shields.io/badge/версия-1.2.7-0078D4?style=flat-square" alt="Версия"/>
<img src="https://img.shields.io/badge/.NET-10.0-512BD4?style=flat-square" alt=".NET"/>
<img src="https://img.shields.io/badge/платформа-Windows-0078D4?style=flat-square" alt="Платформа"/>
<img src="https://img.shields.io/badge/лицензия-MIT-10B981?style=flat-square" alt="Лицензия"/>

# OpenSourceHub

**Профессиональная платформа GitHub-аналитики**

Анализируй. Открывай. Создавай лучшее.

[Скачать v1.2.7](https://github.com/X1NPAR1/OpenSourceHub/releases/latest) · [Сообщить об ошибке](https://github.com/X1NPAR1/OpenSourceHub/issues) · [Предложить функцию](https://github.com/X1NPAR1/OpenSourceHub/issues)

</div>

---

## Обзор

OpenSourceHub — профессиональное настольное приложение для Windows для анализа GitHub-репозиториев. Объединяет данные GitHub API, AI-инсайты, оценку безопасности и профессиональную отчётность в современном интерфейсе с тёмной темой на WPF.

Разработано на **.NET 10**, **WPF** и **чистой архитектуре** — всё необходимое для оценки, сравнения и мониторинга открытых проектов.

---

## Возможности

| Функция | Описание |
|---------|----------|
| **Анализ репозиториев** | 6-мерная оценка здоровья: Активность, Обслуживание, Безопасность, Сообщество, Популярность, Здоровье |
| **AI-инсайты** | Сводки, рекомендации по внедрению и предложения по улучшению через OpenAI и Ollama |
| **Центр безопасности** | Обнаружение рисков по обслуживанию, лицензированию и активности |
| **Тренды** | Обнаружение популярных репозиториев по языку и периоду |
| **Сравнение** | Сравнение до 4 репозиториев одновременно |
| **Организации** | Изучение GitHub-организаций, участников и репозиториев |
| **Избранное** | Закладки и аннотации для репозиториев |
| **Отчёты** | Экспорт в PDF, Markdown и HTML |
| **Журналы** | Журнал событий приложения с фильтрацией и экспортом |
| **Настройки** | Тема, язык (RU/EN/TR), AI-провайдер, кэш и уведомления |

---

## Начало работы

### Требования

- Windows 10 x64 или новее
- [.NET 10 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/10.0)
- Персональный токен доступа GitHub

### Установка

1. Скачайте последний релиз со страницы [Releases](https://github.com/X1NPAR1/OpenSourceHub/releases/latest)
2. Распакуйте ZIP-файл
3. Запустите `OpenSourceHub.exe`

### Настройка токена GitHub

1. Перейдите в **GitHub → Настройки → Настройки разработчика → Персональные токены доступа**
2. Создайте новый токен со следующими областями:
   ```
   repo, read:org, read:user, user:email
   ```
3. Скопируйте токен (начинается с `ghp_` или `github_pat_`)
4. Вставьте его на экране входа и нажмите **Войти через GitHub**

---

## Сборка из исходного кода

### Предварительные условия

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Windows 10 x64 или новее
- Visual Studio 2022+ или JetBrains Rider

### Шаги

```bash
git clone https://github.com/X1NPAR1/OpenSourceHub.git
cd OpenSourceHub
dotnet restore
dotnet build
dotnet run --project src/OpenSourceHub.UI
```

### Публикация (Release Build)

```bash
dotnet publish src/OpenSourceHub.UI/OpenSourceHub.UI.csproj \
  -c Release -r win-x64 --self-contained false \
  -o ./release/output
```

---

## Архитектура

```
OpenSourceHub/
├── src/
│   ├── OpenSourceHub.Domain          # Сущности, перечисления, интерфейсы
│   ├── OpenSourceHub.Application     # Сценарии использования, логика
│   ├── OpenSourceHub.Infrastructure  # GitHub API, AI-сервисы, SQLite, EF Core
│   ├── OpenSourceHub.Localization    # RU / EN / TR строки
│   ├── OpenSourceHub.Reporting       # Генерация PDF / Markdown / HTML
│   └── OpenSourceHub.UI              # WPF-приложение (MVVM)
└── tests/
    └── OpenSourceHub.Tests           # Модульные и интеграционные тесты xUnit
```

**Паттерны:** Clean Architecture · MVVM (CommunityToolkit.Mvvm) · Dependency Injection · Repository Pattern

---

## Настройка AI

### OpenAI (Облако)
1. Выберите **Настройки → AI → Провайдер → OpenAI**
2. Введите ваш API-ключ OpenAI (`sk-...`)
3. Укажите модель (по умолчанию: `gpt-4o-mini`)

### Ollama (Локально)
1. Установите [Ollama](https://ollama.com/)
2. Загрузите модель: `ollama pull llama3.2`
3. Выберите **Настройки → AI → Провайдер → Ollama**
4. Укажите endpoint и имя модели

---

## Локализация

Приложение поддерживает три языка, переключаемых в реальном времени из **Настройки → Внешний вид → Язык**:

| Язык | Код |
|------|-----|
| English | EN |
| Türkçe | TR |
| Русский | RU |

---

## Лицензия

Лицензия MIT — подробности в файле [LICENSE](LICENSE).

---

## Участие в разработке

Вклады приветствуются. Пожалуйста, откройте issue перед отправкой pull request для обсуждения предлагаемых изменений.

---

<div align="center">
Сделано с ❤️ — <a href="https://github.com/X1NPAR1">X1NPAR1</a> · XinPari Software · Версия 1.2.7
</div>
