
using Spectre.Console;
using System;
using System.Globalization;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using MoneyTracker.Models;
using MoneyTracker.Services;
using MoneyTracker.Enums;

namespace MoneyTracker
{
    public static class Program
    {
        private static MoneyTrackerService _moneyTracker = new MoneyTrackerService();
        private static DisplayService _displayService = new DisplayService(_moneyTracker);
        private static InputService _inputService = new InputService();

        public static void Main(string[] args)
        {
            _moneyTracker.LoadItems();

            bool running = true;
            while (running)
            {
                AnsiConsole.Clear();
                _displayService.DisplayItemsAndBalance();

                var selection = GetMenuSelection();
                running = HandleMenuSelection(selection);
            }
        }

        private static string GetMenuSelection()
        {
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

            return AnsiConsole.Prompt(selectionPrompt);
        }

        private static bool HandleMenuSelection(string selection)
        {
            switch (selection)
            {
                case "Add New Item":
                    AddNewItem(_moneyTracker);
                    break;

                case "Show Incoming":
                    ShowFilteredItems(ItemType.Income);
                    break;

                case "Show Expenses":
                    ShowFilteredItems(ItemType.Expense);
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
                    return false;
            }

            AnsiConsole.WriteLine("Press any key to continue...");
            AnsiConsole.Console.Input.ReadKey(false);
            return true;
        }

        private static void ShowFilteredItems(ItemType itemType)
        {
            AnsiConsole.Clear();
            _displayService.DisplayItemsAndBalance(itemType);
        }

        private static void AddNewItem(MoneyTrackerService moneyTracker)
        {
            string title = _inputService.PromptForInput("Enter title:");
            if (string.IsNullOrWhiteSpace(title)) return;

            float amount = _inputService.PromptForAmount();
            if (amount == 0) return;

            ItemType itemType = (amount > 0) ? ItemType.Income : ItemType.Expense;
            DateTime currentDate = DateTime.Now;

            int itemId = GetNextAvailableItemId(moneyTracker.Items);
            Item newItem = new Item(itemId, title, Math.Abs((decimal)amount), currentDate, itemType);
            moneyTracker.AddItem(newItem);

            AnsiConsole.MarkupLine($"[bold yellow]\nAdded new item:[/] [blue]{newItem.Title}[/]");
        }

        private static int GetNextAvailableItemId(List<Item> items)
        {
            if (!items.Any()) return 1;

            var existingIds = new HashSet<int>(items.Select(i => i.ItemId));

            for (int id = 1; id <= existingIds.Count + 1; id++)
            {
                if (!existingIds.Contains(id))
                {
                    return id;
                }
            }

            return existingIds.Count + 1;
        }

        private static void EditItem(MoneyTrackerService moneyTracker)
        {
            AnsiConsole.MarkupLine($"[bold yellow]\nEnter ID of item to edit or delete:[/] ");
            int itemId = Convert.ToInt32(Console.ReadLine());

            Item? existingItem = moneyTracker.Items.FirstOrDefault(i => i.ItemId == itemId);

            if (existingItem != null)
            {
                var action = _inputService.PromptForEditOrDelete();
                if (action == "Edit")
                {
                    EditExistingItem(existingItem, moneyTracker);
                }
                else if (action == "Delete")
                {
                    DeleteExistingItem(existingItem, moneyTracker);
                }
            }
            else
            {
                AnsiConsole.MarkupLine("\nItem not found.");
            }
        }

        private static void EditExistingItem(Item existingItem, MoneyTrackerService moneyTracker)
        {
            string newTitle = _inputService.PromptForInput("Enter new title (leave blank to keep current):");
            if (string.IsNullOrWhiteSpace(newTitle))
            {
                newTitle = existingItem.Title;
            }

            float newAmount = _inputService.PromptForAmount();
            DateTime currentDate = DateTime.Now;

            ItemType newItemType = (newAmount > 0) ? ItemType.Income : ItemType.Expense;
            Item updatedItem = new Item(existingItem.ItemId, newTitle, Math.Abs((decimal)newAmount), currentDate, newItemType);
            moneyTracker.EditItem(existingItem.ItemId, updatedItem);

            AnsiConsole.MarkupLine($"[bold yellow]\nUpdated item:[/] [blue]{updatedItem.Title}[/]");
        }

        private static void DeleteExistingItem(Item existingItem, MoneyTrackerService moneyTracker)
        {
            moneyTracker.Items.Remove(existingItem);
            AnsiConsole.MarkupLine($"[bold yellow]\nDeleted item:[/] [blue]{existingItem.Title}[/]");
        }

        private static void SortItems()
        {
            var sortOptions = new List<string>
    {
        "Sort by ID",
        "Sort by Title",
        "Sort by Amount",
        "Sort by Month"
    };

            string sortBy = _inputService.PromptForSortOption(sortOptions);
            string direction = _inputService.PromptForSortDirection();

            // Use the static field _moneyTracker to access the items
            var sortedItems = SortItems(_moneyTracker.Items.AsEnumerable(), sortBy, direction);
            _moneyTracker.Items = sortedItems.ToList();
            AnsiConsole.MarkupLine("[bold yellow]\nItems sorted successfully.[/]");
        }

        private static IEnumerable<Item> SortItems(IEnumerable<Item> items, string sortBy, string direction)
        {
            Func<Item, object> sortKeySelector = sortBy switch
            {
                "Sort by ID" => i => i.ItemId,
                "Sort by Title" => i => i.Title,
                "Sort by Amount" => i => i.Amount,
                "Sort by Month" => i => i.Date.Month,
                _ => throw new ArgumentOutOfRangeException()
            };

            return direction == "Ascending"
                ? items.OrderBy(sortKeySelector)
                : items.OrderByDescending(sortKeySelector);
        }
    }
}
