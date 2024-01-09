using ArticlesApp.Controllers;
using ArticlesApp.Data;
using ArticlesApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Linq;

namespace ArticlesApp.Controllers
{
    [Authorize]
    public class ProductsController : Controller
    {

        // PASUL 10 - useri si roluri


        private readonly ApplicationDbContext db;

        private readonly UserManager<ApplicationUser> _userManager;

        private readonly RoleManager<IdentityRole> _roleManager;

        public ProductsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager
            )
        {
            db = context;

            _userManager = userManager;

            _roleManager = roleManager;
        }

        // Se afiseaza lista tuturor produselor impreuna cu categoria 
        // din care fac parte
        // Pentru fiecare produs se afiseaza si userul care a postat produsul respectiv
        // HttpGet implicit
        [Authorize(Roles = "User,Collaborator,Admin")]


        public IActionResult Search(string searchQuery, string? order)
        {


            var products = db.Products.Include("Category").Include("User").Where(p => p.Title.Contains(searchQuery));



            if (order == "asc")
            {
                products = db.Products.Include("Category").Include("User").Where(p => p.Title.Contains(searchQuery)).OrderBy(p => p.pret);
            }

            if (order == "desc")
            {
                products = db.Products.Include("Category").Include("User").Where(p => p.Title.Contains(searchQuery)).OrderByDescending(p => p.pret);
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
            var products = db.Products.Include("Category").Include("User");

            // ViewBag.OriceDenumireSugestiva
            ViewBag.Products = products;

            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
                ViewBag.Alert = TempData["messageType"];
            }

            return View();
        }

        // Se afiseaza un singur articol in functie de id-ul sau 
        // impreuna cu categoria din care face parte
        // In plus sunt preluate si toate comentariile asociate unui articol
        // Se afiseaza si userul care a postat articolul respectiv
        // HttpGet implicit

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
            // Daca modelul este valid
            if (ModelState.IsValid)
            {
                // Verificam daca avem deja produsul in cos
                if (db.ProductBaskets
                    .Where(ab => ab.ProductId == productBasket.ProductId)
                    .Where(ab => ab.BasketId == productBasket.ProductId)
                    .Count() > 0)
                {
                    TempData["message"] = "Acest articol este deja adaugat in colectie";
                    TempData["messageType"] = "alert-danger";
                }
                else
                {
                    // Adaugam asocierea intre product si basket
                    db.ProductBaskets.Add(productBasket);
                    // Salvam modificarile
                    db.SaveChanges();

                    // Adaugam un mesaj de succes
                    TempData["message"] = "Produsul a fost adaugat in cosul de cumparaturi";
                    TempData["messageType"] = "alert-success";
                }

            }
            else
            {
                TempData["message"] = "Nu s-a putut adauga produsul in cos";
                TempData["messageType"] = "alert-danger";
            }

            // Ne intoarcem la pagina articolului
            return Redirect("/Products/Show/" + productBasket.ProductId);
        }


        // Se afiseaza formularul in care se vor completa datele unui articol
        // impreuna cu selectarea categoriei din care face parte
        // Doar utilizatorii cu rolul de Editor sau Admin pot adauga articole in platforma
        // HttpGet implicit

        [Authorize(Roles = "Collaborator,Admin")]
        public IActionResult New()
        {
            Product product = new Product();

            // Se preia lista de categorii cu ajutorul metodei GetAllCategories()
            product.Categ = GetAllCategories();


            return View(product);
        }

        // Se adauga articolul in baza de date
        // Doar utilizatorii cu rolul de Collaborator sau Admin pot adauga articole in platforma

        [Authorize(Roles = "Collaborator,Admin")]
        [HttpPost]
        public IActionResult New(Product product)
        {
            product.Date = DateTime.Now;

            //IFormFile file = Request.Files["ImageData"];

            // preluam id-ul utilizatorului care posteaza articolul
            product.UserId = _userManager.GetUserId(User);
            product.Rating = 0;

            if (ModelState.IsValid)
            {
                db.Products.Add(product);
                db.SaveChanges();
                TempData["message"] = "Produsul a fost adaugat";
                TempData["messageType"] = "alert-success";
                return RedirectToAction("Index");
            }
            else
            {
                product.Categ = GetAllCategories();
                return View(product);
            }
        }


        // Se editeaza un articol existent in baza de date impreuna cu categoria din care face parte
        // Categoria se selecteaza dintr-un dropdown
        // Se afiseaza formularul impreuna cu datele aferente articolului din baza de date
        // Doar utilizatorii cu rolul de Collaborator sau Admin pot edita articole
        // Adminii pot edita orice articol din baza de date
        // Collaboratorii pot edita doar articolele proprii (cele pe care ei le-au postat)
        // HttpGet implicit

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

        // Se adauga articolul modificat in baza de date
        // Verificam rolul utilizatorilor care au dreptul sa editeze (Collaborator sau Admin)
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
                    TempData["message"] = "Produsul a fost modificat";
                    TempData["messageType"] = "alert-success";
                    db.SaveChanges();
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



        // Se sterge un articol din baza de date
        // Utilizatorii cu rolul de Collaborator sau Admin pot sterge articole
        // Collaboratorii pot sterge doar articolele publicate de ei
        // Adminii pot sterge orice articol din baza de date

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
            /* Sau se poate implementa astfel: 
             * 
            foreach (var category in categories)
            {
                var listItem = new SelectListItem();
                listItem.Value = category.Id.ToString();
                listItem.Text = category.CategoryName.ToString();

                selectList.Add(listItem);
             }*/


            // returnam lista de categorii
            return selectList;
        }

        // Metoda utilizata pentru exemplificarea Layout-ului
        // Am adaugat un nou Layout in Views -> Shared -> numit _LayoutNou.cshtml
        // Aceasta metoda are un View asociat care utilizeaza noul layout creat
        // in locul celui default generat de framework numit _Layout.cshtml
        public IActionResult IndexNou()
        {
            return View();
        }
    }
}

