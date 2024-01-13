using Calgos.Controllers;
using Calgos.Data;
using Calgos.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Linq;

namespace Calgos.Controllers
{
    [Authorize]
    public class ProductsController : Controller
    {

        // PASUL 10 - useri si roluri


        private readonly ApplicationDbContext db;

        private readonly UserManager<ApplicationUser> _userManager;

        private readonly RoleManager<IdentityRole> _roleManager;

        private IWebHostEnvironment _env;

        public ProductsController(
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

        // Se afiseaza lista tuturor produselor impreuna cu categoria 
        // din care fac parte
        // Pentru fiecare produs se afiseaza si userul care a postat produsul respectiv
        // HttpGet implicit
        [Authorize(Roles = "User,Collaborator,Admin")]
        public IActionResult Search(string searchQuery, string? order)
        {

            var products = db.Products.Include("Category").Include("User").Where(p => p.Title.Contains(searchQuery) || p.Category.CategoryName.Contains(searchQuery));


            ViewBag.UserBaskets = db.Baskets
                                      .Where(b => b.UserId == _userManager.GetUserId(User))
                                      .ToList();
            if (order == "asc")
            {
                products = db.Products.Include("Category").Include("User").Where(p => p.Title.Contains(searchQuery) || p.Category.CategoryName.Contains(searchQuery)).OrderBy(p => p.pret);
            }

            if (order == "desc")
            {
                products = db.Products.Include("Category").Include("User").Where(p => p.Title.Contains(searchQuery) || p.Category.CategoryName.Contains(searchQuery)).OrderByDescending(p => p.pret);
            }

            ViewBag.Products = products;

            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
                ViewBag.Alert = TempData["messageType"];
            }

            ViewBag.searchQuery = searchQuery;
            ViewBag.order = order;

            return View();
        }

        public IActionResult Index()
        {
            var approvedProducts = db.Products.Where(p => p.Approved).Include("Category").Include("User");
            var unapprovedProducts = db.Products.Where(p => !p.Approved).Include("Category").Include("User");

            ViewBag.UserBaskets = db.Baskets
                                          .Where(b => b.UserId == _userManager.GetUserId(User))
                                          .ToList();
            ViewBag.approvedProducts = approvedProducts;
            ViewBag.unapprovedProducts = unapprovedProducts;

            ViewBag.showUnapprovedProducts = unapprovedProducts.Count() > 0;

            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
                ViewBag.Alert = TempData["messageType"];
            }


            SetAccessRights();

            return View();
        }



        [Authorize(Roles = "User,Collaborator,Admin")]
        public IActionResult Show(int id)
        {
            Product product= db.Products.Include("Category")
                                         .Include("User")
                                         .Include("Reviews")
                                         .Include("Reviews.User")
                                         .Where(prod => prod.Id == id)
                                         .First();

            
            ViewBag.UserBaskets = db.Baskets
                                      .Where(b => b.UserId == _userManager.GetUserId(User))
                                      .ToList();


            SetAccessRights();

            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
                ViewBag.Alert = TempData["messageType"];
            }

            return View(product);
        }





        // Adaugarea unui review asociat unui produs in baza de date
        // Toate rolurile pot adauga reviewuri in baza de date
        [HttpPost]
        [Authorize(Roles = "User,Collaborator,Admin")]
        public IActionResult Show([FromForm] Review rev)
        {
            rev.Date = DateTime.Now;
            rev.UserId = _userManager.GetUserId(User);

            if (ModelState.IsValid)
            {
                db.Reviews.Add(rev);
                db.SaveChanges();
                var reviews = db.Reviews.Where(r => r.ProductId == rev.ProductId).ToList();
                double averageRating = reviews.Average(r => r.Points);

                var product = db.Products.Find(rev.ProductId);
                product.Rating = averageRating;
        
                db.SaveChanges();
                return Redirect("/Products/Show/" + rev.ProductId);
            }

            else
            {
                Product prod = db.Products.Include("Category")
                                         .Include("User")
                                         .Include("Review")
                                         .Include("Review.User")
                                         .Where(prod => prod.Id == rev.ProductId)
                                         .First();


                
                ViewBag.UserBaskets = db.Baskets
                                          .Where(b => b.UserId == _userManager.GetUserId(User))
                                          .ToList();

                SetAccessRights();

                return View(prod);
            }
        }
        
        [HttpPost]
        public IActionResult AddBasket([FromForm] ProductBasket productBasket)
        {
          
            if (ModelState.IsValid)
            {
                // Verificam daca avem deja produsul in cos
                if (db.ProductBaskets
                    .Where(ab => ab.ProductId == productBasket.ProductId)
                    .Where(ab => ab.BasketId == productBasket.ProductId)
                    .Count() > 0)
                {
                    TempData["message"] = "Acest produs este deja adaugat in cos";
                    TempData["messageType"] = "alert-danger";
                }
                else
                {
                    // Adaugam asocierea intre product si basket
                    db.ProductBaskets.Add(productBasket);
                 
                    db.SaveChanges();

                   
                    TempData["message"] = "Produsul a fost adaugat in cosul de cumparaturi";
                    TempData["messageType"] = "alert-success";
                }

            }
            else
            {
                TempData["message"] = "Nu s-a putut adauga produsul in cos";
                TempData["messageType"] = "alert-danger";
            }

      
            return Redirect("/Products/Show/" + productBasket.ProductId);
        }


        

        [Authorize(Roles = "Collaborator,Admin")]
        public IActionResult New()
        {
            Product product = new Product();

          
            product.Categ = GetAllCategories();


            return View(product);
        }




        [Authorize(Roles = "Collaborator,Admin")]
        [HttpPost]
        public async Task<IActionResult> New(Product product, IFormFile Image)
        {
            if (Image != null && Image.Length > 0)
            {
                var storagePath = Path.Combine(
                    _env.WebRootPath,
                    "images",
                    Image.FileName
                );

                var databaseFileName = "/images/" + Image.FileName;

                using (var fileStream = new FileStream(storagePath, FileMode.Create))
                {
                    await Image.CopyToAsync(fileStream);
                }

                product.Image = databaseFileName;
            }

            product.Date = DateTime.Now;
            product.Approved = false;
       
            product.UserId = _userManager.GetUserId(User);
            product.Rating = 0;

            if (ModelState.IsValid)
            {
                db.Products.Add(product);
                db.SaveChanges();
                TempData["message"] = "Produsul asteapta aprobare";
                TempData["messageType"] = "alert-success";
                return RedirectToAction("Index");
            }
            else
            {
                product.Categ = GetAllCategories();
                return View(product);
            }
        }

        [Authorize(Roles = "Admin")]

        public IActionResult Approve(int id)
        {
            Product product = db.Products.Include("Category")
                                        .Where(prod => prod.Id == id)
                                        .First();

            product.Approved = true;
            db.SaveChanges();

            return RedirectToAction("Index");
        }
        

        [Authorize(Roles = "Collaborator,Admin")]
        public IActionResult Edit(int id)
        {

            Product product = db.Products.Include("Category")
                                        .Where(prod => prod.Id == id)
                                        .First();

            product.Categ = GetAllCategories();



            if (product.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                return View(product);
            }

            else
            {
                TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra unui produs care nu va apartine";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }

        }


        [HttpPost]
        [Authorize(Roles = "Collaborator,Admin")]
        public IActionResult Edit(int id, Product requestProduct)
        {
            Product product = db.Products.Find(id);

            if (ModelState.IsValid)
            {
                if (product.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
                {
               
                    product.Title = requestProduct.Title;
                    product.Description = requestProduct.Description;
                    product.CategoryId = requestProduct.CategoryId;


                    
                    db.SaveChanges();

                    TempData["message"] = "Produsul a fost modificat";
                    TempData["messageType"] = "alert-success";
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra unui produs care nu va apartine";
                    TempData["messageType"] = "alert-danger";
                    return RedirectToAction("Index");
                }
            }
            else
            {
                requestProduct.Categ = GetAllCategories();
                return View(requestProduct);
            }
        }




       

        [HttpPost]
        [Authorize(Roles = "Collaborator,Admin")]
        public ActionResult Delete(int id)
        {
            Product product = db.Products.Include("Reviews")
                                         .Where(prod => prod.Id == id)
                                         .First();

            if (product.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {   
                if (product.Reviews.Count > 0)
                {
                    foreach (var rev in product.Reviews)
                    {
                        db.Reviews.Remove(rev);
                    }
                }
                db.Products.Remove(product);
                db.SaveChanges();
                TempData["message"] = "Produsul a fost sters";
                TempData["messageType"] = "alert-success";
                return RedirectToAction("Index");
            }

            else
            {
                TempData["message"] = "Nu aveti dreptul sa stergeti un produs care nu va apartine";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }
        }


        // Conditiile de afisare a butoanelor de editare si stergere
        private void SetAccessRights()
        {
            ViewBag.AfisareButoane = false;

            if (User.IsInRole("Collaborator"))
            {
                ViewBag.AfisareButoane = true;
            }

            ViewBag.EsteAdmin = User.IsInRole("Admin");

            ViewBag.UserCurent = _userManager.GetUserId(User);
        }

        [NonAction]
        public IEnumerable<SelectListItem> GetAllCategories()
        {
            // generam o lista de tipul SelectListItem fara elemente
            var selectList = new List<SelectListItem>();

            // extragem toate categoriile din baza de date
            var categories = from cat in db.Categories
                             select cat;

            // iteram prin categorii
            foreach (var category in categories)
            {
                // adaugam in lista elementele necesare pentru dropdown
                // id-ul categoriei si denumirea acesteia
                selectList.Add(new SelectListItem
                {
                    Value = category.Id.ToString(),
                    Text = category.CategoryName.ToString()
                });
            }
            


            // returnam lista de categorii
            return selectList;
        }

        public IActionResult IndexNou()
        {
            return View();
        }
    }
}

