
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
        private static ItemService _itemService = new ItemService();
        private static DisplayService _displayService = new DisplayService(_itemService);
        private static InputService _inputService = new InputService();
        private static MenuService _menuService = new MenuService(_itemService, _displayService, _inputService);

        public static void Main(string[] args)
        {
            _itemService.LoadItems();
            _menuService = new MenuService(_itemService, _displayService, _inputService);
            _menuService.Start();
        }
    }
}