
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;

public static class Program
{
    private static MoneyTracker _moneyTracker = new MoneyTracker();

    public static void Main(string[] args)
    {
        _moneyTracker.LoadItems();

        while (true)
        {
            AnsiConsole.Clear();

            DisplayItemsAndBalance();

            var options = new List<string>
        {
            "Show Income Items",
            "Show Expense Items",
            "Sort Items",
            "Add New Item",
            "Edit or Delete Item",
            "Save and Quit"
        };

            var selectionPrompt = new SelectionPrompt<string>()
                .PageSize(6)
                .AddChoices(options)
                .Title("[bold yellow]\nSelect an option:[/]");

            var selection = AnsiConsole.Prompt(selectionPrompt);

            switch (selection)
            {
                case "Show Income Items":
                    AnsiConsole.Clear();
                    DisplayItemsAndBalance(ItemType.Income);
                    break;

                case "Show Expense Items":
                    AnsiConsole.Clear();
                    DisplayItemsAndBalance(ItemType.Expense);
                    break;

                case "Sort Items":
                    SortItems();
                    break;

                case "Add New Item":
                    AddNewItem(_moneyTracker);
                    break;

                case "Edit or Delete Item":
                    EditItem(_moneyTracker);
                    break;

                case "Save and Quit":
                    _moneyTracker.SaveItems();
                    return;
            }

            AnsiConsole.WriteLine("Press any key to continue...");
            AnsiConsole.Console.Input.ReadKey(false);
        }
    }

    private static void DisplayItemsAndBalance(ItemType? filterType = null)
    {
        var itemsTable = new Table()
            .AddColumn("[white]ID[/]")
            .AddColumn("[white]Title[/]")
            .AddColumn("[white]Amount[/]")
            .AddColumn("[white]Month[/]")
            .AddColumn("[white]Type[/]");

        IEnumerable<Item> itemsToDisplay = filterType.HasValue
            ? _moneyTracker.Items.Where(i => i.ItemType == filterType.Value)
            : _moneyTracker.Items;

        foreach (var item in itemsToDisplay)
        {
            itemsTable.AddRow(
                item.ItemId.ToString(),
                item.Title,
                $"[{(item.ItemType == ItemType.Expense ? "red" : "green")}]"
                + $"{(item.ItemType == ItemType.Expense ? "-" : "")}{item.Amount:C2}[/]",
                item.Date.ToString("MMMM dd, yyyy"),
                $"[{(item.ItemType == ItemType.Expense ? "red" : "green")}] {item.ItemType} [/]"
            );
        }

        var balanceTable = new Table()
            .AddColumn("[white]Total Balance[/]");

        balanceTable.AddRow($"[yellow]{_moneyTracker.Balance:C2}[/]");

        if (filterType == ItemType.Income)
        {
            var totalIncome = _moneyTracker.GetFilteredItems(ItemType.Income).Sum(item => item.Amount);
            balanceTable = new Table()
                .AddColumn("[white]Total Income[/]")
                .AddRow($"[green]{totalIncome:C2}[/]");
        }
        else if (filterType == ItemType.Expense)
        {
            var totalExpenses = _moneyTracker.GetFilteredItems(ItemType.Expense).Sum(item => item.Amount);
            balanceTable = new Table()
                .AddColumn("[white]Total Expenses[/]")
                .AddRow($"[red]{-totalExpenses:C2}[/]");
        }

        var columnsLayout = new Columns(
            new Panel(itemsTable) { Border = BoxBorder.Square, Header = new PanelHeader("Items") },
            new Panel(balanceTable) { Border = BoxBorder.Square, Header = new PanelHeader(filterType == ItemType.Income ? "Total Income" : (filterType == ItemType.Expense ? "Total Expenses" : "Total Balance")) }
        );

        var mainPanel = new Panel(new Rows(columnsLayout))
        {
            Border = BoxBorder.Rounded,
            Padding = new Padding(2, 1),
            Header = new PanelHeader("[bold yellow]Money Tracker[/]")
        };

        AnsiConsole.Write(mainPanel);
    }

    private static void SortItems()
    {
        var sortOptions = new List<string>
    {
        "Sort by ID",
        "Sort by Title",
        "Sort by Amount",
        "Sort by Month",
        "Go Back"
    };

        var sortPrompt = new SelectionPrompt<string>()
            .PageSize(5)
            .AddChoices(sortOptions)
            .Title($"[bold yellow]\nSelect a sorting option:[/]");

        var sortSelection = AnsiConsole.Prompt(sortPrompt);

        switch (sortSelection)
        {
            case "Sort by ID":
                _moneyTracker.Items = _moneyTracker.Items.OrderBy(i => i.ItemId).ToList();
                break;
            case "Sort by Title":
                _moneyTracker.Items = _moneyTracker.Items.OrderBy(i => i.Title).ToList();
                break;
            case "Sort by Amount":
                _moneyTracker.Items = _moneyTracker.Items.OrderBy(i => i.Amount).ToList();
                break;
            case "Sort by Month":
                _moneyTracker.Items = _moneyTracker.Items.OrderBy(i => i.Date).ToList();
                break;
            case "Go Back":
                return;
        }

        AnsiConsole.Clear();
        DisplayItemsAndBalance();
    }

    private static void AddNewItem(MoneyTracker moneyTracker)
    {
        AnsiConsole.Write($"[yellow]\nEnter title:[/] ");
        string? title = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(title))
        {
            AnsiConsole.WriteLine("Title cannot be empty.");
            return;
        }

        AnsiConsole.Write($"[bold yellow]\nEnter amount:[/] ");
        float amount = Convert.ToSingle(Console.ReadLine());

        ItemType itemType = (amount > 0) ? ItemType.Income : ItemType.Expense;

        DateTime currentDate = DateTime.Now;

        int itemId = (moneyTracker.Items.Count > 0) ? moneyTracker.Items.Max(i => i.ItemId) + 1 : 1;

        Item newItem = new Item(itemId, title, Math.Abs((decimal)amount), currentDate, itemType);
        moneyTracker.AddItem(newItem);

        AnsiConsole.MarkupLine($"Added new item: {newItem.Title}");
    }

    private static void EditItem(MoneyTracker moneyTracker)
    {
        AnsiConsole.Write($"[bold yellow]\nEnter ID of item to edit or delete:[/] ");
        int itemId = Convert.ToInt32(Console.ReadLine());

        Item? existingItem = moneyTracker.Items.FirstOrDefault(i => i.ItemId == itemId);

        if (existingItem != null)
        {
            var editOrDeletePrompt = new SelectionPrompt<string>()
                .Title($"[bold yellow]\nWould you like to edit or delete this item?[/]")
                .AddChoices(new[] { "Edit", "Delete" });

            string action = AnsiConsole.Prompt(editOrDeletePrompt);

            if (action == "Edit")
            {
                AnsiConsole.WriteLine($"Current Title: {existingItem.Title}");
                AnsiConsole.Write($"[bold yellow]\nEnter new Title[/] [bold white]\n(leave blank to keep current):[/]");

                string? newTitle = Console.ReadLine();
                newTitle = string.IsNullOrWhiteSpace(newTitle) ? existingItem.Title : newTitle;

                AnsiConsole.WriteLine($"Current Amount: {existingItem.Amount}");
                AnsiConsole.Write($"[bold yellow]\nEnter new Amount[/] [bold white]\n(leave blank to keep current):[/]");

                decimal newAmount = existingItem.Amount; string? newAmountInput = Console.ReadLine();

                if (!string.IsNullOrWhiteSpace(newAmountInput))
                {

                    if (decimal.TryParse(newAmountInput, out newAmount))
                    {
                        existingItem.Amount = newAmount;
                    }
                    else
                    {
                        AnsiConsole.WriteLine("Invalid input. Please enter a valid decimal amount.");
                    }
                }


                ItemType newItemType = (newAmount > 0) ? ItemType.Income : ItemType.Expense;
                DateTime newDate = existingItem.Date;

                Item newItem = new Item(itemId, newTitle, Math.Abs(newAmount), newDate, newItemType);
                moneyTracker.EditItem(itemId, newItem);

                AnsiConsole.MarkupLine($"[bold yellow]\nEdited item: [italic]{newItem.Title}[/][/]");
            }
            else if (action == "Delete")
            {
                var confirmDeletePrompt = new SelectionPrompt<string>()
                    .Title($"[red]Are you sure you want to delete the item '[italic blue]{existingItem.Title}[/]'?[/]")
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

                    AnsiConsole.MarkupLine($"[bold yellow]\nDeleted item:[/] [italic blue]{existingItem.Title}[/]");
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
