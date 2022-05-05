public interface Triggers
{
    public Task StartAsync();
    public List<ActionRunner> GetActions();
}