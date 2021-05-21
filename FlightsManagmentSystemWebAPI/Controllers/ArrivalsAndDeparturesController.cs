using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FlightsManagmentSystemWebAPI.Controllers
{
    public class ArrivalsAndDeparturesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
