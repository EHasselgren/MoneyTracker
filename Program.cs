using Spectre.Console;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using Spectre.Console.Extensions;
using Spectre.Console.Rendering;

public enum ItemType
{
    Income,
    Expense
}

public enum SortDirection
{
    Ascending,
    Descending
}

public class Item
{
    public int ItemId { get; set; }
    public string Title { get; set; }
    public float Amount { get; set; }
    public string Month { get; set; }
    public ItemType ItemType { get; set; }

    public Item(int itemId, string title, float amount, string month, ItemType itemType)
    {
        ItemId = itemId;
        Title = title;
        Amount = amount;
        Month = month;
        ItemType = itemType;
    }
    public bool IsValid()
    {
        if (Amount <= 0)
        {
            return false;
        }
        else
        {
            return true;
        }
    }
}

public class MoneyTracker
{
    public float Balance { get; set; }
    public List<Item> Items { get; set; } = new List<Item>();

    public void AddItem(Item item)
    {
        //if (item.IsValid())
        //{
        item.ItemId = Items.Count + 1;

        Items.Add(item);

        if (item.ItemType == ItemType.Income)
        {
            Balance += item.Amount;
        }
        else if (item.ItemType == ItemType.Expense)
        {
            Balance -= item.Amount;
        }
        //}
        //else
        //{
        //    Console.WriteLine("Invalid item. Please check the input.");
        //}
    }

    public void EditItem(int itemId, Item newItem)
    {
        var existingItem = Items.FirstOrDefault(i => i.ItemId == itemId);

        if (existingItem != null && newItem.IsValid())
        {
            existingItem.Title = newItem.Title;
            existingItem.Amount = newItem.Amount;
            existingItem.Month = newItem.Month;
            existingItem.ItemType = newItem.ItemType;

            if (existingItem.ItemType != newItem.ItemType)
            {
                Balance -= existingItem.Amount;
                Balance += newItem.Amount;
            }
        }
        else
        {
            AnsiConsole.WriteLine("Item not found or invalid new item.");
        }
    }
}

public static class Program
{
    public static void Main(string[] args)
    {
        var moneyTracker = new MoneyTracker();
        moneyTracker.AddItem(new Item(0, "Salary", 5000, "01", ItemType.Income));
        moneyTracker.AddItem(new Item(0, "Rent", 1000, "01", ItemType.Expense));

        while (true)
        {
            Console.Clear();

            var balance = moneyTracker.Balance;
            AnsiConsole.MarkupLine($"Welcome to Money Tracker! You have [green]{balance:C}[/] in your account.");
            AnsiConsole.WriteLine("Pick an option:");

            var selection = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
           .Title("Main Menu")
           .PageSize(4)
           .AddChoices(new[] {
            "Show items",
            "Add New Expense/Income",
            "Edit Item (edit, remove)",
            "Save and Quit"
           }));


            switch (selection)
            {
                case "Show items":
                    ShowItems(moneyTracker);
                    break;
                case "Add New Expense/Income":
                    // Call the method to add an item (you had placeholders here)
                    // AddNewExpenseOrIncome(moneyTracker);
                    break;
                case "Edit Item (edit, remove)":
                    // Call the method to edit items (you had placeholders here)
                    // EditItem(moneyTracker);
                    break;
                case "Save and Quit":
                    // Implement save and exit logic (if needed)
                    return;
            }

            Console.Clear();
        }
    }

    private static void ShowItems(MoneyTracker moneyTracker)
    {
        var items = moneyTracker.Items.ToList();
        if (!items.Any())
        {
            AnsiConsole.MarkupLine("No expenses found.");
            return;
        }

        foreach (var item in items)
        {

            AnsiConsole.Write(new Table()
                .AddColumn("[white]ID[/]")
                .AddColumn("[white]Title[/]")
                .AddColumn("[white]Amount[/]")
                .AddColumn("[white]Month[/]")
                .AddColumn("[white]Type[/]")
                .AddRow(item.ItemId.ToString(), item.Title, $"[green]{item.Amount:C2}[/]", item.Month, $"[{Color.Blue}] {item.ItemType} [/]")
                .Border(TableBorder.Rounded)
                .BorderColor(Color.Red)
                .Alignment(Justify.Center));
        }
        Console.WriteLine("\nPress any key to return to the main menu...");
        Console.ReadKey();
    }
}
