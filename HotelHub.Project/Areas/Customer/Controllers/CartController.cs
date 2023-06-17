using HotelHub.Models;
using HotelHub.Repository;
using HotelHub.Project.Utility;
using HotelHub.Project.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe;
using Stripe.Checkout;
using System.Security.Claims;

namespace HotelHub.Project.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;
        [BindProperty]
        public CartOrderViewModel details { get; set; }

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            details = new CartOrderViewModel()
            {
                OrderHeader = new HotelHub.Models.OrderHeader()
            };

            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            details.ListofCart = _context.Carts.Include(z => z.Item).Where(x => x.ApplicationUserId == claims.Value)
                .ToList();
            if (details.ListofCart != null)
            {
                foreach (var cart in details.ListofCart)
                {
                    details.OrderHeader.OrderTotal += (cart.Item.Price * cart.Count);
                }
            }
            return View(details);
        }

        [HttpGet]
        public IActionResult Summary()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            details = new CartOrderViewModel()
            {
                ListofCart = _context.Carts.Include(x => x.Item).
                Where(x => x.ApplicationUserId == claims.Value).ToList(),
                OrderHeader = new OrderHeader()
            };

            details.OrderHeader.ApplicationUser = _context.ApplicationUsers
                .Where(x => x.Id == claims.Value).FirstOrDefault();

            details.OrderHeader.Name = details.OrderHeader.ApplicationUser.Name;
            details.OrderHeader.Phone = details.OrderHeader.ApplicationUser.PhoneNumber;
            details.OrderHeader.TimeofPick = DateTime.Now;
            foreach (var cart in details.ListofCart)
            {
                details.OrderHeader.OrderTotal += (cart.Item.Price * cart.Count);
            }
            return View(details);

        }
        [HttpPost]     
      
        public async Task<IActionResult> Summary(CartOrderViewModel details)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            details.ListofCart = _context.Carts.Where(x => x.ApplicationUserId
            == claims.Value).Include("Item").ToList();
            details.OrderHeader.OrderStatus = OrderStatus.StatusPending;
            details.OrderHeader.PaymentStatus = PaymentStatus.StatusPending;
           
            details.OrderHeader.ApplicationUserId = claims.Value;
            foreach (var cart in details.ListofCart)
            {
                details.OrderHeader.OrderTotal += (cart.Item.Price * cart.Count);
            }
            _context.OrderHeaders.Add(details.OrderHeader);
            _context.SaveChanges();
            foreach (var cart in details.ListofCart)
            {
                OrderDetails orderDetail = new OrderDetails()
                {
                    ItemId = cart.ItemId,
                    OrderHeaderId = details.OrderHeader.Id,
                    Count = cart.Count,
                    Price = cart.Item.Price
                };
                _context.OrderDetails.Add(orderDetail);
                _context.SaveChanges();

                
            }
           


            // Card Details Here
                var domain = "https://localhost:7018/";
                var options = new SessionCreateOptions
                {
                    LineItems = new List<SessionLineItemOptions>(),
                    Mode = "payment",
                    SuccessUrl = domain + $"customer/cart/ordersuccess?id={details.OrderHeader.Id}",
                    CancelUrl = domain + $"customer/cart/Index",
                };

                foreach (var cart in details.ListofCart)
                {

                    var lineItemsOptions = new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)(cart.Item.Price * 100),
                            Currency = "usd",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = cart.Item.Title,
                            },

                        },
                        Quantity = cart.Count,
                    };
                    options.LineItems.Add(lineItemsOptions);
                }

            var service = new SessionService();
            Session session = service.Create(options);
            var orderHeader = _context.OrderHeaders.FirstOrDefault(x => x.Id == details.OrderHeader.Id);
            orderHeader.DateofPayment = DateTime.Now;
            orderHeader.SessionId = session.Id;
                              
            _context.SaveChanges();

            _context.Carts.RemoveRange(details.ListofCart);
            _context.SaveChanges();
            var count = _context.Carts.Where(x => x.ApplicationUserId == claims.Value).ToList().Count();
            HttpContext.Session.SetInt32("SessionCart", count);

            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);

        }
        public IActionResult ordersuccess(int id)
        {

            var orderHeader = _context.OrderHeaders.Where(x => x.Id == id).FirstOrDefault();
            var service = new SessionService();
            Session session = service.Get(orderHeader.SessionId);
            if (session.PaymentStatus.ToLower() == "paid")
            {
                orderHeader.PaymentIntentId = session.PaymentIntentId;
                orderHeader.OrderStatus = OrderStatus.StatusApproved;
                orderHeader.PaymentStatus = PaymentStatus.StatusApproved;
               
            }
            List<Cart> cart = _context.Carts.Where(x => x.ApplicationUserId == orderHeader.ApplicationUserId).ToList();
            _context.Carts.RemoveRange(cart);
            _context.SaveChanges();
            return View(id);




        }

        public async Task<IActionResult> plus(int id)
        {
            var cart = await _context.Carts.FirstOrDefaultAsync(c => c.Id == id);
            cart.Count += 1;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> minus(int id)
        {
            var cart = await _context.Carts.FirstOrDefaultAsync(c => c.Id == id);
            
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
           
            if (cart.Count == 1)
            {
                _context.Carts.Remove(cart);
                _context.SaveChanges();
                var count = _context.Carts.Where(x => x.ApplicationUserId == claims.Value).ToList().Count();
                HttpContext.Session.SetInt32("SessionCart", count);
            }
            else
            {


                cart.Count -= 1;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }


        public async Task<IActionResult> delete(int id)
        {


                var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
           

            var cart = await _context.Carts.FirstOrDefaultAsync(c => c.Id == id);
                _context.Carts.Remove(cart);
                _context.SaveChanges();
            var count =  _context.Carts.Where(x=>x.ApplicationUserId==claims.Value).ToList().Count();
            HttpContext.Session.SetInt32("SessionCart", count);
            return RedirectToAction(nameof(Index));
        }

    }



}
