# F1 Monitor

[![CI](https://github.com/savi0810/f1-monitoring/actions/workflows/ci.yml/badge.svg)](https://github.com/savi0810/f1-monitoring/actions/workflows/ci.yml)

Веб-приложение для наблюдения за гонками Формулы 1.

Пока идёт гонка, пользователь видит текущие позиции пилотов. Можно подписаться на пилота и получать уведомления, когда он меняет позицию.

Прошедшие сессии можно выбрать из списка, загрузить их результаты и посмотреть круги пилотов. Загруженные данные сохраняются в базе.

## Что можно делать в приложении

- Смотреть позиции пилотов во время гонки и состояние подключения к источнику.
- Подписываться на изменения позиций выбранных пилотов и отменять подписки. После завершения гонки подписки удаляются.
- Смотреть последние пять уведомлений, полученных на странице live.
- Выбирать прошедшие сессии: практики, квалификации, спринты и гонки.
- Загружать результаты сессии, смотреть время кругов и секторов, искать пилота по имени, номеру или команде.
- Регистрироваться и входить в аккаунт. Каждый пользователь видит и меняет только свои подписки.

## Из чего состоит проект

| Сервис | Что делает | База данных |
|---|---|---|
| Gateway.Api | Единая точка входа: запросы от сайта идут через него | нет |
| Auth.Api | Регистрация, вход, выдача JWT-токена | PostgreSQL (authdb) |
| Telemetry.Api | Хранит каталог сессий, результаты пилотов и круги, запускает импорт данных | PostgreSQL (telemetrydb) |
| Alerts.Api | Хранит подписки на пилотов и создаёт уведомления об изменении позиций | PostgreSQL (alertsdb) |
| Live.Api | Хранит снимок гонки, определяет изменения позиций и отправляет обновления и уведомления через SignalR | Redis |
| F1.Collector | Получает прошлые данные из OpenF1 и live-позиции из ESPN, передаёт их через RabbitMQ | нет |
| Frontend | Сайт на Vue 3 | нет |

Данные гонок и уведомления передаются между сервисами через RabbitMQ.

## Технологии

- Сервисы: .NET 10, ASP.NET Core, Entity Framework Core, YARP, SignalR
- Базы данных: PostgreSQL, Redis
- Очередь сообщений: RabbitMQ
- Сбор данных: OpenF1, ESPN
- Веб интерфейс: Vue 3, TypeScript, Tailwind CSS, shadcn-vue
- Запуск: Docker Compose
- Логирование: Serilog
- Тесты: xUnit
- CI: GitHub Actions

## Схема

<img width="1313" height="1198" alt="exec-70af1d08-b974-4cbd-bcc6-b8f904eeb533" src="https://github.com/user-attachments/assets/411065d4-f163-4733-8814-7c184cb89d37" />

## Быстрый старт

Нужен установленный и запущенный Docker Desktop. Для загрузки данных нужен доступ к OpenF1 и ESPN.

1. Склонируйте репозиторий:

```bash
git clone https://github.com/savi0810/f1-monitoring.git
cd f1-monitoring
```

2. Создайте файл `.env` из шаблона:

```bash
cp .env.example .env
```

Откройте `.env` и заполните переменные:

- `POSTGRES_USER` и `POSTGRES_PASSWORD`: логин и пароль базы данных;
- `JWT_KEY`: секретный ключ для подписи токенов, случайная строка не короче 32 символов;
- `RABBITMQ_USER` и `RABBITMQ_PASSWORD`: логин и пароль RabbitMQ. Логин уже указан в шаблоне, его можно оставить;
- `REDIS_PASSWORD`: пароль Redis.

Файл `.env` содержит секреты и в репозиторий не попадает.

3. Запустите систему:

```bash
docker compose up --build
```

Первая сборка занимает несколько минут. Остановить систему: `docker compose down`.

Адреса:

| Сервис | Адрес |
|---|---|
| Сайт | http://localhost:5173 |
| Gateway | http://localhost:5055 |

Остальные сервисы доступны только внутри Docker. Swagger есть у Auth.Api, Telemetry.Api, Alerts.Api и Live.Api, но через Gateway он не открыт.

Проверка работоспособности Gateway: http://localhost:5055/health.

Готового тестового аккаунта нет. Для демонстрации зарегистрируйте пользователя с именем `demo`, почтой `demo@example.com` и паролем `demo123`, затем войдите.

Каталог сессий загружается в фоне. Если список пока пустой, подождите и обновите страницу. Выберите прошедшую сессию и нажмите «Загрузить результаты».

## Тесты и CI

Для локального запуска тестов нужен .NET SDK 10.

```bash
dotnet test
```

GitHub Actions собирает backend и запускает тесты при push в main и при Pull Request.

## Структура репозитория

```text
.github/
└── workflows/                  — сборка и тесты в GitHub Actions

src/
├── Auth.Api/                   — регистрация, вход и JWT
│   ├── Controllers/
│   ├── Services/
│   ├── Data/
│   ├── Exceptions/              — обработка ошибок регистрации и входа
│   └── Extensions/
├── Telemetry.Api/              — каталог сессий, результаты и круги
│   ├── Controllers/
│   ├── Services/
│   ├── Data/
│   ├── Consuming/               — получение каталога и результатов из RabbitMQ
│   ├── Publishing/              — отправка запросов на импорт
│   ├── Mapping/                 — преобразование данных в модели и DTO
│   └── Extensions/
├── Alerts.Api/                 — подписки и уведомления
│   ├── Controllers/
│   ├── Services/
│   ├── Data/
│   ├── Consuming/               — получение изменений позиций и завершения гонки
│   ├── Publishing/              — отправка уведомлений через Outbox
│   └── Extensions/
├── Live.Api/                   — текущее состояние гонки и SignalR
│   ├── Controllers/
│   ├── Services/
│   ├── Data/
│   ├── Hubs/                    — подключение клиентов к обновлениям гонки
│   ├── Consuming/
│   ├── Publishing/
│   └── Extensions/
├── F1.Collector/               — сбор данных из внешних источников
│   ├── OpenF1/                  — запросы прошлых данных и ограничение частоты запросов
│   ├── Espn/                    — получение live-позиций в фоне
│   ├── Mapping/
│   ├── Consuming/               — получение запросов на импорт
│   ├── Publishing/              — отправка каталога, результатов и live-снимков
│   └── Extensions/
├── Gateway.Api/                — маршрутизация запросов через YARP
└── F1Monitor.Contracts/        — общие форматы сообщений, названия очередей и событий

frontend/
├── public/                     — статические файлы
├── src/
│   ├── components/             — общие компоненты интерфейса
│   │   └── ui/                  — компоненты shadcn-vue
│   ├── features/
│   │   ├── auth/                — вход, регистрация и хранение токена
│   │   ├── telemetry/           — каталог, импорт и результаты сессий
│   │   ├── live/                — live-позиции, SignalR и уведомления
│   │   └── alerts/              — подписки на пилотов
│   ├── pages/                  — страницы приложения
│   ├── layouts/                — общий макет страниц
│   ├── router/                 — маршруты и проверка входа
│   ├── assets/                 — изображения
│   └── lib/                    — общие вспомогательные функции
├── Dockerfile
└── nginx.conf                  — раздача сайта и передача запросов в Gateway

tests/
├── Auth.Api.Tests/
├── Telemetry.Api.Tests/
├── Alerts.Api.Tests/
├── Live.Api.Tests/
├── F1.Collector.Tests/
└── F1Monitor.Contracts.Tests/

docs/
├── PLAN.md                     — этапы, сроки и статус работы
└── ARCHITECTURE.md              — устройство системы и принятые решения

docker-compose.yml              — запуск сервисов, сайта, баз и RabbitMQ
.env.example                    — шаблон переменных окружения
F1Monitor.sln                   — решение с backend-проектами и тестами
```

В API-сервисах `Controllers` принимают запросы, `Services` выполняют логику, а `Data/Repositories` работают с базой. В `Data/DTOs` лежат данные для API, в `Data/Models` — модели хранения. У сервисов с PostgreSQL в `Data/Migrations` находятся миграции базы.

В папках функций клиента используются `api` для запросов, `composables` для логики, `components` для интерфейса и `types` для типов данных. В `Extensions` на backend настраиваются зависимости, подключение к базам, RabbitMQ и JWT.

## Документы

- [План работ](docs/PLAN.md)
- [Архитектура](docs/ARCHITECTURE.md)
