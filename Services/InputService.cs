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

        public string PromptForNewTitle(string currentTitle)
        {
            AnsiConsole.MarkupLine($"[yellow]Current title:[/] [blue]{currentTitle}[/]");
            string newTitle = AnsiConsole.Ask<string>($"[bold yellow]Enter new title (leave blank to keep current):[/]", currentTitle);
            return string.IsNullOrWhiteSpace(newTitle) ? currentTitle : newTitle.Trim();
        }

        public float PromptForAmount()
        {
            float amount;
            string amountInput;

            do
            {
                amountInput = AnsiConsole.Ask<string>("[bold yellow]Enter amount (must be a number):[/] ");

                if (string.IsNullOrWhiteSpace(amountInput) || !float.TryParse(amountInput, out amount))
                {
                    AnsiConsole.MarkupLine("[red]Invalid input. Please enter a valid number for the amount.[/]");
                }
            } while (string.IsNullOrWhiteSpace(amountInput) || !float.TryParse(amountInput, out amount));

            return amount;
        }

        string PromptForSelection(string title, IEnumerable<string> choices)
        {
            SelectionPrompt<string> selectionPrompt = new SelectionPrompt<string>()
                .PageSize(choices.Count() > 3 ? choices.Count() : 3)
                .AddChoices(choices)
                .Title(title);

            return AnsiConsole.Prompt(selectionPrompt);
        }

        public string PromptForEditOrDelete(string itemTitle)
        {
            return PromptForSelection($"[bold yellow]\nWould you like to edit or delete the item \"[blue italic bold]{itemTitle}[/]\"?[/]",
                                      new[] { "Edit", "Delete" });
        }

        public string PromptForSortOption(List<string> sortOptions)
        {
            return PromptForSelection("[bold yellow]\nSelect a sorting option:[/]", sortOptions);
        }

        public string PromptForSortDirection()
        {
            return PromptForSelection("[bold yellow]\nSelect sorting direction:[/]", new List<string> { "Ascending", "Descending" });
        }
    }
}