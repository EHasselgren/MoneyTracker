using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

public class MoneyTracker
{
    public float Balance { get; set; }
    public List<Item> Items { get; set; } = new List<Item>();

    private void CalculateInitialBalance()
    {
        Balance = Items.Sum(item => item.ItemType == ItemType.Income ? item.Amount : -item.Amount);
    }

    public void LoadItems()
    {
        try
        {
            string jsonString = File.ReadAllText("items.json");
            Items = JsonSerializer.Deserialize<List<Item>>(jsonString);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading items: {ex.Message}");
        }
    }

    public MoneyTracker()
    {
        LoadItems();
        CalculateInitialBalance();
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
            Console.WriteLine($"Error saving item: {ex.Message}");
        }
    }

    public void AddItem(Item item)
    {
        item.ItemId = Items.Count + 1;
        Items.Add(item);

        Balance += item.ItemType == ItemType.Income ? item.Amount : -Math.Abs(item.Amount);

        SaveItems();
    }

    public void EditItem(int itemId, Item newItem)
    {
        var existingItem = Items.FirstOrDefault(i => i.ItemId == itemId);
        if (existingItem != null)
        {
            Balance -= existingItem.Amount;
            existingItem.Title = newItem.Title;
            existingItem.Amount = newItem.Amount;
            existingItem.Date = newItem.Date;
            existingItem.ItemType = newItem.ItemType;

            Balance += newItem.ItemType == ItemType.Income ? newItem.Amount : -newItem.Amount;

            SaveItems();
        }
        else
        {
            Console.WriteLine("Item not found or invalid new item.");
        }
    }
}
