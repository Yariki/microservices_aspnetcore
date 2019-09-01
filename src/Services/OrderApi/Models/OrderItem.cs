using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OrderApi.Infrastructure.Exceptions;

namespace OrderApi.Models
{
    public class OrderItem
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string ProductName { get; set; }

        public string PictureUrl { get; set; }

        public decimal UnitPrice { get; set; }

        public int Units { get; set; }

        public int ProductId { get; set; }

        public int OrderId { get; set; }

        public OrderItem()
        {
            
        }

        public OrderItem(string productName, string pictureUrl, decimal unitPrice, int units, int productId)
        {
            if (units < 0)
            {
                throw new OrderingDomainException($"Invalid number of units");
            }

            ProductName = productName;
            PictureUrl = pictureUrl;
            UnitPrice = unitPrice;
            Units = units;
            ProductId = productId;
        }

        public void SeyPictureUrl(string pictureUrl)
        {
            if (!string.IsNullOrEmpty(pictureUrl))
            {
                PictureUrl = pictureUrl;
            }
        }

        public void AddUnits(int units)
        {
            if (units < 0)
            {
                throw new OrderingDomainException("Invalaid units");
            }

            Units += units;
        }
    }
}