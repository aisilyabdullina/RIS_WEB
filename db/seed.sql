-- КонтрОрдер: начальные данные
-- Запуск: psql -d kontrorder -f db/seed.sql
TRUNCATE TABLE orders, users, controller_models RESTART IDENTITY CASCADE;

-- 1. Справочник моделей контроллеров
INSERT INTO controller_models (id, name, article, controller_type, io_count, protocol, stock_qty) VALUES
    (1, 'ПЛК-110', 'PLC-110', 'ПЛК', 16, 'Modbus RTU', 12),
    (2, 'ПЛК-120', 'PLC-120', 'ПЛК', 32, 'Modbus TCP', 5),
    (3, 'МВ-8DI8DO', 'IO-8DI8DO', 'Модуль ввода-вывода', 16, 'Modbus RTU', 20),
    (4, 'МВ-8AI', 'IO-8AI', 'Модуль ввода-вывода', 8, 'Modbus RTU', 9),
    (5, 'КСУ-300', 'KSU-300', 'Контроллер локальной автоматики', 24, 'CAN', 3);

SELECT setval(pg_get_serial_sequence('controller_models', 'id'), (SELECT MAX(id) FROM controller_models));

-- 2. Пользователи
INSERT INTO users (id, full_name, email, role) VALUES
    (1, 'Мустафина Ляйсан Рашитовна', 'l.mustafina@incomsystem.local', 'Manager'),
    (2, 'Ахметов Рустем Данилович', 'r.akhmetov@incomsystem.local', 'Engineer'),
    (3, 'Сафина Гульнара Маратовна', 'g.safina@incomsystem.local', 'Engineer'),
    (4, 'Фаттахов Ренат Ильгизович', 'r.fattakhov@incomsystem.local', 'Chief'),
    (5, 'Хайруллин Тимур Азатович', 't.khairullin@incomsystem.local', 'Engineer');

SELECT setval(pg_get_serial_sequence('users', 'id'), (SELECT MAX(id) FROM users));

-- 3. Заказы: представлены все четыре статуса
INSERT INTO orders (id, number, title, description, model_id, status, created_at, created_by_user_id, assignee_user_id) VALUES
    (1, 'ЗК-2026-0001', 'Контроллеры для линии фасовки',
        '10 шт., интерфейс Modbus RTU, исполнение для цеха с повышенной температурой',
        1, 'New', '2026-11-01 09:15:00+03', 1, NULL),
    (2, 'ЗК-2026-0002', 'Модули аналогового ввода для насосной станции',
        '4 шт., входы 4–20 мА, монтаж на DIN-рейку, поставка вместе с паспортами',
        4, 'InProgress', '2026-11-02 10:30:00+03', 1, 2),
    (3, 'ЗК-2026-0003', 'Контроллер локальной автоматики КСУ',
        '2 шт., интерфейс CAN, комплект кабелей подключения',
        5, 'Closed', '2026-11-03 14:20:00+03', 1, 3),
    (4, 'ЗК-2026-0004', 'Партия ПЛК-120 для котельной',
        '5 шт., интерфейс Modbus TCP; заказ отменён заказчиком до начала сборки',
        2, 'Cancelled', '2026-11-04 16:45:00+03', 1, NULL),
    (5, 'ЗК-2026-0005', 'Модули ввода-вывода для компрессорной станции',
        '8 шт., 16 каналов дискретного ввода-вывода, интерфейс Modbus RTU',
        3, 'New', '2026-11-05 11:00:00+03', 1, NULL);

SELECT setval(pg_get_serial_sequence('orders', 'id'), (SELECT MAX(id) FROM orders));
