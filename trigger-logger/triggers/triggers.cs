public interface Triggers
{
    public Task StartAsync();
    public List<ActionRunner> GetActions();
    public void AddAction(ActionRunner action);
}
