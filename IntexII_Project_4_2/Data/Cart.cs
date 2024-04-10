using Microsoft.CodeAnalysis;

namespace IntexII_Project_4_2.Data
{
    public class Cart
    {
        public List<CartLine> Lines { get; set; } = new List<CartLine>();

        public void AddItem(Product prod, int quantity)
        {
            CartLine? line = Lines
                .Where(x => x.Product.ProductId == prod.ProductId)
                .FirstOrDefault();

            if (line == null)
            {
                Lines.Add(new CartLine
                {
                    Product = prod,
                    Quantity = quantity
                });
            }
            else
            {
                line.Quantity += quantity;
            }
        }

        public void RemoveLine(Product prod) => Lines.RemoveAll(x => x.Product.ProductId == prod.ProductId);
        public void Clear() => Lines.Clear();
        public decimal CalculateTotal()
        {
            return Lines.Sum(line => line.Quantity * (line.Product.Price ?? 0));
        }
        public class CartLine
        {
            public int CartLineId { get; set; }
            public Product Product { get; set; }
            public int Quantity { get; set; }

        }
    }
}
