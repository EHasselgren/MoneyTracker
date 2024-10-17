using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Spectre.Console;

namespace MoneyTracker.Services
{
    public class InputService
    {
        public string PromptForInput(string message)
        {
            return AnsiConsole.Ask<string>($"[yellow]{message}[/]");
        }

        public int PromptForItemId(string promptMessage)
        {
            AnsiConsole.Markup($"[bold yellow]{promptMessage}[/] ");
            int itemId;
            while (!int.TryParse(Console.ReadLine(), out itemId))
            {
                AnsiConsole.MarkupLine("[red]Invalid ID entered. Please enter a valid number.[/]");
            }
            return itemId;
        }

        public float PromptForAmount()
        {
            string amountInput = AnsiConsole.Ask<string>("[bold yellow]Enter amount (must be a number):[/] ");
            if (string.IsNullOrWhiteSpace(amountInput) || !float.TryParse(amountInput, out float amount))
            {
                AnsiConsole.MarkupLine("[red]Invalid input. Please enter a valid number for the amount.[/]");
                return PromptForAmount();
            }
            return amount;
        }

        public string PromptForEditOrDelete(string itemTitle)
        {
            SelectionPrompt<string> editOrDeletePrompt = new SelectionPrompt<string>()
                .Title($"[bold yellow]\nWould you like to edit or delete the item \"[blue italic bold]{itemTitle}[/]\"?[/]")
                .AddChoices(new[] { "Edit", "Delete" });

            return AnsiConsole.Prompt(editOrDeletePrompt);
        }

        public string PromptForSortOption(List<string> sortOptions)
        {
            SelectionPrompt<string> sortPrompt = new SelectionPrompt<string>()
                .PageSize(sortOptions.Count > 3 ? sortOptions.Count : 3)
                .AddChoices(sortOptions)
                .Title("[bold yellow]\nSelect a sorting option:[/]");

            return AnsiConsole.Prompt(sortPrompt);
        }
        public string PromptForSortDirection()
        {
            List<string> directionOptions = new List<string> { "Ascending", "Descending" };
            SelectionPrompt<string> directionPrompt = new SelectionPrompt<string>()
                .PageSize(directionOptions.Count > 3 ? directionOptions.Count : 3)
                .AddChoices(directionOptions)
                .Title("[bold yellow]\nSelect sorting direction:[/]");

            return AnsiConsole.Prompt(directionPrompt);
        }
    }
}
