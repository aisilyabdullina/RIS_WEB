using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Data;

public static class DbSeeder
{
    private static void FixSequence(AppDbContext db, string table)
    {
#pragma warning disable EF1002
        db.Database.ExecuteSqlRaw(
            $"SELECT setval(pg_get_serial_sequence('{table}', 'id'), COALESCE((SELECT MAX(id) FROM {table}), 1));");
#pragma warning restore EF1002
    }

    public static void SeedIfEmpty(AppDbContext db)
    {
        if (!db.ControllerModels.Any())
        {
            db.ControllerModels.AddRange(
                new ControllerModel { Id = 1, Name = "ПЛК-110", Article = "PLC-110", ControllerType = "ПЛК", IoCount = 16, Protocol = "Modbus RTU", StockQty = 12 },
                new ControllerModel { Id = 2, Name = "ПЛК-120", Article = "PLC-120", ControllerType = "ПЛК", IoCount = 32, Protocol = "Modbus TCP", StockQty = 5 },
                new ControllerModel { Id = 3, Name = "МВ-8DI8DO", Article = "IO-8DI8DO", ControllerType = "Модуль ввода-вывода", IoCount = 16, Protocol = "Modbus RTU", StockQty = 20 },
                new ControllerModel { Id = 4, Name = "МВ-8AI", Article = "IO-8AI", ControllerType = "Модуль ввода-вывода", IoCount = 8, Protocol = "Modbus RTU", StockQty = 9 },
                new ControllerModel { Id = 5, Name = "КСУ-300", Article = "KSU-300", ControllerType = "Контроллер локальной автоматики", IoCount = 24, Protocol = "CAN", StockQty = 3 }
            );
            db.SaveChanges();
            FixSequence(db, "controller_models");
        }

        if (!db.Users.Any())
        {
            db.Users.AddRange(
                new User { Id = 1, FullName = "Мустафина Ляйсан Рашитовна", Email = "l.mustafina@incomsystem.local", Role = "Manager" },
                new User { Id = 2, FullName = "Ахметов Рустем Данилович", Email = "r.akhmetov@incomsystem.local", Role = "Engineer" },
                new User { Id = 3, FullName = "Сафина Гульнара Маратовна", Email = "g.safina@incomsystem.local", Role = "Engineer" },
                new User { Id = 4, FullName = "Фаттахов Ренат Ильгизович", Email = "r.fattakhov@incomsystem.local", Role = "Chief" },
                new User { Id = 5, FullName = "Хайруллин Тимур Азатович", Email = "t.khairullin@incomsystem.local", Role = "Engineer" }
            );
            db.SaveChanges();
            FixSequence(db, "users");
        }

        if (!db.Orders.Any())
        {
            db.Orders.AddRange(
                new Order
                {
                    Id = 1,
                    Number = "ЗК-2026-0001",
                    Title = "Контроллеры для линии фасовки",
                    Description = "10 шт., интерфейс Modbus RTU, исполнение для цеха с повышенной температурой",
                    ModelId = 1,
                    Status = "New",
                    CreatedAt = new DateTimeOffset(2026, 11, 1, 9, 15, 0, TimeSpan.FromHours(3)).ToUniversalTime(),
                    CreatedByUserId = 1,
                    AssigneeUserId = null
                },
                new Order
                {
                    Id = 2,
                    Number = "ЗК-2026-0002",
                    Title = "Модули аналогового ввода для насосной станции",
                    Description = "4 шт., входы 4–20 мА, монтаж на DIN-рейку, поставка вместе с паспортами",
                    ModelId = 4,
                    Status = "InProgress",
                    CreatedAt = new DateTimeOffset(2026, 11, 2, 10, 30, 0, TimeSpan.FromHours(3)).ToUniversalTime(),
                    CreatedByUserId = 1,
                    AssigneeUserId = 2
                },
                new Order
                {
                    Id = 3,
                    Number = "ЗК-2026-0003",
                    Title = "Контроллер локальной автоматики КСУ",
                    Description = "2 шт., интерфейс CAN, комплект кабелей подключения",
                    ModelId = 5,
                    Status = "Closed",
                    CreatedAt = new DateTimeOffset(2026, 11, 3, 14, 20, 0, TimeSpan.FromHours(3)).ToUniversalTime(),
                    CreatedByUserId = 1,
                    AssigneeUserId = 3
                },
                new Order
                {
                    Id = 4,
                    Number = "ЗК-2026-0004",
                    Title = "Партия ПЛК-120 для котельной",
                    Description = "5 шт., интерфейс Modbus TCP; заказ отменён заказчиком до начала сборки",
                    ModelId = 2,
                    Status = "Cancelled",
                    CreatedAt = new DateTimeOffset(2026, 11, 4, 16, 45, 0, TimeSpan.FromHours(3)).ToUniversalTime(),
                    CreatedByUserId = 1,
                    AssigneeUserId = null
                },
                new Order
                {
                    Id = 5,
                    Number = "ЗК-2026-0005",
                    Title = "Модули ввода-вывода для компрессорной станции",
                    Description = "8 шт., 16 каналов дискретного ввода-вывода, интерфейс Modbus RTU",
                    ModelId = 3,
                    Status = "New",
                    CreatedAt = new DateTimeOffset(2026, 11, 5, 11, 0, 0, TimeSpan.FromHours(3)).ToUniversalTime(),
                    CreatedByUserId = 1,
                    AssigneeUserId = null
                }
            );
            db.SaveChanges();
            FixSequence(db, "orders");
        }
    }
}
