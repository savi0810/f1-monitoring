# Архитектура системы

Статус: реализованы импорт прошлых сессий, live-позиции, подписки на пилотов, уведомления и клиентский интерфейс.


## 1. Цель и контекст

F1 Monitor показывает ход гонок Формулы 1 в реальном времени и хранит историю прошедших гонок. Во время гонки пользователь видит таблицу пилотов и получает уведомления об изменении позиций выбранных пилотов. После гонки данные остаются, и любую гонку можно открыть из списка.

Данные прошедших гонок загружаются из OpenF1 и сохраняются в базе. Live-позиции приходят из ESPN.

Пользователь один: зарегистрированный посетитель (роль `user`).

## 2. Требования

- Приём потока событий гонки и сохранение в базе.
- Подписки на пилотов и получение уведомлений
- Повторная доставка сообщения не даёт двойного эффекта.
- Уведомление не теряется, если сервис упал при отправке.
- Чужие подписки недоступны (ответ 403).
- Запуск одной командой `docker compose up --build`.

## 3. Общая схема

<img width="1313" height="1198" alt="exec-70af1d08-b974-4cbd-bcc6-b8f904eeb533" src="https://github.com/user-attachments/assets/01f1c71a-fb9d-4d47-87be-6b8b68a79a1c" />

Клиент обращается к сервисам через Gateway. Сервисы между собой общаются через RabbitMQ. У каждого сервиса своя база данных.

## 4. Описание сервисов

| Сервис | Ответственность | База данных | Публикует | Читает |
|--------|----------------|-------------|-----------|--------|
| Gateway.Api | Единая точка входа (YARP), маршруты `/api/auth`, `/api/telemetry`, `/api/alerts`, `/api/live`, `/livehub` | нет | — | — |
| Auth.Api | Регистрация, вход, выдача JWT | PostgreSQL `authdb` | — | — |
| Telemetry.Api | Каталог сессий, импорт, результаты пилотов и круги | PostgreSQL `telemetrydb` | `import.requested` | `sessions.available`, `history.session_driver`, `history.lap_completed`, `history.session_import_finished` |
| Alerts.Api | Подписки на пилотов, уведомления об изменениях позиций, Outbox | PostgreSQL `alertsdb` | `alerts.position_alert_triggered` | `live.driver_positions_changed`, `live.race_completed` |
| Live.Api | SignalR-хаб, снимок гонки, определение изменений позиций и завершения гонки | Redis | `live.driver_positions_changed`, `live.race_completed` | `live.snapshot`, `alerts.position_alert_triggered` |
| F1.Collector | Получение прошлых данных из OpenF1 и live-позиций из ESPN | нет | `sessions.available`, `history.session_driver`, `history.lap_completed`, `history.session_import_finished`, `live.snapshot` | `import.requested` |
| Frontend | Клиент на Vue 3 | нет | — | — |

В API-сервисах контроллеры обращаются к сервисам, сервисы — к репозиториям. DTO отделены от моделей хранения. Общие контракты сообщений находятся в `F1Monitor.Contracts`.


### Эндпоинты

| Сервис | Запрос | Что делает |
|--------|--------|-----------|
| Auth.Api | `POST /api/auth/register` | Регистрация |
| Auth.Api | `POST /api/auth/login` | Вход по email и паролю, выдача JWT |
| Telemetry.Api | `GET /api/telemetry/sessions?type=&year=` | Каталог сессий по годам и гоночным уикендам |
| Telemetry.Api | `GET /api/telemetry/sessions/{sessionKey}/results` | Результаты пилотов и сохранённые круги |
| Telemetry.Api | `GET /api/telemetry/sessions/{sessionKey}/import` | Состояние импорта |
| Telemetry.Api | `POST /api/telemetry/sessions/{sessionKey}/import` | Запуск импорта; нужен JWT, принятый запрос возвращает `202 Accepted` |
| Alerts.Api | `GET /api/alerts/subscriptions` | Свои подписки |
| Alerts.Api | `PUT /api/alerts/subscriptions` | Подписка на пилота, `driverId` передаётся в теле запроса |
| Alerts.Api | `DELETE /api/alerts/subscriptions?driverId=...` | Удаление своей подписки |
| Live.Api | `GET /api/live/current` | Текущий снимок гонки |
| Live.Api | `/livehub` (SignalR) | Подключение к обновлениям и уведомлениям |

В хабе метод `SubscribeSession(sessionId)` подключает клиента к группе выбранной гонки. Клиент получает `TimingUpdated` с состоянием гонки, `SourceUpdated` со временем обновления источника и личные уведомления `AlertTriggered`.

## 5. Модель данных

- `authdb`: `Users` — id, имя, email и хеш пароля. Имя и email уникальны.
- `telemetrydb`: `Meetings` — гоночные уикенды; `Sessions` — сессии и состояние их импорта; `SessionDrivers` — участники и результаты; `Laps` — круги и время секторов. Сессия связана с уикендом, результат пилота — с сессией.
- `alertsdb`: `DriverSubscriptions` — подписки пользователей; `ProcessedEvents` — обработанные события; `OutboxMessages` — подготовленные уведомления; `CompletedRaces` — завершённые гонки.
- Redis: текущий снимок, снимки по сессиям, отметки завершения гонок и очередь исходящих событий Live.

Круг определяется по `EventId`, результат пилота по паре `SessionKey` и `DriverNumber`. Подписка уникальна для пары `UserId` и `DriverId`.

## 6. Взаимодействие сервисов

В RabbitMQ используются два topic-обменника: `f1.events` для прошлых данных и `f1.live` для live-событий. Каталог сессий и запросы импорта передаются напрямую в отдельные очереди через стандартный обменник.


| Обменник | Ключ маршрутизации | Очередь | Кто отправляет -> кто получает |
|----------|--------------------|---------|-------------------------------|
| Стандартный | `sessions.available` | `sessions.available` | Collector -> Telemetry |
| Стандартный | `import.requested` | `import.requested` | Telemetry -> Collector |
| `f1.events` | `history.*`, подписка `#` | `telemetry.events` | Collector -> Telemetry |
| `f1.live` | `live.snapshot` | `live.snapshots` | Collector -> Live |
| `f1.live` | `live.driver_positions_changed`, `live.race_completed` | `alerts.live.events` | Live -> Alerts |
| `f1.live` | `alerts.position_alert_triggered` | `alerts.position_alerts_triggered` | Alerts -> Live |

### Импорт прошлой сессии

Пользователь запускает импорт через Telemetry.Api. Сервис сохраняет состояние `Pending` и отправляет `import.requested`. Collector запрашивает участников, результаты и круги в OpenF1, затем публикует события `session_driver`, `lap_completed` и `session_import_finished`.

Telemetry сохраняет данные и завершает импорт со статусом `Completed`, `Empty` или `Failed`. Клиент периодически запрашивает состояние импорта и после завершения обновляет результаты.

### Изменение позиции и уведомление

Live сравнивает позиции только между снимками одной гонки в состоянии `in`. Старые снимки не заменяют новые. Снимок и исходящее событие сохраняются одной транзакцией Redis.

Alerts записывает обработанный `EventId` и уведомления Outbox одной транзакцией PostgreSQL. Повторное событие не создаёт новые уведомления. Фоновые обработчики отправляют сообщения из Outbox с повторными попытками. Клиент убирает повторные уведомления по `AlertId`.

Когда гонка завершена, Live отправляет `live.race_completed`. Alerts сохраняет завершение и удаляет подписки, созданные не позже времени этого события. После завершения новые уведомления об изменениях позиций этой гонки не создаются.

## 7. Безопасность

- Пароли хранятся как хеш BCrypt, вход выдаёт JWT.
- Роль одна, `user`. Доступ к правилам, подпискам и избранному только у владельца.
- Секреты и строки подключения в переменных окружения, не в репозитории.

## 8. Развёртывание и конфигурация

Вся система запускается через `docker compose up --build`. Наружу открыты сайт на порту `5173` и Gateway на порту `5055`. Остальные сервисы и базы доступны внутри Docker.

Nginx раздаёт сайт и передаёт `/api/` и `/livehub` в Gateway. Gateway направляет запросы в нужный API.

Переменные `.env`: `POSTGRES_USER`, `POSTGRES_PASSWORD`, `JWT_KEY`, `RABBITMQ_USER`, `RABBITMQ_PASSWORD`, `REDIS_PASSWORD`. Инструкция по заполнению находится в [README](../README.md).

Миграции PostgreSQL применяются при запуске API. Во всех backend-сервисах есть `/health`. Логи выводятся в консоль через Serilog.

## 9. Принятые решения

| Решение | Альтернатива | Причина |
|---------|-------------|---------|
| Collector на .NET | Отдельный сборщик на Python | Общий стек и общие контракты сообщений с остальными сервисами |
| Отдельные обменники истории и live, очереди для получателей | Одна общая очередь | Разделение потоков и доставка сообщений нужным обработчикам |
| Outbox в Live и Alerts | Отправка сразу после сохранения данных | Исходящее сообщение сохраняется вместе с данными и может быть отправлено повторно |
| Redis для live-состояния | Хранить снимок в PostgreSQL | Быстрое чтение текущего снимка и атомарное сохранение снимка вместе с исходящим событием |
## 10. Ограничения и развитие

- ESPN настроен на конкретную гонку. Дата запроса находится в `src/F1.Collector/Espn/EspnClient.cs` и идентификаторы в `src/F1.Collector/appsettings.json`. Автоматического выбора следующей гонки нет.
- Live показывает позиции пилотов. Время кругов, разрывы, пит-стопы, погода и флаги в live не отображаются, ESPN этого не даёт, а другие API платные.
- Импорт зависит от доступности и ограничений OpenF1.
