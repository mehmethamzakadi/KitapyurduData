using KitapyurduData;
using KitapYurduData.Data;

using var context = new BookLibraryContext();

// Menü öğelerini tanımla
string[] menuItems = { "Aktarmaya Başla veya Kaldığı Yerden Devam Et..", "Yeni Eklenen Kitap Var Mı Kontrol Et!", "Çıkış" };

MenuManager menu = new(menuItems);

while (true)
{
    int selectedIndex = menu.DisplayMenu();

    Console.Clear();
    switch (selectedIndex)
    {
        case 0:
            await GetAllCategoriesAndBooks.Execute(context);
            break;
        case 1:
            await GetNewlyAddedBooks.Execute(context);
            break;
        case 2:
            Console.WriteLine("Çıkış yapılıyor...");
            return;
    }

    Console.WriteLine("\nDevam etmek için bir tuşa basın...");
    Console.ReadKey();
}