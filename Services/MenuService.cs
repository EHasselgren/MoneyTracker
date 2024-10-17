using MoneyTracker.Enums;
using MoneyTracker.Models;
using MoneyTracker.Services;
using Spectre.Console;

public class MenuService
{
    readonly ItemService _itemService;
    readonly DisplayService _displayService;
    readonly InputService _inputService;

    public MenuService(ItemService itemService, DisplayService displayService, InputService inputService)
    {
        _itemService = itemService;
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
            "Edit or Delete Item",
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
        Action? selectedAction = selection switch
        {
            "Add New Item" => AddNewItem,
            "Show Incoming" => () => ShowFilteredItems(ItemType.Income),
            "Show Expenses" => () => ShowFilteredItems(ItemType.Expense),
            "Sort Items" => SortItems,
            "Edit or Delete Item" => EditItem,
            "Print Items List" => _itemService.PrintItemsToFile,
            "Save and Quit" => (Action?)null,
            _ => throw new InvalidOperationException("Invalid selection")
        };

        if (selectedAction == null)
        {
            _itemService.SaveItems();
            return false;
        }

        selectedAction();
        WaitForUser();
        return true;
    }

    void WaitForUser()
    {
        AnsiConsole.WriteLine("Press any key to continue...");
        AnsiConsole.Console.Input.ReadKey(false);
    }

    void ShowFilteredItems(ItemType itemType)
    {
        AnsiConsole.Clear();
        _displayService.DisplayItemsAndBalance(itemType);
    }

    bool IsValidTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            AnsiConsole.MarkupLine("[red]Title cannot be empty.[/]");
            return false;
        }

        return true;
    }

    bool IsValidAmount(float amount)
    {
        if (amount == 0)
        {
            AnsiConsole.MarkupLine("[red]Amount cannot be zero.[/]");
            return false;
        }

        return true;
    }

    void AddNewItem()
    {
        string title = _inputService.PromptForInput("Enter title:");
        if (!IsValidTitle(title)) return;

        float amount = _inputService.PromptForAmount();
        if (!IsValidAmount(amount)) return;

        ItemType itemType = (amount > 0) ? ItemType.Income : ItemType.Expense;
        DateTime currentDate = DateTime.Now;

        int itemId = GetNextAvailableItemId(_itemService.Items);
        Item newItem = new Item(itemId, title, Math.Abs((decimal)amount), currentDate, itemType);
        _itemService.AddItem(newItem);

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

        Item? existingItem = _itemService.Items.FirstOrDefault(i => i.ItemId == itemId);

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
        string newTitle = _inputService.PromptForNewTitle(existingItem.Title);
        float newAmount = _inputService.PromptForAmount();
        ItemType newItemType = (newAmount > 0) ? ItemType.Income : ItemType.Expense;

        Item updatedItem = new Item(existingItem.ItemId, newTitle, Math.Abs((decimal)newAmount), DateTime.Now, newItemType);
        _itemService.EditItem(existingItem.ItemId, updatedItem);

        AnsiConsole.MarkupLine($"[bold yellow]\nUpdated item:[/] [blue]{updatedItem.Title}[/]");
    }

    void DeleteExistingItem(Item existingItem)
    {
        _itemService.Items.Remove(existingItem);
        AnsiConsole.MarkupLine($"[bold yellow]\nDeleted item:[/] [blue]{existingItem.Title}[/]");
    }

    void SortItems()
    {
        List<string> sortOptions = new List<string> { "Sort by ID", "Sort by Title", "Sort by Amount", "Sort by Date" };
        string sortBy = _inputService.PromptForSortOption(sortOptions);
        string direction = _inputService.PromptForSortDirection();

        List<Item> sortedItems = PerformItemSorting(_itemService.Items, sortBy, direction).ToList();
        _itemService.Items = sortedItems;

        AnsiConsole.MarkupLine("[bold yellow]\nItems sorted successfully.[/]");
    }

    IEnumerable<Item> PerformItemSorting(IEnumerable<Item> items, string sortBy, string direction)
    {
        Func<Item, object> sortKeySelector = sortBy switch
        {
            "Sort by ID" => (Func<Item, object>)(i => i.ItemId),
            "Sort by Title" => (Func<Item, object>)(i => i.Title),
            "Sort by Amount" => (Func<Item, object>)(i => i.ItemType == ItemType.Income ? i.Amount : -i.Amount),
            "Sort by Date" => (Func<Item, object>)(i => i.Date),
            _ => throw new ArgumentOutOfRangeException(nameof(sortBy), "Invalid sort option")
        };

        return direction == "Ascending"
            ? items.OrderBy(sortKeySelector)
            : items.OrderByDescending(sortKeySelector);
    }
}