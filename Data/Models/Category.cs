namespace KitapyurduData.Data.Models;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string KitapyurduCategoryId { get; set; }
    public string KitapyurduCategoryUrl { get; set; }
    public int? PageCount { get; set; }
    public int? ToplamKitapSayisi { get; set; }
}
