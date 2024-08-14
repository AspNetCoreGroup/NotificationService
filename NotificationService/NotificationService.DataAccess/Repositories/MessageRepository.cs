namespace NotificationService.DataAccess.Repositories;

public class MessageRepository
{
    private readonly DataContext _dataContext;
    
    public MessageRepository(DataContext dataContext)
    {
        _dataContext = dataContext;
    }
    
    
}