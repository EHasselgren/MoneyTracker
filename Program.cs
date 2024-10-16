
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
        private static MoneyTrackerService _moneyTracker = new MoneyTrackerService();
        private static DisplayService _displayService = new DisplayService(_moneyTracker);
        private static InputService _inputService = new InputService();
        private static MenuService _menuService;

        public static void Main(string[] args)
        {
            _moneyTracker.LoadItems();
            _menuService = new MenuService(_moneyTracker, _displayService, _inputService);
            _menuService.Start();
        }
    }
}