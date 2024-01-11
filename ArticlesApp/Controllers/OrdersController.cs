using ArticlesApp.Data;
using ArticlesApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ArticlesApp.Controllers
{
    public class OrdersController : Controller
    {

        private readonly ApplicationDbContext db;

        private readonly UserManager<ApplicationUser> _userManager;

        private readonly RoleManager<IdentityRole> _roleManager;

        private IWebHostEnvironment _env;

        public OrdersController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IWebHostEnvironment env
            )
        {
            db = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _env = env;
        }


        
        [Authorize(Roles = "User,Collaborator,Admin")]
        public IActionResult Index()
        {
            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
                ViewBag.Alert = TempData["messageType"];
            }

            SetAccessRights();

            if (User.IsInRole("User") || User.IsInRole("Collaborator"))
            {
                var orders = from order in db.Orders.Include("User")
                               .Where(b => b.UserId == _userManager.GetUserId(User))
                              select order;
                if (orders.Any())
                    ViewBag.Orders = orders;
                else
                    ViewBag.Orders = null;

                return View();
            }
            else
            if (User.IsInRole("Admin"))
            {
                var orders = from order in db.Baskets
                              select order;

                ViewBag.Orders = orders;

                return View();
            }

            else
            {
                return RedirectToAction("Index", "Products");
            }
        }
        

        [HttpGet]
        public IActionResult New()
        {
            Order order = new Order();
            return View(order);
        }

        [HttpPost]
        public IActionResult New(Order order)
        {

            order.UserId = _userManager.GetUserId(User);

            //Basket basket = db.Baskets.Where(bask => bask.UserId == order.UserId)
            //                          .First();

            order.OrderDate = DateTime.Now;

            //preluam id-ul utilizatorului care plaseaza comanda

            order.UserId = _userManager.GetUserId(User);

            if (ModelState.IsValid)
            {

                db.Orders.Add(order);
                db.SaveChanges();
                // transferam produsele de la cos la comanda

                List<ProductBasket> productsInBasket = db.ProductBaskets.Include(pb => pb.Basket)
                                                                        .Where(pb => pb.Basket.UserId == order.UserId)
                                                                        .ToList();

                
                foreach (var productInBasket in productsInBasket)
                {
                    
                    ProductOrder productOrder = new ProductOrder
                    {
                        OrderId = order.Id, // Assuming OrderId is the foreign key in ProductOrder referencing Order
                        ProductId = productInBasket.ProductId // Assuming ProductId is the foreign key in ProductOrder referencing Product
                    };

                    db.ProductOrders.Add(productOrder);
                   
                }
                
                db.ProductBaskets.RemoveRange(productsInBasket);


                db.SaveChanges();
                TempData["message"] = "Produsul a fost adaugat";
                TempData["messageType"] = "alert-success";
                return RedirectToAction("Index");
            }
            else
            {
                return View(order);
            }
        }



        // Conditiile de afisare a butoanelor de editare si stergere
        private void SetAccessRights()
        {
            ViewBag.AfisareButoane = false;

            if (User.IsInRole("Collaborator") || User.IsInRole("User"))
            {
                ViewBag.AfisareButoane = true;
            }

            ViewBag.EsteAdmin = User.IsInRole("Admin");

            ViewBag.UserCurent = _userManager.GetUserId(User);
        }



    }





 


}
