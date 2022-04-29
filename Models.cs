namespace TodoApi;

record UserDto(string Username, string Password);

class Item
{
    public Item()
    {
        Title = string.Empty;
    }
    public int Id { get; set; }
    public string Title { get; set; }
    public bool IsCompleted { get; set; }
}