using HtmlAgilityPack;
using KitapYurduData.Data;
using Microsoft.EntityFrameworkCore;

namespace KitapyurduData;

public class GetNewlyAddedBooks
{
    public static async Task Execute(BookLibraryContext context)
    {

        if (!await context.Books.AnyAsync())
        {
            Console.WriteLine("Daha önce hiç aktarım yapılmamış. Lütfen önce 1. adımdan başlayınız!");
            return;
        }

        HttpClient client = new HttpClient();
        client.
            DefaultRequestHeaders.
            UserAgent.
            ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/86.0.4240.183 Safari/537.36");

        string url = "https://www.kitapyurdu.com/index.php?route=product/category";
        var response = await client.GetStringAsync(url);
        HtmlDocument htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(response);

        Console.WriteLine("Kontrol ediliyor lütfen bekleyiniz..");

        //Tüm kategori düğümlerini bul
        var categoryNodes = htmlDoc.DocumentNode.SelectNodes("//div[@class='category-container']//div[@class='category']");
        if (categoryNodes != null)
        {
            var yeniEklnenBilgi = 0;

            foreach (var categoryNode in categoryNodes)
            {
                var titleNode = categoryNode.SelectSingleNode(".//h2");
                string categoryName = titleNode?.InnerText.Trim();

                var linkNode = categoryNode.SelectSingleNode(".//a[@class='category-item']");
                string categoryUrl = linkNode?.GetAttributeValue("href", "");
                int startIndex = categoryUrl.LastIndexOf('/') + 1;
                int endIndex = categoryUrl.LastIndexOf('.');
                string kitapYurduCategoryId = categoryUrl.Substring(startIndex, endIndex - startIndex);

                var bookDetailsUrl = $"https://www.kitapyurdu.com/index.php?route=product/category&filter_category_all=true&filter_in_stock=1&sort=publish_date&order=DESC&limit=100&path={kitapYurduCategoryId}";

                // Load book details page and get total books/pages information
                var bookDetailsResponse = await client.GetStringAsync(bookDetailsUrl);
                HtmlDocument bookDetailsDoc = new HtmlDocument();
                bookDetailsDoc.LoadHtml(bookDetailsResponse);

                HtmlNode resultsNode = bookDetailsDoc.DocumentNode.SelectSingleNode("//div[@class='results']");
                string resultsText = resultsNode?.InnerText.Trim();

                int startTotalBooks = resultsText.IndexOf("toplam:") + "toplam:".Length;
                int endTotalBooks = resultsText.IndexOf("(", startTotalBooks);
                string totalBooks = resultsText.Substring(startTotalBooks, endTotalBooks - startTotalBooks).Trim();

                int startTotalPages = resultsText.IndexOf("(") + 1;
                int endTotalPages = resultsText.IndexOf("Sayfa", startTotalPages);
                string totalPages = resultsText.Substring(startTotalPages, endTotalPages - startTotalPages).Trim();

                var category = await context.Categories.Where(x => x.KitapyurduCategoryId == kitapYurduCategoryId).FirstOrDefaultAsync();

                if ((categoryName != "Başvuru Kitapları" &&
                        categoryName != "Çocuk Kitapları" &&
                        categoryName != "Ders Kitapları" &&
                        categoryName != "Diğer" &&
                        categoryName != "Diğer Dildeki Yayınlar" &&
                        categoryName != "Din" &&
                        categoryName != "İslam" &&
                        categoryName != "Sınav" &&
                        categoryName != "Puzzle - Yapboz" &&
                        categoryName != "Kırtasiye" &&
                        categoryName != "Dekorasyon" &&
                        categoryName != "Aksesuar" &&
                        categoryName != "Hobi ve Oyuncak" &&
                        categoryName != "Çeşitli" &&
                        categoryName != "Ahşap Ürünler" &&
                        category.ToplamKitapSayisi < int.Parse(totalBooks)))
                {
                    // Sayfa sayısı ve toplam kitap sayısını güncelle
                    yeniEklnenBilgi += 1;

                    Console.WriteLine($"{category.Name} kategorisinde yeni eklenen {int.Parse(totalBooks) - category.ToplamKitapSayisi} adet kitap var. (İşlemler tamamlandıktan sonra 1. adımdan devam ediniz..)");

                    #region Kategoriyi Güncelle
                    context.Attach(category);
                    category.PageCount = int.Parse(totalPages);
                    category.ToplamKitapSayisi = int.Parse(totalBooks);
                    #endregion

                    #region En Son Hareket Güncelle
                    // Enson hareket nesnesini bağlayarak güncelleme
                    var ensonHareket = await context.TransferInfos.Where(x => x.EnSonKaldigiKategoriId == category.Id).FirstOrDefaultAsync();

                    context.Attach(ensonHareket);
                    ensonHareket.TransferDurum = Enums.TransferDurum.YarimKalmis;
                    #endregion

                    await context.SaveChangesAsync();

                }
            }

            if (yeniEklnenBilgi == 0)
                Console.WriteLine("Yeni eklenen kitap bulunamadı.");
        }
    }
}
