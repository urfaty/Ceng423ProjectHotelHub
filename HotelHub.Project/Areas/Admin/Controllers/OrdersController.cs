using HotelHub.Models;
using HotelHub.Repository;
using HotelHub.Project.Utility;
using HotelHub.Project.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Razorpay.Api;
using Stripe.Checkout;
using Stripe;
using System.Security.Claims;

namespace HotelHub.Project.Areas.Admin.Controllers
{
    [Authorize]
    [Area("Admin")]
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;
        public OrdersController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(string status)
        {
            IEnumerable<OrderHeader> order;
            if (User.IsInRole("Admin"))
            {
                order = _context.OrderHeaders.
                    Include(x => x.ApplicationUser).ToList();

            }
            else
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var claims =  claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                order = _context.OrderHeaders.Where(x => x.ApplicationUserId==claims.Value); 
            }
            switch (status)
            {
                case "pending":
                    order = order.Where(x => x.PaymentStatus == PaymentStatus.StatusPending);
                    break;
                case "approved":
                    order = order.Where(x => x.PaymentStatus == PaymentStatus.StatusApproved);
                    break;
                case "underprocess":
                    order = order.Where(x => x.OrderStatus == OrderStatus.StatusInProcess);
                    break;
                case "shipped":
                    order = order.Where(x => x.OrderStatus == OrderStatus.StatusShpped);
                    break;

                default:
                    break;
            }
            return View(order);
        }
        public IActionResult OrderDetails(int id)
        {
            var OrderDetail = new OrderDetailsViewModel()
            {
                OrderHeader = _context.OrderHeaders.Include(x => x.ApplicationUser)
                .FirstOrDefault(x => x.Id == id),
                OrderDetails = _context.OrderDetails.Include(x => x.Item)
                .Where(Item => Item.Id == id).ToList()

            };
            return View(OrderDetail);
        }
        public IActionResult InProcess(OrderDetailsViewModel vm)
        {
            var orderHeader = _context.OrderHeaders.FirstOrDefault(x => x.Id == vm.OrderHeader.Id);
            orderHeader.OrderStatus = OrderStatus.StatusInProcess;
            _context.SaveChanges();

            TempData["success"] = "Order Status Updated-Inprocess";
            return RedirectToAction("OrderDetails", "Orders", new { id = vm.OrderHeader.Id });
        }
        public IActionResult Shipped(OrderDetailsViewModel vm)
        {
            var orderHeader = _context.OrderHeaders.FirstOrDefault(x => x.Id == vm.OrderHeader.Id);
            orderHeader.OrderStatus = OrderStatus.StatusShpped;
            _context.OrderHeaders.Update(orderHeader);
            _context.SaveChanges();
            TempData["success"] = "Order Status Updated-Shipped";
            return RedirectToAction("OrderDetails", "Orders", new { id = vm.OrderHeader.Id });
        }
        public IActionResult CancelOrder(OrderDetailsViewModel vm)
        {
            var orderHeader = _context.OrderHeaders.
                FirstOrDefault(x => x.Id == vm.OrderHeader.Id);
            if (orderHeader.PaymentStatus == PaymentStatus.StatusApproved)
            {
                var refund = new RefundCreateOptions
                {
                    Reason = RefundReasons.RequestedByCustomer,
                    PaymentIntent = orderHeader.PaymentIntentId
                };
                var service = new RefundService();
                Stripe.Refund Refund = service.Create(refund);
                orderHeader.OrderStatus = OrderStatus.StatusCancelled;
                orderHeader.PaymentStatus = OrderStatus.StatusRefund;


            }
            else
            {

                orderHeader.OrderStatus = OrderStatus.StatusCancelled;
                orderHeader.PaymentStatus = OrderStatus.StatusCancelled;
            }

            _context.SaveChanges();
            TempData["success"] = "Order Cancelled";
            return RedirectToAction("OrderDetails", "Orders", new { id = vm.OrderHeader.Id });
        }














        //public IActionResult PayNow(OrderVM vm)
        //{
        //    var OrderHeader = _unitOfWork.OrderHeader.GetT(x => x.Id == vm.OrderHeader.Id,
        //         includeProperties: "ApplicationUser");
        //    var OrderDetail = _unitOfWork.OrderDetail.GetAll(x => x.OrderHeaderId == vm.OrderHeader.Id, includeProperties: "Product");
        //    var domain = "https://localhost:7129/";
        //    var options = new SessionCreateOptions
        //    {
        //        LineItems = new List<SessionLineItemOptions>(),
        //        Mode = "payment",
        //        SuccessUrl = domain + $"customer/cart/ordersuccess?id={vm.OrderHeader.Id}",
        //        CancelUrl = domain + $"customer/cart/Index",
        //    };

        //    foreach (var item in OrderDetail)
        //    {

        //        var lineItemsOptions = new SessionLineItemOptions
        //        {
        //            PriceData = new SessionLineItemPriceDataOptions
        //            {
        //                UnitAmount = (long)(item.Product.Price * 100),
        //                Currency = "INR",
        //                ProductData = new SessionLineItemPriceDataProductDataOptions
        //                {
        //                    Name = item.Product.Name,
        //                },

        //            },
        //            Quantity = item.Count,
        //        };
        //        options.LineItems.Add(lineItemsOptions);
        //    }
        //    var service = new SessionService();
        //    Session session = service.Create(options);
        //    _unitOfWork.OrderHeader.PaymentStatus(vm.OrderHeader.Id, session.Id, session.PaymentIntentId);
        //    _unitOfWork.Save();
        //    Response.Headers.Add("Location", session.Url);
        //    return new StatusCodeResult(303);
        //    return RedirectToAction("Index", "Home");
        //}

    }
}
