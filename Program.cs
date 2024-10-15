
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.IO;
using System.Linq;
using System.Collections.Generic;

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
            "Add New Item",
            "Sort Items",
            "Show Incoming",
            "Show Expenses",
            "Edit or Delete an Item",
            "Print Items List",
            "Save and Quit"
        };

            var selectionPrompt = new SelectionPrompt<string>()
                .PageSize(7)
                .AddChoices(options)
                .Title("[bold yellow]\nSelect an option:[/]");

            var selection = AnsiConsole.Prompt(selectionPrompt);

            switch (selection)
            {
                case "Add New Item":
                    AddNewItem(_moneyTracker);
                    break;

                case "Show Incoming":
                    AnsiConsole.Clear();
                    DisplayItemsAndBalance(ItemType.Income);
                    break;

                case "Show Expenses":
                    AnsiConsole.Clear();
                    DisplayItemsAndBalance(ItemType.Expense);
                    break;

                case "Sort Items":
                    SortItems();
                    break;

                case "Edit or Delete an Item":
                    EditItem(_moneyTracker);
                    break;

                case "Print Items List":
                    _moneyTracker.PrintItemsToFile();
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
                new CultureInfo("se-SW").TextInfo.ToTitleCase(item.Date.ToString("MMMM dd, yyyy").ToLower()),
                $"[{(item.ItemType == ItemType.Expense ? "red" : "green")}] {item.ItemType} [/]"
            );
        }

        var balanceTable = new Table().AddColumn("[yellow bold]Total Balance[/]");
        balanceTable.AddRow($"[white]{_moneyTracker.Balance:C2}[/]");

        if (filterType == ItemType.Income)
        {
            var totalIncome = _moneyTracker.GetFilteredItems(ItemType.Income).Sum(item => item.Amount);
            balanceTable = new Table()
                .AddColumn("[green bold]Total Income[/]")
                .AddRow($"[white]{totalIncome:C2}[/]");
        }
        else if (filterType == ItemType.Expense)
        {
            var totalExpenses = _moneyTracker.GetFilteredItems(ItemType.Expense).Sum(item => item.Amount);
            balanceTable = new Table()
                .AddColumn("[red bold]Total Expenses[/]")
                .AddRow($"[white]{-totalExpenses:C2}[/]");
        }

        var oldestDate = itemsToDisplay.Min(item => item.Date);
        var newestDate = itemsToDisplay.Max(item => item.Date);

        var dateRangeTable = new Table().AddColumn("[yellow bold]Date Range[/]");
        dateRangeTable.AddRow($"[white]{new CultureInfo("se-SW").TextInfo.ToTitleCase(oldestDate.ToString("MMMM dd, yyyy").ToLower())} - {new CultureInfo("se-SW").TextInfo.ToTitleCase(newestDate.ToString("MMMM dd, yyyy").ToLower())}[/]");

        var summaryTable = new Columns(
            new Panel(balanceTable) { Border = BoxBorder.Square },
            new Panel(dateRangeTable) { Border = BoxBorder.Square }
        );

        var headerTitle = filterType switch
        {
            ItemType.Income => "[bold yellow]Income Items[/]",
            ItemType.Expense => "[bold yellow]Expense Items[/]",
            _ => "[bold yellow]All Transactions[/]"
        };

        var mainPanel = new Panel(new Rows(
            summaryTable,
            new Panel(itemsTable) { Border = BoxBorder.Square, Header = new PanelHeader(headerTitle) }
        ))
        {
            Border = BoxBorder.Rounded,
            Padding = new Padding(2, 1),
            Header = new PanelHeader("[bold yellow]Money Tracker[/]")
        };

        AnsiConsole.Write(mainPanel);
    }




    private static void SortItems(ItemType? filterType = null)
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
            .PageSize(sortOptions.Count > 3 ? sortOptions.Count : 3) //PageSize has some issues when it gets below 3 so I hardcoded this :S 
            .AddChoices(sortOptions)
            .Title("[bold yellow]\nSelect a sorting option:[/]");

        var sortSelection = AnsiConsole.Prompt(sortPrompt);

        // create list depending on user selection in sorting menu
        IEnumerable<Item> itemsToSort = filterType.HasValue
            ? _moneyTracker.Items.Where(i => i.ItemType == filterType.Value)
            : _moneyTracker.Items;

        // ask for sorting direction
        var directionOptions = new List<string> { "Ascending", "Descending" };
        var directionPrompt = new SelectionPrompt<string>()
            .PageSize(directionOptions.Count > 3 ? directionOptions.Count : 3) // same as other pageSize
            .AddChoices(directionOptions)
            .Title("[bold yellow]\nSelect sorting direction:[/]");

        var directionSelection = AnsiConsole.Prompt(directionPrompt);

        // Apply sorting based on selection
        switch (sortSelection)
        {
            case "Sort by ID":
                itemsToSort = directionSelection == "Ascending"
                    ? itemsToSort.OrderBy(i => i.ItemId)
                    : itemsToSort.OrderByDescending(i => i.ItemId);
                break;
            case "Sort by Title":
                itemsToSort = directionSelection == "Ascending"
                    ? itemsToSort.OrderBy(i => i.Title)
                    : itemsToSort.OrderByDescending(i => i.Title);
                break;
            case "Sort by Amount":
                itemsToSort = directionSelection == "Ascending"
                    ? itemsToSort.OrderBy(i => i.ItemType == ItemType.Expense ? -i.Amount : i.Amount)
                    : itemsToSort.OrderByDescending(i => i.ItemType == ItemType.Expense ? -i.Amount : i.Amount);
                break;
            case "Sort by Month":
                itemsToSort = directionSelection == "Ascending"
                    ? itemsToSort.OrderBy(i => i.Date)
                    : itemsToSort.OrderByDescending(i => i.Date);
                break;
            case "Go Back":
                return;
        }

        // create list from our filtered items and assign it to _moneyTracker.Items
        _moneyTracker.Items = itemsToSort.ToList();
        AnsiConsole.Clear();
        // display items based on filterType
        DisplayItemsAndBalance(filterType);
    }

    private static void AddNewItem(MoneyTracker moneyTracker)
    {
        AnsiConsole.MarkupLine($"[yellow]\nEnter title:[/] ");
        string? title = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(title))
        {
            AnsiConsole.WriteLine("Title cannot be empty.");
            return;
        }

        AnsiConsole.MarkupLine($"[bold yellow]\nEnter amount:[/] ");
        float amount = Convert.ToSingle(Console.ReadLine());

        ItemType itemType = (amount > 0) ? ItemType.Income : ItemType.Expense;

        DateTime currentDate = DateTime.Now;

        int itemId = (moneyTracker.Items.Count > 0) ? moneyTracker.Items.Max(i => i.ItemId) + 1 : 1;

        Item newItem = new Item(itemId, title, Math.Abs((decimal)amount), currentDate, itemType);
        moneyTracker.AddItem(newItem);

        AnsiConsole.MarkupLine($"Added new item: [blue]{newItem.Title}[/]");
    }

    private static void EditItem(MoneyTracker moneyTracker)
    {
        AnsiConsole.MarkupLine($"[bold yellow]\nEnter ID of item to edit or delete:[/] ");
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
                AnsiConsole.MarkupLine($"Current Title: {existingItem.Title}");
                AnsiConsole.MarkupLine($"[bold yellow]\nEnter new Title[/] [bold white]\n(leave blank to keep current):[/]");

                string? newTitle = Console.ReadLine();
                newTitle = string.IsNullOrWhiteSpace(newTitle) ? existingItem.Title : newTitle;

                AnsiConsole.MarkupLine($"Current Amount: {existingItem.Amount}");
                AnsiConsole.MarkupLine($"[bold yellow]\nEnter new Amount[/] [bold white]\n(leave blank to keep current):[/]");

                decimal newAmount = existingItem.Amount; string? newAmountInput = Console.ReadLine();

                if (!string.IsNullOrWhiteSpace(newAmountInput))
                {
                    if (decimal.TryParse(newAmountInput, out newAmount))
                    {
                        existingItem.Amount = newAmount;
                    }
                    else
                    {
                        AnsiConsole.MarkupLine("\nInvalid input. Please enter a valid decimal amount.");
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
                    AnsiConsole.MarkupLine("\nDeletion canceled.");
                }
            }
        }
        else
        {
            AnsiConsole.MarkupLine("\nItem not found.");
        }
    }
}
