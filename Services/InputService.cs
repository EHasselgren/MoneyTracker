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
            AnsiConsole.MarkupLine($"[yellow]\n{message}[/]");
            return Console.ReadLine();
        }

        public float PromptForAmount()
        {
            AnsiConsole.MarkupLine($"[bold yellow]\nEnter amount:[/] ");
            string? amountInput = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(amountInput) || !float.TryParse(amountInput, out float amount))
            {
                AnsiConsole.WriteLine("Invalid input. Please enter a valid number for the amount.");
                return 0;
            }
            return amount;
        }

        public string PromptForEditOrDelete()
        {
            var editOrDeletePrompt = new SelectionPrompt<string>()
                .Title($"[bold yellow]\nWould you like to edit or delete this item?[/]")
                .AddChoices(new[] { "Edit", "Delete" });

            return AnsiConsole.Prompt(editOrDeletePrompt);
        }

        public string PromptForSortOption(List<string> sortOptions)
        {
            var sortPrompt = new SelectionPrompt<string>()
                .PageSize(sortOptions.Count > 3 ? sortOptions.Count : 3)
                .AddChoices(sortOptions)
                .Title("[bold yellow]\nSelect a sorting option:[/]");

            return AnsiConsole.Prompt(sortPrompt);
        }

        public string PromptForSortDirection()
        {
            var directionOptions = new List<string> { "Ascending", "Descending" };
            var directionPrompt = new SelectionPrompt<string>()
                .PageSize(directionOptions.Count > 3 ? directionOptions.Count : 3)
                .AddChoices(directionOptions)
                .Title("[bold yellow]\nSelect sorting direction:[/]");

            return AnsiConsole.Prompt(directionPrompt);
        }
    }
}

