namespace Backend.Infrastructure.Persistence.Seeding;

public sealed class OlistSeedOptions
{
    public const string SectionName = "OlistImport";

    public bool Enabled { get; set; }
    public string? DatasetRootPath { get; set; }
    public string ProductCategoryTranslationFileName { get; set; } = "product_category_name_translation.csv";
    public string CustomersFileName { get; set; } = "olist_customers_dataset.csv";
    public string SellersFileName { get; set; } = "olist_sellers_dataset.csv";
    public string ProductsFileName { get; set; } = "olist_products_dataset.csv";
    public string OrdersFileName { get; set; } = "olist_orders_dataset.csv";
    public string OrderItemsFileName { get; set; } = "olist_order_items_dataset.csv";
    public string OrderPaymentsFileName { get; set; } = "olist_order_payments_dataset.csv";
    public string OrderReviewsFileName { get; set; } = "olist_order_reviews_dataset.csv";
}
