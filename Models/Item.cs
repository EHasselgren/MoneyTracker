public class Item
{
    public int ItemId { get; set; }
    public string Title { get; set; }
    public float Amount { get; set; }
    public DateTime Date { get; set; }
    public ItemType ItemType { get; set; }

    public Item(int itemId, string title, float amount, DateTime date, ItemType itemType)
    {
        ItemId = itemId;
        Title = title;
        Amount = amount;
        Date = date;
        ItemType = itemType;
    }

    //public bool IsValid()
    //{
    //}
}