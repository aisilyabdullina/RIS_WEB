-- КонтрОрдер: SQL-запросы
-- Запуск: psql -d kontrorder -f db/queries.sql

-- 1. Фильтр и сортировка: заказы в статусе New, новые сверху
SELECT
    number,
    title,
    status,
    created_at
FROM orders
WHERE status = 'New'
ORDER BY created_at DESC;

-- 2. Соединение со справочником моделей и пользователями:
-- в результате видно название модели и ФИО, а не только числовые идентификаторы
SELECT
    o.number,
    o.title,
    m.name AS model_name,
    u_creator.full_name AS created_by,
    COALESCE(u_assignee.full_name, 'Не назначен') AS assignee_name,
    o.status,
    o.created_at
FROM orders AS o
JOIN controller_models AS m ON m.id = o.model_id
JOIN users AS u_creator ON u_creator.id = o.created_by_user_id
LEFT JOIN users AS u_assignee ON u_assignee.id = o.assignee_user_id
ORDER BY o.created_at DESC;

-- 3. Группировка: количество заказов по каждому из четырёх статусов
SELECT
    status,
    COUNT(*) AS count
FROM orders
GROUP BY status
ORDER BY status;

-- 4. Проверка ограничения CHECK: вставка недопустимого статуса 'Done' завершается отказом СУБД.
-- Запрос закомментирован, чтобы не прерывать выполнение файла; выполняйте его отдельно.
/*
INSERT INTO orders (number, title, description, model_id, status, created_by_user_id)
VALUES ('ЗК-2026-9999', 'Проверка недопустимого статуса Done',
        'Эта строка должна быть отклонена ограничением CHECK в PostgreSQL', 1, 'Done', 1);
*/
