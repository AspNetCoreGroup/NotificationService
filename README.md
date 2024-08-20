# NotificationService

## Для миграций

```cmd
dotnet ef migrations add MigrationName --startup-project NotificationService.Api --project NotificationService.DataAccess --context DataContext
```

```cmd
dotnet ef database update --startup-project NotificationService.Api --project NotificationService.DataAccess --context DataContext
```
