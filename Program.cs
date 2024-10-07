
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;

public static class Program
{
    private static MoneyTracker _moneyTracker = new MoneyTracker();

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
                    $"[{(item.ItemType == ItemType.Expense ? "red" : "green")}]"
                    + $"{(item.ItemType == ItemType.Expense ? "-" : "")}{item.Amount:C2}[/]",
                    item.Date.ToString("MMMM dd, yyyy"),
                    $"[{(item.ItemType == ItemType.Expense ? "red" : "green")}] {item.ItemType} [/]"
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
                .Title("[bold yellow]\nSelect an option:[/]");

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
        string? title = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(title))
        {
            Console.WriteLine("Title cannot be empty.");
            return;
        }

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

        Item? existingItem = moneyTracker.Items.FirstOrDefault(i => i.ItemId == itemId); // Make existingItem nullable

        if (existingItem != null) // Check if existingItem is not null
        {
            var editOrDeletePrompt = new SelectionPrompt<string>()
                .Title("[bold yellow]\nWould you like to edit or delete this item?[/]")
                .AddChoices(new[] { "Edit", "Delete" });

            string action = AnsiConsole.Prompt(editOrDeletePrompt);

            if (action == "Edit")
            {
                Console.WriteLine($"Current Title: {existingItem.Title}");
                Console.Write("Enter new title (leave blank to keep current): ");

                string? newTitle = Console.ReadLine();
                newTitle = string.IsNullOrWhiteSpace(newTitle) ? existingItem.Title : newTitle;

                Console.WriteLine($"Current Amount: {existingItem.Amount}");
                Console.Write("Enter new amount (leave blank to keep current): ");

                float newAmount = existingItem.Amount;
                string? newAmountInput = Console.ReadLine();

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
                    .Title($"[red]Are you sure you want to delete the item '[blue]{existingItem.Title}[/]'?[/]")
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

                    AnsiConsole.MarkupLine($"Deleted item: [blue]{existingItem.Title}[/]");
                }
                else
                {
                    AnsiConsole.MarkupLine("Deletion canceled.");
                }
            }
        }
        else
        {
            Console.WriteLine("Item not found."); // Handle case when existingItem is null
        }
    }
}
