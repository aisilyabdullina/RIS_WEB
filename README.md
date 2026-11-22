# Разработка системы учёта заказов контроллеров на предприятии (КонтрОрдер)

- **Студент:** Абдуллина Айсылу Фанисовна
- **Группа:** ТРИС-1-23
- **Курс:** Б1.В.ДЭ.01.02.02 · КГЭУ · семестр 1 · 2026/2027
- **Заказчик:** АО «НИЦ «Инкомсистем»», производственный участок
- **Стек:** Vue 3 (`:5173`) + ASP.NET Core (`:5000`) + EF Core + PostgreSQL (`:5432`)

---

## База данных PostgreSQL

Скрипты лежат в папке `db/`:

- `db/schema.sql` — таблицы `controller_models`, `users`, `orders`, ключи, ограничения `CHECK` и индексы;
- `db/seed.sql` — начальные данные: 5 моделей, 5 пользователей и 5 заказов во всех четырёх статусах;
- `db/queries.sql` — запросы с фильтрацией, сортировкой, `JOIN` и `GROUP BY`.

Создание базы и загрузка данных:

```bash
psql -U postgres -c "CREATE DATABASE kontrorder;"
psql -U postgres -d kontrorder -f db/schema.sql
psql -U postgres -d kontrorder -f db/seed.sql
psql -U postgres -d kontrorder -f db/queries.sql
```

---

## Сервер Backend: ASP.NET Core и EF Core

### 1. Строка подключения

Пароль не хранится в Git. Строка подключения задаётся через `dotnet user-secrets`:

```bash
cd backend
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:Default" "Host=localhost;Port=5432;Database=kontrorder;Username=postgres;Password=ВАШ_ПАРОЛЬ"
```

Шаблон без пароля лежит в `backend/appsettings.example.json`.

### 2. Миграции и запуск

```bash
cd backend
dotnet restore
dotnet ef database update
dotnet run
```

Если база пуста, при запуске сервер применяет миграции и сам заполняет таблицы начальными данными.

### 3. Адреса и маршруты

- **Базовый адрес API:** `http://localhost:5000`
- **Swagger UI:** `http://localhost:5000/swagger`
- **Проверка работы процесса:** `GET /health` возвращает `200 OK` и `Healthy`
- **Сведения о сервисе:** `GET /info`
- **Маршруты заказов:**
  - `POST /api/orders` — создать заказ: `201 Created` или `400 Bad Request`; сервер сам задаёт `id`, `number`, `status: "New"`, `assigneeUserId: null`
  - `GET /api/orders` — список заказов: `200 OK`, пустой список возвращается как `[]`
  - `GET /api/orders/{id}` — один заказ: `200 OK` или `404 Not Found`
  - `PATCH /api/orders/{id}/assignee` — назначить исполнителя, `{ "assigneeUserId": 5 }`: `200 OK`, `400`, `404` или `409` для завершённого заказа
  - `PATCH /api/orders/{id}/status` — сменить статус, `{ "status": "InProgress" }`: `200 OK`, `400`, `404` или `409` при недопустимом переходе и при переводе в `InProgress` без исполнителя

Заказы сохраняются в PostgreSQL и после перезапуска сервера остаются на месте. Метод `DELETE` не используется: отмена заказа выполняется переводом в статус `Cancelled`. Вход, справочник моделей и проверка ролей описаны в `docs/api-contract.md` и на сервере пока не реализованы.

При неожиданном сбое сервер пишет в лог одну строку без секретов, а клиенту возвращает `ProblemDetails` без `stack trace`.

### 4. Коллекция Postman

Коллекция с проверками `201`, `400`, `404`, `409` и обоих `PATCH`: `docs/api-collection/orders.postman_collection.json`.

---

## Клиент Frontend: Vue 3 и Vite

```bash
cd frontend
cp -n .env.example .env
npm install
npm run dev
```

Адрес клиента: `http://localhost:5173`. По умолчанию клиент работает на тестовых данных. Чтобы подключить сервер, в `frontend/src/api/orders.js` установите `USE_MOCK = false`.

---

## Структура репозитория

```text
RIS_WEB/
├── README.md
├── .gitignore
├── db/
│   ├── schema.sql                      ← таблицы, ключи, ограничения и индексы
│   ├── seed.sql                        ← начальные данные
│   └── queries.sql                     ← аналитические запросы
├── docs/
│   ├── project-proposal.md             ← заявка на тему
│   ├── lab-01-requirements.md          ← требования и критерии приёмки
│   ├── architecture.md                 ← архитектура системы
│   ├── data-model.md                   ← ER-диаграмма, ключи, 3НФ, статусы
│   ├── api-contract.md                 ← маршруты API и примеры JSON
│   ├── requirements-matrix.md          ← трассировка требований
│   ├── api-collection/
│   │   └── orders.postman_collection.json
│   └── reports/
│       └── lab-02.md .. lab-10.md      ← отчёты по лабораторным работам 2–10
├── frontend/
│   ├── package.json, vite.config.js, index.html, .env.example
│   └── src/
│       ├── main.js, App.vue, router.js
│       ├── ordersStore.js, modelCatalog.js, statuses.js
│       ├── api/ (orders.js, mockOrders.js)
│       ├── components/ (AppHeader.vue, OrderTable.vue)
│       └── pages/ (OrderList.vue, OrderNew.vue, Login.vue)
└── backend/
    ├── backend.csproj, Program.cs
    ├── appsettings.json, appsettings.example.json
    ├── Controllers/ (SystemController.cs, OrdersController.cs)
    ├── Dtos/ (CreateOrderRequest, OrderResponse, AssignAssigneeRequest, ChangeStatusRequest)
    ├── Models/ (ControllerModel.cs, User.cs, Order.cs)
    ├── Data/ (AppDbContext.cs, DbSeeder.cs)
    ├── Migrations/                     ← миграция InitialCreate
    └── Services/ (IOrderService.cs, OrderService.cs)
```
