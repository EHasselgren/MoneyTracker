using Spectre.Console;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using MoneyTracker.Enums;
using MoneyTracker.Models;

namespace MoneyTracker.Services
{
    public class DisplayService
    {
        private readonly MoneyTrackerService _moneyTracker;

        public DisplayService(MoneyTrackerService moneyTracker)
        {
            _moneyTracker = moneyTracker;
        }

        public void DisplayItemsAndBalance(ItemType? filterType = null)
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

            if (!itemsToDisplay.Any())
            {
                AnsiConsole.MarkupLine("[yellow]No items to display.[/]");
                return;
            }

            var balanceTable = CreateBalanceTable(filterType);
            var dateRangeTable = CreateDateRangeTable(itemsToDisplay);

            var summaryTable = new Columns(
                new Panel(balanceTable) { Border = BoxBorder.Square },
                new Panel(dateRangeTable) { Border = BoxBorder.Square }
            );

            // change menu header based on ItemType
            var headerTitle = filterType switch
            {
                ItemType.Income => "[bold green]Income Items[/]",
                ItemType.Expense => "[bold red]Expense Items[/]",
                _ => "[bold yellow]All Transactions[/]"
            };

            var mainPanel = new Panel(new Rows(
                new Panel(itemsTable) { Border = BoxBorder.Square, Header = new PanelHeader(headerTitle) },
                summaryTable
            ))
            {
                Border = BoxBorder.Rounded,
                Padding = new Padding(2, 1),
                Header = new PanelHeader("[bold italic slowblink #85BB65]Money Tracker[/]")
            };

            AnsiConsole.Write(mainPanel);
        }

        private Table CreateBalanceTable(ItemType? filterType)
        {
            var balanceTable = new Table().AddColumn("[yellow bold]Total Balance[/]");
            balanceTable.AddRow($"[white]{_moneyTracker.Balance:C2}[/]");

            // filter by income/expense:
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

            return balanceTable;
        }

        private Table CreateDateRangeTable(IEnumerable<Item> itemsToDisplay)
        {
            var oldestDate = itemsToDisplay.Min(item => item.Date);
            var newestDate = itemsToDisplay.Max(item => item.Date);

            var dateRangeTable = new Table().AddColumn("[yellow bold]Date Range[/]");
            dateRangeTable.AddRow($"[white]{new CultureInfo("se-SW").TextInfo.ToTitleCase(oldestDate.ToString("MMMM dd, yyyy").ToLower())} " +
                $"- {new CultureInfo("se-SW").TextInfo.ToTitleCase(newestDate.ToString("MMMM dd, yyyy").ToLower())}[/]");

            return dateRangeTable;
        }
    }
}