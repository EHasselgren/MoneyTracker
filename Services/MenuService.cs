using MoneyTracker.Enums;
using MoneyTracker.Models;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyTracker.Services
{
    public class MenuService
    {
        readonly ItemService _moneyTracker;
        readonly DisplayService _displayService;
        readonly InputService _inputService;

        public MenuService(ItemService moneyTracker, DisplayService displayService, InputService inputService)
        {
            _moneyTracker = moneyTracker;
            _displayService = displayService;
            _inputService = inputService;
        }

        public void Start()
        {
            bool running = true;

            while (running)
            {
                AnsiConsole.Clear();
                _displayService.DisplayItemsAndBalance();
                string selection = GetMenuSelection();
                running = HandleMenuSelection(selection);
            }
        }

        string GetMenuSelection()
        {
            List<string> options = new List<string>
        {
            "Add New Item",
            "Sort Items",
            "Show Incoming",
            "Show Expenses",
            "Edit or Delete an Item",
            "Print Items List",
            "Save and Quit"
        };

            SelectionPrompt<string> selectionPrompt = new SelectionPrompt<string>()
                .PageSize(7)
                .AddChoices(options)
                .Title("[bold yellow]\nSelect an option:[/]");

            return AnsiConsole.Prompt(selectionPrompt);
        }

        bool HandleMenuSelection(string selection)
        {
            switch (selection)
            {
                case "Add New Item":
                    AddNewItem();
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
                    EditItem();
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

        void ShowFilteredItems(ItemType itemType)
        {
            AnsiConsole.Clear();
            _displayService.DisplayItemsAndBalance(itemType);
        }

        void AddNewItem()
        {
            string title = _inputService.PromptForInput("Enter title:");
            if (string.IsNullOrWhiteSpace(title)) return;

            float amount = _inputService.PromptForAmount();
            if (amount == 0) return;

            ItemType itemType = (amount > 0) ? ItemType.Income : ItemType.Expense;
            DateTime currentDate = DateTime.Now;

            int itemId = GetNextAvailableItemId(_moneyTracker.Items);
            Item newItem = new Item(itemId, title, Math.Abs((decimal)amount), currentDate, itemType);
            _moneyTracker.AddItem(newItem);

            AnsiConsole.MarkupLine($"[bold yellow]\nAdded new item:[/] [blue]{newItem.Title}[/]");
        }

        int GetNextAvailableItemId(List<Item> items)
        {
            if (!items.Any()) return 1;

            HashSet<int> existingIds = new HashSet<int>(items.Select(i => i.ItemId));

            for (int id = 1; id <= existingIds.Count + 1; id++)
            {
                if (!existingIds.Contains(id))
                {
                    return id;
                }
            }

            return existingIds.Count + 1;
        }

        void EditItem()
        {
            int itemId = _inputService.PromptForItemId("Enter ID of item to edit or delete:");

            Item? existingItem = _moneyTracker.Items.FirstOrDefault(i => i.ItemId == itemId);

            if (existingItem != null)
            {
                string action = _inputService.PromptForEditOrDelete(existingItem.Title);

                if (action == "Edit")
                {
                    EditExistingItem(existingItem);
                }
                else if (action == "Delete")
                {
                    DeleteExistingItem(existingItem);
                }
            }
            else
            {
                AnsiConsole.MarkupLine("\nItem not found.");
            }
        }

        void EditExistingItem(Item existingItem)
        {
            AnsiConsole.MarkupLine($"[yellow]Current title:[/] [blue]{existingItem.Title}[/]");

            string newTitle = AnsiConsole.Ask<string>($"[bold yellow]Enter new title (leave blank to keep current):[/]", existingItem.Title);

            newTitle = string.IsNullOrWhiteSpace(newTitle) ? existingItem.Title : newTitle.Trim();

            float newAmount = _inputService.PromptForAmount();

            ItemType newItemType = (newAmount > 0) ? ItemType.Income : ItemType.Expense;

            Item updatedItem = new Item(existingItem.ItemId, newTitle, Math.Abs((decimal)newAmount), DateTime.Now, newItemType);

            _moneyTracker.EditItem(existingItem.ItemId, updatedItem);

            AnsiConsole.MarkupLine($"[bold yellow]\nUpdated item:[/] [blue]{updatedItem.Title}[/]");
        }

        void DeleteExistingItem(Item existingItem)
        {
            _moneyTracker.Items.Remove(existingItem);
            AnsiConsole.MarkupLine($"[bold yellow]\nDeleted item:[/] [blue]{existingItem.Title}[/]");
        }

        void SortItems()
        {
            List<string> sortOptions = new List<string>
        {
            "Sort by ID",
            "Sort by Title",
            "Sort by Amount",
            "Sort by Month"
        };

            string sortBy = _inputService.PromptForSortOption(sortOptions);
            string direction = _inputService.PromptForSortDirection();

            IEnumerable<Item> sortedItems = SortItems(_moneyTracker.Items.AsEnumerable(), sortBy, direction);
            _moneyTracker.Items = sortedItems.ToList();
            AnsiConsole.MarkupLine("[bold yellow]\nItems sorted successfully.[/]");
        }

        IEnumerable<Item> SortItems(IEnumerable<Item> items, string sortBy, string direction)
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