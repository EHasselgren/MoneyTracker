using MoneyTracker.Enums;

namespace MoneyTracker.Models
{
    public class Item
    {
        public int ItemId { get; set; }
        public string Title { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public ItemType ItemType { get; set; }

        public Item(int itemId, string title, decimal amount, DateTime date, ItemType itemType)
        {
            ItemId = itemId;
            Title = title;
            Amount = amount;
            Date = date;
            ItemType = itemType;
        }
    }
}
