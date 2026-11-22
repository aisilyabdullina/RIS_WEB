using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<ControllerModel> ControllerModels => Set<ControllerModel>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Order> Orders => Set<Order>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Справочник моделей контроллеров (controller_models)
        modelBuilder.Entity<ControllerModel>(entity =>
        {
            entity.ToTable("controller_models");
            entity.HasKey(m => m.Id);

            entity.Property(m => m.Id)
                .HasColumnName("id")
                .UseIdentityByDefaultColumn();

            entity.Property(m => m.Name)
                .HasColumnName("name")
                .IsRequired();

            entity.Property(m => m.Article)
                .HasColumnName("article")
                .IsRequired();

            entity.Property(m => m.ControllerType)
                .HasColumnName("controller_type")
                .IsRequired();

            entity.Property(m => m.IoCount)
                .HasColumnName("io_count")
                .IsRequired();

            entity.Property(m => m.Protocol)
                .HasColumnName("protocol")
                .IsRequired();

            entity.Property(m => m.StockQty)
                .HasColumnName("stock_qty")
                .HasDefaultValue(0)
                .IsRequired();

            entity.HasIndex(m => m.Name).IsUnique();
            entity.HasIndex(m => m.Article).IsUnique();
        });

        // Пользователи (users)
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users", t =>
            {
                t.HasCheckConstraint(
                    "users_role_check",
                    "role IN ('Manager', 'Chief', 'Engineer')");
            });

            entity.HasKey(u => u.Id);

            entity.Property(u => u.Id)
                .HasColumnName("id")
                .UseIdentityByDefaultColumn();

            entity.Property(u => u.FullName)
                .HasColumnName("full_name")
                .IsRequired();

            entity.Property(u => u.Email)
                .HasColumnName("email")
                .IsRequired();

            entity.Property(u => u.Role)
                .HasColumnName("role")
                .IsRequired();

            entity.HasIndex(u => u.Email).IsUnique();
        });

        // Главный объект — заказы на контроллеры (orders)
        modelBuilder.Entity<Order>(entity =>
        {
            entity.ToTable("orders", t =>
            {
                t.HasCheckConstraint(
                    "orders_status_check",
                    "status IN ('New', 'InProgress', 'Closed', 'Cancelled')");
                t.HasCheckConstraint(
                    "orders_title_len_check",
                    "char_length(btrim(title)) BETWEEN 5 AND 80");
                t.HasCheckConstraint(
                    "orders_description_len_check",
                    "char_length(btrim(description)) BETWEEN 10 AND 500");
            });

            entity.HasKey(o => o.Id);

            entity.Property(o => o.Id)
                .HasColumnName("id")
                .UseIdentityByDefaultColumn();

            entity.Property(o => o.Number)
                .HasColumnName("number")
                .IsRequired();

            entity.Property(o => o.Title)
                .HasColumnName("title")
                .HasMaxLength(80)
                .IsRequired();

            entity.Property(o => o.Description)
                .HasColumnName("description")
                .HasMaxLength(500)
                .IsRequired();

            entity.Property(o => o.ModelId)
                .HasColumnName("model_id")
                .IsRequired();

            entity.Property(o => o.Status)
                .HasColumnName("status")
                .HasDefaultValue("New")
                .IsRequired();

            entity.Property(o => o.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("now()")
                .IsRequired();

            entity.Property(o => o.CreatedByUserId)
                .HasColumnName("created_by_user_id")
                .IsRequired();

            entity.Property(o => o.AssigneeUserId)
                .HasColumnName("assignee_user_id")
                .IsRequired(false);

            entity.HasIndex(o => o.Number).IsUnique();

            // Индексы под частые запросы: выборка по статусу и соединение со справочником моделей
            entity.HasIndex(o => o.Status)
                .HasDatabaseName("orders_status_idx");

            entity.HasIndex(o => o.ModelId)
                .HasDatabaseName("orders_model_id_idx");

            entity.HasOne(o => o.ControllerModel)
                .WithMany(m => m.Orders)
                .HasForeignKey(o => o.ModelId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(o => o.CreatedByUser)
                .WithMany(u => u.CreatedOrders)
                .HasForeignKey(o => o.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(o => o.AssigneeUser)
                .WithMany(u => u.AssignedOrders)
                .HasForeignKey(o => o.AssigneeUserId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
}
