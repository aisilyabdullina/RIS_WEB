# Разработка системы учёта заказов контроллеров на предприятии (КонтрОрдер)

- **Студент:** Абдуллина Айсылу Фанисовна
- **Группа:** ТРИС-1-23
- **Курс:** Б1.В.ДЭ.01.02.02 · КГЭУ · семестр 1 · 2026/2027
- **Заказчик:** АО «НИЦ «Инкомсистем»», производственный участок
- **Стек:** Vue 3 (`:5173`) + ASP.NET Core (`:5000`) + PostgreSQL (`:5432`)

---

## Запуск сервера Backend (ЛР8 — REST: создание, чтение, назначение и статус)

Выполните из корня репозитория:

```bash
cd backend
dotnet restore
dotnet run
```

*(Если `dotnet` установлен локально в `~/.dotnet`, используйте `~/.dotnet/dotnet restore` и `~/.dotnet/dotnet run`).*

### Адреса и ограничения ЛР8

- **Базовый адрес API:** `http://localhost:5000`
- **Swagger UI:** `http://localhost:5000/swagger` (в среде `Development`)
- **Проверка живости (`GET /health`):** `http://localhost:5000/health` (`200 OK`, `Healthy` — проверяет только живость процесса, а не создание записи)
- **Сведения о сервисе (`GET /info`):** `http://localhost:5000/info`
- **Маршруты главного объекта (`Orders` — заказы на контроллеры):**
  - `POST /api/orders` — создать заказ (`201 Created`; сервер сам назначает `id`, `number`, `status: "New"`, `assigneeUserId: null`; при ошибке проверки полей — `400 Bad Request`)
  - `GET /api/orders` — получить список заказов (`200 OK`, включая пустой массив `[]`)
  - `GET /api/orders/{id}` — получить один заказ (`200 OK` или `404 Not Found`)
  - `PATCH /api/orders/{id}/assignee` — назначить исполнителя (`{ "assigneeUserId": 5 }` → `200 OK`, `400`, `404` или `409` для завершённого заказа)
  - `PATCH /api/orders/{id}/status` — изменить статус (`{ "status": "InProgress" }`, допустимые статусы: `New`, `InProgress`, `Closed`, `Cancelled` → `200 OK`, `400`, `404` или `409 Conflict` при недопустимом переходе и при переводе в `InProgress` без назначенного исполнителя)
  - те же маршруты доступны по псевдониму `/api/tickets` (имя из методички)
- **Важные ограничения текущего этапа (ЛР8):**
  - данные заказов хранятся в оперативной памяти сервера (`OrderService`), после перезапуска процесса они пропадут;
  - вход, справочник моделей (`/api/models`) и список инженеров (`/api/users`) описаны в контракте `docs/api-contract.md`, но на сервере пока не реализованы; проверка ролей (`403`) появится вместе с авторизацией;
  - метод `DELETE` не используется (отмена заказа выполняется переводом в статус `Cancelled`);
  - при неожиданном сбое (`500`) сервер пишет краткую строку в лог (`CreateOrder failed; modelId=...; error=...`), а клиенту возвращает безопасный `ProblemDetails` без `stack trace`.

---

## Коллекция запросов Postman (ЛР8)

Готовая коллекция с проверками `201`, `400`, `404`, `409` и обоих `PATCH` лежит в репозитории:

- `docs/api-collection/orders.postman_collection.json`
- `docs/api-collection/tickets.postman_collection.json` (копия со стандартным именем из методички)

---

## Запуск клиента Frontend (ЛР3–ЛР6 — Vue 3 + Vite)

Выполните из корня репозитория в отдельном терминале:

```bash
cd frontend
cp -n .env.example .env
npm install
npm run dev
```

Откройте адрес Vite, обычно `http://localhost:5173`. По умолчанию клиент работает на mock-данных (`USE_MOCK = true` в `frontend/src/api/tickets.js`); чтобы подключить сервер на порту `5000`, поставьте `USE_MOCK = false`.

---

## Дерево файлов репозитория

```text
RIS_WEB/
├── README.md                                           ← запуск backend и frontend, маршруты и ограничения ЛР8
├── .gitignore                                          ← исключения Git (node_modules/, .env, **/bin/, **/obj/)
├── docs/
│   ├── project-proposal.md                             ← заявка на тему (ЛР1)
│   ├── lab-01-requirements.md                          ← требования и критерии приёмки (ЛР1)
│   ├── architecture.md                                 ← схема из трёх узлов (ЛР2)
│   ├── data-model.md                                   ← ER-диаграмма, ключи и проверка 3НФ (ЛР2)
│   ├── api-contract.md                                 ← таблица API и примеры JSON (ЛР2)
│   ├── requirements-matrix.md                          ← матрица: критерий → поле → запрос (ЛР2)
│   ├── api-collection/
│   │   ├── orders.postman_collection.json              ← коллекция Postman для проверки REST API (ЛР8)
│   │   └── tickets.postman_collection.json             ← копия со стандартным именем из методички
│   └── reports/
│       └── lab-02.md .. lab-08.md                      ← отчёты по ЛР2–ЛР8
├── frontend/                                           ← клиент Vue 3 + Vite (ЛР3–ЛР6)
│   └── src/
│       ├── router.js, ticketsStore.js, App.vue
│       ├── api/ (tickets.js, mockTickets.js)
│       ├── components/ (AppHeader.vue, TicketTable.vue)
│       └── pages/ (TicketList.vue, TicketNew.vue, Login.vue)
└── backend/                                            ← сервер ASP.NET Core Web API (ЛР7–ЛР8)
    ├── backend.csproj, Program.cs, appsettings*.json
    ├── Properties/launchSettings.json                  ← HTTP-порт 5000, среда Development
    ├── Controllers/ (SystemController.cs, OrdersController.cs)
    ├── Dtos/ (CreateOrderRequest, AssignAssigneeRequest, ChangeStatusRequest, OrderResponse)
    ├── Models/Order.cs
    ├── Services/ (IOrderService.cs, OrderService.cs)   ← хранение в памяти, счётчик и правила процесса
    └── Data/.gitkeep                                   ← заготовка под контекст БД
```
