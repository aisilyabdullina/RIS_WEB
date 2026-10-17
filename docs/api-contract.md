# Контракт API — АИС «КонтрОрдер»

## Общие сведения

- **Базовый URL:** `http://localhost:5000/api`
- **Формат данных:** JSON
- **Аутентификация:** Bearer-токен (в заголовке `Authorization`)
- **Коды статусов:** `New`, `InProgress`, `Closed`, `Cancelled`

---

## Таблица API

| Метод и путь                      | Что прислать (тело)              | Успех                                         | Ошибка                                                          |
| --------------------------------- | -------------------------------- | --------------------------------------------- | --------------------------------------------------------------- |
| `POST /api/auth/login`            | `login`, `password`              | `200`, `token`, `role`, `fullName`            | `401` — неверный логин или пароль                               |
| `GET /api/models`                 | —                                | `200`, массив моделей (или `[]`)              | —                                                               |
| `GET /api/orders`                 | —                                | `200`, массив заказов (или `[]`)              | —                                                               |
| `GET /api/orders/{id}`            | —                                | `200`, объект заказа                          | `404` — заказ не найден                                         |
| `POST /api/orders`                | `title`, `modelId`, `description`| `201`, в ответе `id`, `number`, `status: New` | `400` — невалидные данные                                       |
| `PATCH /api/orders/{id}/assignee` | `assigneeUserId`                 | `200`, обновлённый заказ                      | `404` — заказ не найден                                         |
| `PATCH /api/orders/{id}/status`   | `status`                         | `200`, обновлённый заказ                      | `404` — заказ не найден; `409` — недопустимый переход статуса   |

---

## Примеры запросов и ответов

### 1. Создание заказа (POST)

**Запрос:**

```
POST /api/orders
Content-Type: application/json

{
  "title": "Контроллер для линии фасовки",
  "modelId": 5,
  "description": "10 шт., интерфейс Modbus RTU, исполнение для цеха с повышенной температурой"
}
```

**Успешный ответ (201 Created):**

```
{
  "id": 17,
  "number": "ORD-2026-042",
  "title": "Контроллер для линии фасовки",
  "description": "10 шт., интерфейс Modbus RTU, исполнение для цеха с повышенной температурой",
  "status": "New",
  "modelId": 5,
  "createdByUserId": 3,
  "assigneeUserId": null,
  "createdAt": "2026-10-19T10:30:00Z",
  "updatedAt": "2026-10-19T10:30:00Z"
}
```

**Ошибка (400 Bad Request):**

```
{
  "error": "Поле modelId должно быть больше 0"
}
```

### 2. Получение списка заказов (GET)

**Запрос:** GET /api/orders

**Успешный ответ (200 OK):**

```
[
  {
    "id": 17,
    "number": "ORD-2026-042",
    "title": "Контроллер для линии фасовки",
    "status": "New",
    "modelId": 5,
    "createdByUserId": 3,
    "assigneeUserId": null
  },
  {
    "id": 16,
    "number": "ORD-2026-041",
    "title": "Модули ввода-вывода для насосной станции",
    "status": "InProgress",
    "modelId": 12,
    "createdByUserId": 7,
    "assigneeUserId": 2
  }
]
```

**Пустой список (200 OK):**

```
[]
```

### 3. Получение одного заказа (GET)

**Запрос:** GET /api/orders/17

```
{
  "id": 17,
  "number": "ORD-2026-042",
  "title": "Контроллер для линии фасовки",
  "description": "10 шт., интерфейс Modbus RTU, исполнение для цеха с повышенной температурой",
  "status": "New",
  "modelId": 5,
  "createdByUserId": 3,
  "assigneeUserId": null,
  "createdAt": "2026-10-19T10:30:00Z",
  "updatedAt": "2026-10-19T10:30:00Z"
}
```

**Ошибка (404 Not Found):**

```
{
  "error": "Заказ с id=17 не найден"
}
```

### 4. Назначение исполнителя (PATCH)

**Запрос:**

```
PATCH /api/orders/17/assignee
Content-Type: application/json

{
  "assigneeUserId": 2
}
```

**Успешный ответ (200 OK):**

```
{
  "id": 17,
  "number": "ORD-2026-042",
  "title": "Контроллер для линии фасовки",
  "status": "New",
  "assigneeUserId": 2,
  "updatedAt": "2026-10-19T11:00:00Z"
}
```

**Ошибка (404 Not Found):**

```
{
  "error": "Заказ с id=17 не найден"
}
```

### 5. Смена статуса (PATCH)

**Запрос:**

```
PATCH /api/orders/17/status
Content-Type: application/json

{
  "status": "InProgress"
}
```

**Успешный ответ (200 OK):**

```
{
  "id": 17,
  "number": "ORD-2026-042",
  "title": "Контроллер для линии фасовки",
  "status": "InProgress",
  "assigneeUserId": 2,
  "updatedAt": "2026-10-19T11:15:00Z"
}
```

**Ошибка (409 Conflict — недопустимый переход):**

```
{
  "error": "Нельзя перевести заказ из статуса 'Closed' в 'InProgress'"
}
```

## Важные правила

1. Клиент НЕ присылает при создании: id, number, status, assigneeUserId, createdAt, updatedAt. Их генерирует сервер.
2. Два разных PATCH: назначение исполнителя (/assignee) и смена статуса (/status) — это разные запросы. Нельзя объединять в один PATCH /update.
3. Нет DELETE: отмена заказа = смена статуса на Cancelled, строка не удаляется из БД.
4. Пустой список = 200 и [], а не 404.
5. Пути вида /api/orders/create не используются. Действие определяется методом (POST), а не словом в адресе.
