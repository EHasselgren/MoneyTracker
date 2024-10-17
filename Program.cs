using MoneyTracker.Services;

namespace MoneyTracker
{
    public static class Program
    {
        static readonly ItemService _itemService = new();
        static readonly InputService _inputService = new();
        static readonly DisplayService _displayService = new(_itemService);
        static readonly MenuService _menuService = new(_itemService, _displayService, _inputService);

        public static void Main()
        {
            _itemService.LoadItems();
            _menuService.Start();
        }
    }
}