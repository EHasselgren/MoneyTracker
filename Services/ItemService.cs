using Spectre.Console;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Linq;
using MoneyTracker.Models;
using MoneyTracker.Enums;

namespace MoneyTracker.Services
{
    public class ItemService
    {
        public decimal Balance { get; set; }
        public List<Item> Items { get; set; } = new List<Item>();
        public void AddItem(Item item)
        {
            item.ItemId = Items.Count + 1;
            Items.Add(item);
            Balance += item.ItemType == ItemType.Income ? item.Amount : -Math.Abs(item.Amount);
            SaveItems();
        }
        void CalculateInitialBalance()
        {
            Balance = Items.Sum(item => item.ItemType == ItemType.Income ? item.Amount : -item.Amount);
        }
        public void LoadItems()
        {
            try
            {
                string jsonString = File.ReadAllText("items.json");
                Items = JsonSerializer.Deserialize<List<Item>>(jsonString) ?? new List<Item>();
                CalculateInitialBalance();
            }
            catch (Exception ex)
            {
                AnsiConsole.WriteLine($"Error loading items: {ex.Message}");
                Items = new List<Item>();
            }
        }
        public void SaveItems()
        {
            try
            {
                string jsonString = JsonSerializer.Serialize(Items);
                File.WriteAllText("items.json", jsonString);
            }
            catch (Exception ex)
            {
                AnsiConsole.WriteLine($"Error saving item: {ex.Message}");
            }
        }

        public List<Item> GetFilteredItems(ItemType itemType)
        {
            return Items.Where(i => i.ItemType == itemType).ToList();
        }

        public void EditItem(int itemId, Item newItem)
        {
            Item? existingItem = Items.FirstOrDefault(i => i.ItemId == itemId);

            if (existingItem != null)
            {
                // remove *old* amount from balance
                Balance -= existingItem.Amount;
                existingItem.Title = newItem.Title;
                existingItem.Amount = newItem.Amount;
                existingItem.Date = newItem.Date;
                existingItem.ItemType = newItem.ItemType;
                // add *new* amount to balance
                Balance += newItem.ItemType == ItemType.Income ? newItem.Amount : -newItem.Amount;

                SaveItems();
            }
            else
            {
                AnsiConsole.WriteLine("Item not found or invalid new item.");
            }
        }

        public void PrintItemsToFile()
        {
            string fileName = AnsiConsole.Ask<string>("[bold yellow]Enter a file name to save the items list (without extension): [/]").Trim();

            if (string.IsNullOrWhiteSpace(fileName))
            {
                fileName = "saved_items.txt";
            }

            string filePath = $"{fileName}.txt";

            try
            {
                // write itemList to file (needs alot of work, formating is awful):
                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    writer.WriteLine("ID\tTitle\tAmount\tDate\tType");
                    writer.WriteLine("------------------------------------------------");

                    foreach (Item item in Items)
                    {
                        writer.WriteLine($"{item.ItemId}\t{item.Title}\t{item.Amount:C2}\t{item.Date.ToShortDateString()}\t{item.ItemType}");
                    }
                }
                AnsiConsole.MarkupLine($"[green]Items list has been saved to {filePath}[/]");
            }
            catch (Exception ex)
            {
                AnsiConsole.WriteLine($"Error writing to file: {ex.Message}");
            }
        }
    }
}