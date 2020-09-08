using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Warehouse.Data;
using Warehouse.Models;

namespace Warehouse.Controllers
{   
    public class ProductQuantitySummary
    {
        [DisplayName("รหัสสินค้า")]
        public int ProductId { get; set; }
        [DisplayName("ชื่อสินค้า")]
        public string Product_Name { get; set; }
        [DisplayName("จำนวนที่ขายได้")]
        public int Quantity { get; set; }
    }
    [Authorize]
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Sale()
        {
            try
            {
                if (string.IsNullOrEmpty(Request.Cookies["OrderID"]))
                {
                    int lastOrderID = 0;
                    if (await _context.Order.AnyAsync())
                    {
                        lastOrderID = await _context.Order.MaxAsync(o => o.OrderId);
                    }
                    Response.Cookies.Append("OrderID", (lastOrderID + 1).ToString());
                    ViewData["OrderId"] = lastOrderID + 1;
                }
                else
                {
                    ViewData["OrderId"] = Request.Cookies["OrderID"];
                }
            }
            catch (Exception)
            {
                throw;
            }
            ViewData["Success"] = true;
            ViewData["Products"] = await _context.Products.ToListAsync();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Sale(SaleViewModel model)
        {
            try
            {
                var selectedProduct = await _context.Products.FirstOrDefaultAsync(p => p.ProductId == model.ProductId);
                selectedProduct.Quantity_P -= model.Amount;
                var newOrder = new Order()
                {
                    OrderId = model.OrderId,
                    ProductId = model.ProductId,
                    Quantity_O = model.Amount,
                    Date = DateTime.Now,
                    Price = selectedProduct.Price,
                    Total_Price = selectedProduct.Price * model.Amount
                };
                await _context.Order.AddAsync(newOrder);
                await _context.SaveChangesAsync();
                return RedirectToAction("Bill");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            ViewData["Products"] = await _context.Products.ToListAsync();
            return View();
        }
        public async Task<IActionResult> Bill()
        {
            var OrderID = int.Parse(Request.Cookies["OrderID"]);
            var order = await _context.Order.FirstOrDefaultAsync(o => o.OrderId == OrderID);
            Response.Cookies.Delete("OrderID");
            return View(order);
        }
        public async Task<IActionResult> Order()
        {
            return View(await _context.Products.ToListAsync());
        }
        public async Task<IActionResult> Summary(DateTime? searchSum)
        {
            // IList<> , List<>, IQueryable<>, IEnumerable<>	
            List<Order> result;
            if (searchSum != null)
            {
                if (searchSum.Value.Year > 2100)
                {
                    searchSum = searchSum.Value.AddYears(-543);
                }
                result = await _context.Order.Include(o => o.Product).Where(o => o.Date.Date.Date == searchSum.Value.Date).ToListAsync();
            }
            else
            {
                result = await _context.Order.Include(o => o.Product).ToListAsync();

            }
            ViewData["TotalQuantities"] = result.Select(o => o.Quantity_O).Sum();
            List<ProductQuantitySummary> summaries = result.GroupBy(o => new
            {
                o.ProductId,
                o.Product.Product_Name
            }
            , (key, group) => new ProductQuantitySummary { ProductId = key.ProductId, Product_Name = key.Product_Name, Quantity = group.Sum(o => o.Quantity_O) }).ToList();
            ViewData["Summary"] = summaries;
            return View(result);
        }

        public async Task<IActionResult> SummaryMonth(DateTime month)
        {
            List<Order> result;
            if (month != null)
            {
                result = await _context.Order.Include(o => o.Product).Where(o => o.Date.Year == month.Year && o.Date.Month == month.Month).ToListAsync();
            }
            else
            {
                result = await _context.Order.Include(o => o.Product).ToListAsync();
            }
            ViewData["TotalQuantitiesM"] = result.Select(o => o.Quantity_O).Sum();
            List<ProductQuantitySummary> summariesM = result.GroupBy(o => new
            {
                o.ProductId,
                o.Product.Product_Name
            }
            , (key, group) => new ProductQuantitySummary { ProductId = key.ProductId, Product_Name = key.Product_Name, Quantity = group.Sum(o => o.Quantity_O) }).ToList();
            ViewData["SummaryM"] = summariesM;
            var myMonths = _context.Order.OrderBy(o => o.Date).AsEnumerable()
                .Select(o => new DateTime(o.Date.Year, o.Date.Month, 1))
                .Distinct()
                .Select(o => new SelectListItem { Text = o.ToString("MM yyyy"), Value = new DateTime(o.Year, o.Month, 1).ToString() }).ToList();
            ViewData["myMonths"] = myMonths;
            return View(result);
        }
        //GET: Products Search
        public async Task<IActionResult> Index(string searchstring)
        {
            var searchPro = _context.Products.Include(p => p.ProductType).AsQueryable();
            if (!String.IsNullOrEmpty(searchstring))
            {
                searchPro = searchPro.Where(s => s.Product_Name.Contains(searchstring));
            }
            return View(await searchPro.ToListAsync());
        }
        public async Task<IActionResult> Warning()
        {
            return View(await _context.Products.Where(p => p.Quantity_P < 5).ToListAsync());
        }

        // GET: Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.ProductType)
                .FirstOrDefaultAsync(m => m.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: Products/Create
        public IActionResult Create()
        {
            ViewData["TypeId"] = new SelectList(_context.Set<ProductType>(), "TypeId", "TypeId");
            return View();
        }

        // POST: Products/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProductId,Product_Name,TypeId,Price,Quantity_P")] Product product)
        {
            if (ModelState.IsValid)
            {
                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["TypeId"] = new SelectList(_context.Set<ProductType>(), "TypeId", "TypeId", product.TypeId);
            return View(product);
        }

        // GET: Products/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            ViewData["TypeId"] = new SelectList(_context.Set<ProductType>(), "TypeId", "TypeId", product.TypeId);
            return View(product);
        }

        // POST: Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ProductId,Product_Name,TypeId,Price,Quantity_P")] Product product)
        {
            if (id != product.ProductId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(product);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.ProductId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["TypeId"] = new SelectList(_context.Set<ProductType>(), "TypeId", "TypeId", product.TypeId);
            return View(product);
        }

        // GET: Products/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.ProductType)
                .FirstOrDefaultAsync(m => m.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.ProductId == id);
        }
    }
}
