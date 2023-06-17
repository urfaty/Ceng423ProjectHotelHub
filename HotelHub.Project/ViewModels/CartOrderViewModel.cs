using HotelHub.Models;

namespace HotelHub.Project.ViewModels
{
    public class CartOrderViewModel
    {
        public List<Cart> ListofCart { get; set; }
        public OrderHeader OrderHeader { get; set; }
    }
}
