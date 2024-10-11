using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public static class ItemDisplayHelper
{
    public static void DisplayItems(List<Item> items)
    {
        var itemsTable = new Table()
            .AddColumn("[white]ID[/]")
            .AddColumn("[white]Title[/]")
            .AddColumn("[white]Amount[/]")
            .AddColumn("[white]Month[/]")
            .AddColumn("[white]Type[/]");

        foreach (var item in items)
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

        AnsiConsole.Write(itemsTable);
    }
}


