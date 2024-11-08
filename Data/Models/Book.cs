namespace KitapyurduData.Data.Models;

public class Book
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int CategoryId { get; set; }
    public Category Category { get; set; }

    public int? PublisherId { get; set; }
    public string PublisherName { get; set; }
    //public Publisher Publisher { get; set; }

    //public int? AuthorId { get; set; }
    public string AuthorName { get; set; }
    //public Author Author { get; set; }

    public string BookName { get; set; }
    public int? PageCount { get; set; }
    public string Language { get; set; }
    public DateTime? PublishDate { get; set; }
    public string Translator { get; set; }
    public decimal? Price { get; set; }
    public string ProductInfo { get; set; }
    public string CoverText { get; set; }
    public int? Rating { get; set; }
    public string ImageUrl { get; set; }

}
