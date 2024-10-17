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
        static readonly ItemService _itemService = new();
        static readonly DisplayService _displayService = new(_itemService);
        static readonly InputService _inputService = new();
        static readonly MenuService _menuService = new(_itemService, _displayService, _inputService);

        public static void Main(string[] args)
        {
            _itemService.LoadItems();
            _menuService.Start();
        }
    }
}