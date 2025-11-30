namespace ToDoAplication.Services
{
    public interface IDialogService
    {
        string? ShowDescriptionDialog(string taskText, string currentDescription);
    }
}