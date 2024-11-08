using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KitapyurduData;

public class MenuManager
{
    private string[] menuItems;
    private int selectedIndex;

    public MenuManager(string[] items)
    {
        menuItems = items;
        selectedIndex = 0;
    }

    public int DisplayMenu()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("Lütfen bir seçenek seçin:");

            // Menü öğelerini listele
            for (int i = 0; i < menuItems.Length; i++)
            {
                if (i == selectedIndex)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"> {menuItems[i]}");
                    Console.ResetColor();
                }
                else
                {
                    Console.WriteLine($"  {menuItems[i]}");
                }
            }

            // Kullanıcıdan giriş al
            ConsoleKey key = Console.ReadKey(true).Key;

            // Ok tuşları ile gezinme
            if (key == ConsoleKey.UpArrow)
            {
                selectedIndex = (selectedIndex > 0) ? selectedIndex - 1 : menuItems.Length - 1;
            }
            else if (key == ConsoleKey.DownArrow)
            {
                selectedIndex = (selectedIndex < menuItems.Length - 1) ? selectedIndex + 1 : 0;
            }
            else if (key == ConsoleKey.Enter)
            {
                return selectedIndex;
            }
        }
    }
}
