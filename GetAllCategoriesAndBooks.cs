using HtmlAgilityPack;
using KitapyurduData.Data.Models;
using KitapYurduData.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace KitapyurduData;

public class GetAllCategoriesAndBooks
{
    public static async Task Execute(BookLibraryContext context)
    {
        HttpClient client = new HttpClient();
        client.
            DefaultRequestHeaders.
            UserAgent.
            ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/86.0.4240.183 Safari/537.36");

        #region Kategorileri Getir ve Ekle
        // Kategorilerin veritabanında zaten mevcut olup olmadığını kontrol et.
        if (!await context.Categories.AnyAsync())
        {
            string url = "https://www.kitapyurdu.com/index.php?route=product/category";

            var response = await client.GetStringAsync(url);
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(response);

            // Tüm kategori düğümlerini bul
            var categoryNodes = htmlDoc.DocumentNode.SelectNodes("//div[@class='category-container']//div[@class='category']");
            if (categoryNodes != null)
            {
                var categoriesToAdd = new List<Category>();

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

                    if (categoryName != "Başvuru Kitapları" &&
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
                        categoryName != "Ahşap Ürünler")
                    {
                        var category = await context.AddAsync(new Category
                        {
                            Name = categoryName,
                            KitapyurduCategoryId = kitapYurduCategoryId,
                            KitapyurduCategoryUrl = categoryUrl,
                            PageCount = int.Parse(totalPages),
                            ToplamKitapSayisi = int.Parse(totalBooks),
                        });
                        await context.SaveChangesAsync();

                        //Kategori hiç aktarılmamışsa
                        await context.TransferInfos.AddAsync(new TransferInfo
                        {
                            EnSonKaldigiSayfa = int.Parse(totalPages),
                            EnSonKaldigiKategoriId = category.Entity.Id,
                            TransferDurum = Enums.TransferDurum.Aktarilmamis
                        });

                        await context.SaveChangesAsync();

                        Console.WriteLine($"Eklenen Kategori: {categoryName}");

                    }

                }
                Console.WriteLine("Kategoriler eklendi.. Lütfen bekleyiniz..");
            }
            else
            {
                Console.WriteLine("Kategori bilgisi bulunamadı.");
            }
        }
        #endregion

        #region Kitapları Getir ve Ekle

        var kacinciSayfada = 0;
        var ensonHareketId = 0;
        var categories = await context.Categories.ToListAsync();

        foreach (var category in categories)
        {
            try
            {
                #region En Son Hareketi Bul
                var ensonHareket = await context.TransferInfos
                .Where(x => x.EnSonKaldigiKategoriId == category.Id && x.TransferDurum != KitapyurduData.Enums.TransferDurum.Tamamlanmis)
                .FirstOrDefaultAsync();

                if (ensonHareket == null)
                    continue;

                ensonHareketId = ensonHareket.Id;
                #endregion

                for (int i = 1; i <= category.PageCount; i++)
                {
                    var bookList = new List<Book>();
                    kacinciSayfada = i;

                    #region Kaldığı Yere Gelene Kadar İlerle

                    if (ensonHareket.EnSonKaldigiSayfa != category.PageCount &&
                        ensonHareket.EnSonKaldigiSayfa > kacinciSayfada)
                        continue;
                    #endregion

                    #region Sayfayı Yükle
                    var belgeUrl = $"https://www.kitapyurdu.com/index.php?route=product/category&filter_category_all=true&filter_in_stock=1&sort=publish_date&order=DESC&limit=100&path={category.KitapyurduCategoryId}&page={i}";
                    var belgeResponse = await client.GetStringAsync(belgeUrl);

                    HtmlDocument belge = new HtmlDocument();
                    belge.LoadHtml(belgeResponse);
                    #endregion

                    var productNodes = belge.DocumentNode.SelectNodes("//div[@class='product-grid']/div[@class='product-cr']");
                    if (productNodes != null)
                    {
                        foreach (var productNode in productNodes)
                        {
                            #region ProductId Al ve Eklenmiş Mi Kontrol Et
                            //Daha önceden eklemişse ekleme, bir sonraki kayıttan devam et.
                            int productId = 0;
                            Match matchProductId = Regex.Match(productNode.Id, @"\d+");
                            if (matchProductId.Success)
                                productId = int.Parse(matchProductId.Value);

                            if (await context.Books.AnyAsync(x => x.ProductId == productId))
                                continue;
                            #endregion

                            #region Kitap Bilgilerini Al
                            int pageCount = 0;
                            DateTime publishDate = DateTime.Today;
                            string language = string.Empty;
                            string translator = string.Empty;
                            string infoText = string.Empty;

                            string imageUrl = productNode.SelectSingleNode(".//div[@class='image']/div[@class='cover']/a/img")?.GetAttributeValue("src", "");
                            string bookName = productNode.SelectSingleNode(".//div[@class='name ellipsis']/a/span")?.InnerText.Trim();
                            string publisher = productNode.SelectSingleNode(".//div[@class='publisher']/span/a/span")?.InnerText.Trim();
                            string author = productNode.SelectSingleNode(".//div[@class='author']")?.InnerText.Trim().Replace("Yazar:", "").Trim();
                            string productInfo = productNode.SelectSingleNode(".//div[@class='product-info']")?.InnerText.Trim();
                            string price = productNode.SelectSingleNode(".//div[@class='price-new ']/span[@class='value']")?.InnerText.Trim() ?? "0";
                            var raiting = int.Parse(Regex.Match(productNode.SelectSingleNode(".//div[@class='rating']/div")?.GetAttributeValue("title", ""), @"\d+").Value);

                            var detailLink = productNode.SelectSingleNode(".//div[@class='name ellipsis']/a")?.Attributes[0].Value;
                            if (!string.IsNullOrEmpty(detailLink))
                            {
                                var detailResponse = await client.GetStringAsync(detailLink);
                                HtmlDocument detailDocument = new HtmlDocument();
                                detailDocument.LoadHtml(detailResponse);

                                var infoTextNodes = detailDocument.DocumentNode.SelectNodes("//span[@class='info__text']");
                                infoText = infoTextNodes?.FirstOrDefault()?.InnerText.Trim();

                                var attributesNode = detailDocument.DocumentNode.SelectSingleNode("//div[@class='attributes']");
                                if (attributesNode != null)
                                {
                                    foreach (var row in attributesNode.SelectNodes(".//tr"))
                                    {
                                        var cells = row.SelectNodes("td");
                                        if (cells != null && cells.Count == 2)
                                        {
                                            string attributeName = cells[0].InnerText.Trim();
                                            string attributeValue = cells[1].InnerText.Trim();

                                            if (attributeName.StartsWith("Dil"))
                                                language = attributeValue;
                                            else if (attributeName.StartsWith("Sayfa"))
                                                pageCount = int.TryParse(attributeValue, out var pages) ? pages : 0;
                                            else if (attributeName.StartsWith("Çevirmen"))
                                                translator = attributeValue;
                                            else if (attributeName.StartsWith("Yayın"))
                                            {
                                                var dateParts = attributeValue.Split(".");
                                                publishDate = new DateTime(
                                                    int.Parse(dateParts[2]),
                                                    int.Parse(dateParts[1]),
                                                    int.Parse(dateParts[0]));
                                            }
                                        }
                                    }
                                }
                            }
                            #endregion

                            bookList.Add(new Book
                            {
                                AuthorName = author,
                                PublisherName = publisher,
                                BookName = bookName,
                                ImageUrl = imageUrl,
                                CoverText = infoText,
                                Language = language,
                                PageCount = pageCount,
                                Price = decimal.Parse(price),
                                ProductId = productId,
                                ProductInfo = productInfo,
                                PublishDate = publishDate,
                                Rating = raiting,
                                Translator = translator,
                                CategoryId = category.Id,
                            });
                        }

                        await context.Books.AddRangeAsync(bookList);

                        #region En Son Hareket Güncelle
                        // Enson hareket nesnesini bağlayarak güncelleme
                        context.Attach(ensonHareket);
                        ensonHareket.EnSonKaldigiSayfa = kacinciSayfada;
                        ensonHareket.TransferDurum = KitapyurduData.Enums.TransferDurum.YarimKalmis;
                        #endregion

                        await context.SaveChangesAsync();

                        Console.WriteLine($"Eklenen Kategori: {category.Name} - Eklenen Sayfa: {kacinciSayfada} - Kalan Sayfa: {category.PageCount - kacinciSayfada}");

                    }
                }

                #region En Son Hareket Güncelle
                //Kategori hatasız şekilde aktarılmışsa
                context.Attach(ensonHareket);
                ensonHareket.TransferDurum = KitapyurduData.Enums.TransferDurum.Tamamlanmis;
                await context.SaveChangesAsync();
                #endregion

            }
            catch (Exception)
            {
                #region En Son Hareket Güncelle
                //Kategori aktarım sırasında hata oluşmuşsa
                if (ensonHareketId > 0)
                {
                    var transferInfo = await context.TransferInfos.FirstOrDefaultAsync(x => x.Id == ensonHareketId);
                    if (transferInfo != null)
                    {
                        context.Attach(transferInfo);
                        transferInfo.EnSonKaldigiSayfa = kacinciSayfada;
                        transferInfo.TransferDurum = Enums.TransferDurum.YarimKalmis;
                        await context.SaveChangesAsync();
                    }
                }
                #endregion

                Console.WriteLine("İşlem sırasında hata oluştu");
                return;
            }
        }

        #endregion

        Console.WriteLine("Tüm veriler başarıyla eklendi.");
        return;
    }
}
