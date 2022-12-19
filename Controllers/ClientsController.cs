using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using CrystalDecisions.CrystalReports.Engine;
using Eight_Evid_01.Models;
using Eight_Evid_01.ViewMidels;

namespace Eight_Evid_01.Controllers
{
    public class ClientsController : Controller
    {
        private BookingDbContext db = new BookingDbContext();

        public ActionResult Index()
        {
            return View(db.Clients.Include(x=>x.BookingEntries).ToList());
        }

        public ActionResult AddNewSpot(int? id)
        {
            ViewBag.spots = new SelectList(db.Spots.ToList(), "SpotId", "SpotName", id.ToString() ?? "");
            return PartialView("_addSpot");
        }


        public ActionResult Create()
        {
            return View();
        }


        [HttpPost]
        public ActionResult Create( ClientVM clientVM, int[] spotId)
        {
            if (ModelState.IsValid)
            {
                Client client = new Client()
                {
                    ClientName = clientVM.ClientName,
                    BirthDate = clientVM.BirthDate,
                    Age = clientVM.Age,
                    MaritalState = clientVM.MaritalState
                };
                HttpPostedFileBase file = clientVM.PictureFile;
                if (file!=null)
                {
                    string filePath= Path.Combine("/Images/",Guid.NewGuid().ToString()+Path.GetExtension(file.FileName));
                    file.SaveAs(Server.MapPath(filePath));
                    client.Picture = filePath;
                }

                foreach (var item in spotId)
                {
                    BookingEntry bookingEntry = new BookingEntry()
                    {
                        Client = client,
                        ClientId = client.ClientId,
                        SpotId = item
                    };
                    db.BookingEntries.Add(bookingEntry);
                }
                db.SaveChanges();
            }

            return RedirectToAction("Index");
        }


        public ActionResult Edit(int? id)
        {
            Client client = db.Clients.First(x => x.ClientId == id);
            ClientVM clientVM = new ClientVM()
            {
                ClientId= client.ClientId,
                ClientName = client.ClientName,
                BirthDate = client.BirthDate,
                Age = client.Age,
                MaritalState = client.MaritalState
            };

            var clientSpots=db.BookingEntries.Where(x=>x.ClientId== id).ToList();
            foreach (var item in clientSpots)
            {
                clientVM.SpotList.Add(item.SpotId);
            }
            return View(clientVM);
        }


        [HttpPost]
        public ActionResult Edit(ClientVM clientVM, int[] spotId)
        {
            if (ModelState.IsValid)
            {
                Client client = new Client()
                {
                    ClientId=clientVM.ClientId,
                    ClientName = clientVM.ClientName,
                    BirthDate = clientVM.BirthDate,
                    Age = clientVM.Age,
                    MaritalState = clientVM.MaritalState
                };
                HttpPostedFileBase file = clientVM.PictureFile;
                if (file != null)
                {
                    string filePath = Path.Combine("/Images/", Guid.NewGuid().ToString() + Path.GetExtension(file.FileName));
                    file.SaveAs(Server.MapPath(filePath));
                    client.Picture = filePath;
                }

                var clientSpots = db.BookingEntries.Where(x=>x.ClientId==clientVM.ClientId).ToList();
                foreach (var item in clientSpots)
                {
                    db.BookingEntries.Remove(item);
                }
                foreach (var item in spotId)
                {
                    BookingEntry bookingEntry = new BookingEntry()
                    {
                        
                        ClientId = client.ClientId,
                        SpotId = item
                    };
                    db.BookingEntries.Add(bookingEntry);
                }
                db.Entry(client).State= EntityState.Modified;
                db.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        public ActionResult Delete(int? id)
        {
            Client client = db.Clients.First(x => x.ClientId == id);

            var clientSpots = db.BookingEntries.Where(x => x.ClientId == id ).ToList();
            foreach (var item in clientSpots)
            {
                db.BookingEntries.Remove(item);
            }
            db.Entry(client).State = EntityState.Deleted;
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult Report()
        {
            List<Client> allclients = new List<Client>();
            allclients=db.Clients.ToList();

            ReportDocument rd=new ReportDocument();
            rd.Load(Server.MapPath("~/ClientReport.rpt"));
            rd.SetDataSource(allclients);
            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();

            Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            stream.Seek(0, SeekOrigin.Begin);

            return File(stream,"application/pdf","ClientList.pdf");
        }
    }
}
