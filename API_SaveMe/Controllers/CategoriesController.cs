using API_SaveMe.HelperModels;
using API_SaveMe.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace API_SaveMe.Controllers
{
    public class CategoriesController : ApiController
    {
        private db_SaveMeTrackerEntities db = new db_SaveMeTrackerEntities();

        /// <summary>
        /// Returns List of all categories
        /// </summary>
        /// <returns></returns>
        [Route("Categories/GetAllCategories")]
        [HttpGet]
        public IHttpActionResult GetAllCategories()
        {
            List<CategoryListModel> categoriesList = new List<CategoryListModel>();
            List<string> categoryNames = db.Categories.Select(e => e.CategoryName).ToList();
            foreach (var item in categoryNames)
            {
                CategoryListModel categoryListModel = new CategoryListModel();
                categoryListModel.CategoryName = item;
                categoriesList.Add(categoryListModel);
            }
            return Ok(categoriesList);
        }
    }
}
