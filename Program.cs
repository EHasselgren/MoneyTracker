
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

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

public class MoneyTracker
{
    public float Balance { get; set; }
    public List<Item> Items { get; set; } = new List<Item>();

    private void CalculateInitialBalance()
    {
        Balance = Items.Sum(item => item.ItemType == ItemType.Income ? item.Amount : -item.Amount);
    }
    public void LoadItems()
    {
        try
        {
            string jsonString = File.ReadAllText("items.json");
            Items = JsonSerializer.Deserialize<List<Item>>(jsonString);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading items: {ex.Message}");
        }
    }

    public MoneyTracker()
    {
        LoadItems();
        CalculateInitialBalance();
    }

    public void SaveItems()
    {
        try
        {
            string jsonString = JsonSerializer.Serialize(Items);
            File.WriteAllText("items.json", jsonString);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving items: {ex.Message}");
        }
    }

    public void AddItem(Item item)
    {
        item.ItemId = Items.Count + 1;
        Items.Add(item);

        if (item.Amount > 0)
        {
            Balance += item.Amount;
        }
        else
        {
            Balance -= Math.Abs(item.Amount);
        }

        SaveItems();
    }

    public void EditItem(int itemId, Item newItem)
    {
        var existingItem = Items.FirstOrDefault(i => i.ItemId == itemId);
        if (existingItem != null /*&& newItem.IsValid()*/)
        {
            Balance -= existingItem.Amount;
            existingItem.Title = newItem.Title;
            existingItem.Amount = newItem.Amount;
            existingItem.Date = newItem.Date;
            existingItem.ItemType = newItem.ItemType;

            if (existingItem.ItemType != newItem.ItemType)
            {
                Balance += (newItem.ItemType == ItemType.Income ? newItem.Amount : -newItem.Amount);
            }
            else
            {
                Balance += (newItem.ItemType == ItemType.Income ? newItem.Amount : -newItem.Amount);
            }

            SaveItems();
        }
        else
        {
            Console.WriteLine("Item not found or invalid new item.");
        }
    }

}

public static class Program
{
    private static MoneyTracker _moneyTracker;

    public static void Main(string[] args)
    {
        _moneyTracker = new MoneyTracker();
        _moneyTracker.LoadItems();

        while (true)
        {
            Console.Clear();

            var itemsTable = new Table()
                .AddColumn("[white]ID[/]")
                .AddColumn("[white]Title[/]")
                .AddColumn("[white]Amount[/]")
                .AddColumn("[white]Month[/]")
                .AddColumn("[white]Type[/]");

            foreach (var item in _moneyTracker.Items)
            {
                itemsTable.AddRow(
                    item.ItemId.ToString(),
                    item.Title,
                    $"[green]{(item.ItemType == ItemType.Expense ? "-" : "")}{item.Amount:C2}[/]",
                    item.Date.ToString("MMMM dd, yyyy"),
                    $"[{Color.Blue}] {item.ItemType} [/]"
                );
            }

            var itemsPanel = new Panel(itemsTable)
            {
                Border = BoxBorder.Square,
                Header = new PanelHeader("Items")
            };

            var balanceTable = new Table()
                .AddColumn("[white]Total Balance[/]");

            balanceTable.AddRow($"[yellow]{_moneyTracker.Balance:C2}[/]");

            var balancePanel = new Panel(balanceTable)
            {
                Border = BoxBorder.Square,
                Header = new PanelHeader("Balance")
            };

            var columnsLayout = new Columns(itemsPanel, balancePanel);

            var mainPanel = new Panel(new Rows(columnsLayout))
            {
                Border = BoxBorder.Rounded,
                Padding = new Padding(2, 1),
                Header = new PanelHeader("[bold yellow]Money Tracker[/]")
            };

            AnsiConsole.Write(mainPanel);

            var options = new List<string>
            {
                "Add New Expense/Income",
                "Edit Item (edit, remove)",
                "Save and Quit"
            };

            var selectionPrompt = new SelectionPrompt<string>()
                .PageSize(4)
                .AddChoices(options)
                .Title("[bold yellow]Select an option:[/]");

            var selection = AnsiConsole.Prompt(selectionPrompt);

            switch (selection)
            {
                case "Add New Expense/Income":
                    AddNewItem(_moneyTracker);
                    break;
                case "Edit Item (edit, remove)":
                    EditItem(_moneyTracker);
                    break;
                case "Save and Quit":
                    _moneyTracker.SaveItems();
                    return;
            }
        }
    }

    private static void AddNewItem(MoneyTracker moneyTracker)
    {
        Console.Write("Enter title: ");
        string title = Console.ReadLine();
        Console.Write("Enter amount: ");
        float amount = Convert.ToSingle(Console.ReadLine());

        ItemType itemType = (amount > 0) ? ItemType.Income : ItemType.Expense;

        DateTime currentDate = DateTime.Now;

        int itemId = (moneyTracker.Items.Count > 0) ? moneyTracker.Items.Max(i => i.ItemId) + 1 : 1;

        Item newItem = new Item(itemId, title, Math.Abs(amount), currentDate, itemType);
        moneyTracker.AddItem(newItem);

        AnsiConsole.MarkupLine($"Added new item: {newItem.Title}");
    }

    private static void EditItem(MoneyTracker moneyTracker)
    {
        Console.Write("Enter ID of item to edit or delete: ");
        int itemId = Convert.ToInt32(Console.ReadLine());

        Item existingItem = moneyTracker.Items.FirstOrDefault(i => i.ItemId == itemId);

        if (existingItem != null)
        {
            var editOrDeletePrompt = new SelectionPrompt<string>()
                .Title("[bold yellow]Would you like to edit or delete this item?[/]")
                .AddChoices(new[] { "Edit", "Delete" });

            string action = AnsiConsole.Prompt(editOrDeletePrompt);

            if (action == "Edit")
            {
                Console.WriteLine($"Current Title: {existingItem.Title}");
                Console.Write("Enter new title (leave blank to keep current): ");

                string newTitle = Console.ReadLine();
                newTitle = string.IsNullOrWhiteSpace(newTitle) ? existingItem.Title : newTitle;

                Console.WriteLine($"Current Amount: {existingItem.Amount}");
                Console.Write("Enter new amount (leave blank to keep current): ");

                string newAmountInput = Console.ReadLine();

                float newAmount = existingItem.Amount;
                if (!string.IsNullOrWhiteSpace(newAmountInput))
                {
                    newAmount = Convert.ToSingle(newAmountInput);
                }

                ItemType newItemType = (newAmount > 0) ? ItemType.Income : ItemType.Expense;
                DateTime newDate = existingItem.Date;

                Item newItem = new Item(itemId, newTitle, Math.Abs(newAmount), newDate, newItemType);
                moneyTracker.EditItem(itemId, newItem);

                AnsiConsole.MarkupLine($"Edited item: {newItem.Title}");
            }
            else if (action == "Delete")
            {
                var confirmDeletePrompt = new SelectionPrompt<string>()
                    .Title($"[red]Are you sure you want to delete the item '{existingItem.Title}'?[/]")
                    .AddChoices(new[] { "Yes", "No" });

                string confirmAction = AnsiConsole.Prompt(confirmDeletePrompt);
                if (confirmAction == "Yes")
                {
                    if (existingItem.ItemType == ItemType.Income)
                    {
                        moneyTracker.Balance -= existingItem.Amount;
                    }
                    else if (existingItem.ItemType == ItemType.Expense)
                    {
                        moneyTracker.Balance += existingItem.Amount;
                    }

                    moneyTracker.Items.Remove(existingItem);
                    moneyTracker.SaveItems();

                    AnsiConsole.MarkupLine($"Deleted item: {existingItem.Title}");
                }
                else
                {
                    AnsiConsole.MarkupLine("Deletion canceled.");
                }
            }
        }
        else
        {
            Console.WriteLine("Item not found.");
        }
    }
}