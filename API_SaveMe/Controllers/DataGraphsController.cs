using API_SaveMe.Constants;
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
    public class DataGraphsController : ApiController
    {
        private db_SaveMeTrackerEntities db = new db_SaveMeTrackerEntities();

        /// <summary>
        /// Gets the total savings of a user
        /// </summary>
        /// <param name="userDataID"></param>
        /// <returns></returns>
        [Route("Savings/GetTotalSavings/{userDataID}")]
        [HttpGet]
        public IHttpActionResult GetTotalSavings(int userDataID)
        {
            if (this.TotalSavingsExistForUser(userDataID))
            {
                decimal totalSavingsAmount = db.TotalSavings.Where(e => e.UserDataID == userDataID).FirstOrDefault().Amount;
                return Ok(totalSavingsAmount);
            }
            else
            {
                return BadRequest();
            }

        }

        /// <summary>
        /// Gets aggregated data to display graph
        /// </summary>
        /// <param name="dataRequestModel"></param>
        /// <returns></returns>
        [Route("Graph/GetData")]
        [HttpPost]
        public IHttpActionResult GetGraphData(DataRequestModel dataRequestModel)
        {
            if (dataRequestModel.UserDataID != 0)
            {
                int year = DateTime.UtcNow.Year;
                if (dataRequestModel.Type == GlobalConstants.Type_Amount ||
                    dataRequestModel.Type == GlobalConstants.Type_Category)
                {
                    bool hasCategory = dataRequestModel.Type == GlobalConstants.Type_Amount ? false : true;
                    
                    int categoryID = hasCategory? db.Categories.Where(e => e.CategoryName.Equals(dataRequestModel.CategoryName)).FirstOrDefault().CategoryID : 0;                    

                    if (dataRequestModel.Period == GlobalConstants.Period_CurrentMonth ||
                       dataRequestModel.Period == GlobalConstants.Period_PreviousMonth)
                    {
                        //List<SavingsWeekly> weeklySavingsData = this.GetWeeklyExpenseData(dataRequestModel.Period, dataRequestModel.UserDataID, hasCategory, categoryID);
                        List<SavingsCategory> weeklySavingsData = this.GetExpenseDataCategoryWise(dataRequestModel.Period, dataRequestModel.UserDataID, hasCategory, categoryID,year);
                        if (weeklySavingsData != null && weeklySavingsData.Any())
                        {
                            return Ok(weeklySavingsData);
                        }
                        else
                        {
                            return Ok(new List<SavingsCategory>());
                        }
                    }
                    else if (dataRequestModel.Period == GlobalConstants.Period_HalfYearly ||
                            dataRequestModel.Period == GlobalConstants.Period_FullYear)
                    {
                        List<SavingsMonthly> monthlySavingsdata = this.GetMonthlyExpenseData(dataRequestModel.Period, dataRequestModel.UserDataID, hasCategory, categoryID);
                        if (monthlySavingsdata != null && monthlySavingsdata.Any())
                        {
                            return Ok(monthlySavingsdata);
                        }
                        else
                        {
                            return Ok(new List<SavingsMonthly>());
                        }
                    }
                    else
                    {
                        return BadRequest();
                    }
                }
                else
                {
                    return BadRequest();
                }

            }
            else
            {
                return BadRequest();
            }
        }

        /// <summary>
        /// Aggregates data week wise
        /// </summary>
        /// <param name="timePeriod"></param>
        /// <param name="userDataID"></param>
        /// <param name="hasCategory"></param>
        /// <param name="categoryID"></param>
        /// <returns></returns>
        public List<SavingsWeekly> GetWeeklyExpenseData(string timePeriod, int userDataID, bool hasCategory, int categoryID)
        {
            List<SavingsWeekly> savingsWeekly = new List<SavingsWeekly>();
            int intializer = 0;
            List<WeeklySaving> weeklyData = new List<WeeklySaving>();
            int month = timePeriod.Equals("CurrentMonth") ? DateTime.UtcNow.Month : timePeriod.Equals("PreviousMonth") ? DateTime.UtcNow.Month - 1 : 0;
            if (month != 0)
            {
                if (hasCategory && categoryID != 0)
                {
                    var data = from w in db.WeeklySavings
                               where w.CurrentMonth == month &&
                               w.CurrentYear == DateTime.UtcNow.Year &&
                               w.CategoryID == categoryID &&
                               w.UserDataID == userDataID
                               orderby w.CurrentWeek
                               select w;
                    weeklyData = data.ToList();

                }
                else
                {
                    var data = from w in db.WeeklySavings
                               where w.CurrentMonth == month &&
                               w.CurrentYear == DateTime.UtcNow.Year &&
                               w.UserDataID == userDataID
                               orderby w.CurrentWeek
                               select w;
                    weeklyData = data.ToList();
                }
                if(weeklyData.Any())
                {
                    int maxWeek = weeklyData.Select(e => e.CurrentWeek).Max();
                    int minWeek = weeklyData.Select(e => e.CurrentWeek).Min();
                    for (int i = minWeek; i <= maxWeek; i++)
                    {
                        SavingsWeekly saving = new SavingsWeekly();
                        saving.WeekNumber = Convert.ToString(intializer++);
                        decimal v = weeklyData.Where(e => e.CurrentWeek == i).Sum(s => s.Amount);
                        saving.Amount = v;
                        savingsWeekly.Add(saving);
                    }
                }                
            }
            return savingsWeekly;
        }

        /// <summary>
        /// Aggregates data month wise
        /// </summary>
        /// <param name="timePeriod"></param>
        /// <param name="userDataID"></param>
        /// <param name="hasCategory"></param>
        /// <param name="categoryID"></param>
        /// <returns></returns>
        public List<SavingsMonthly> GetMonthlyExpenseData(string timePeriod, int userDataID, bool hasCategory, int categoryID)
        {
            List<SavingsMonthly> savingsMonthly = new List<SavingsMonthly>();
            Tuple<List<int>, List<int>> range = this.GetMonthAndYearRange(timePeriod);
            List<int> months = range.Item1;
            List<int> years = range.Item2;
            List<MonthlySaving> monthlySavingData = new List<MonthlySaving>();
            if (hasCategory && categoryID != 0)
            {
                var expensedata = from m in db.MonthlySavings
                                  where m.UserDataID == userDataID &&
                                  m.CategoryID == categoryID &&
                                  months.Contains(m.CurrentMonth) &&
                                  years.Contains(m.CurrentYear)
                                  orderby m.CurrentMonth
                                  select m;
                monthlySavingData = expensedata.ToList();

            }
            else
            {
                var expensedata = from m in db.MonthlySavings
                                  where m.UserDataID == userDataID &&
                                  months.Contains(m.CurrentMonth) &&
                                  years.Contains(m.CurrentYear)
                                  orderby m.CurrentMonth
                                  select m;
                monthlySavingData = expensedata.ToList();
            }
            if(monthlySavingData.Any())
            {
                foreach (var year in years)
                {
                    int maxMonth = 0;
                    int minMonth = 0;
                    var dataCheck = monthlySavingData.Where(y => y.CurrentYear == year);
                    if (dataCheck.Any())
                    {
                        maxMonth = monthlySavingData.Where(y => y.CurrentYear == year).Select(e => e.CurrentMonth).Max();
                        minMonth = monthlySavingData.Where(y => y.CurrentYear == year).Select(e => e.CurrentMonth).Min();
                        for (int i = minMonth; i <= maxMonth; i++)
                        {
                            SavingsMonthly saving = new SavingsMonthly();
                            saving.MonthNumber = Convert.ToString(i);
                            decimal totalvalue = monthlySavingData.Where(e => e.CurrentMonth == i && e.CurrentYear == year).Sum(s => s.Amount);
                            saving.Amount = totalvalue;
                            savingsMonthly.Add(saving);
                        }
                    }                    
                    
                }
            }            
            return savingsMonthly;
        }

        /// <summary>
        /// Aggregates data month wise
        /// </summary>
        /// <param name="timePeriod"></param>
        /// <param name="userDataID"></param>
        /// <param name="hasCategory"></param>
        /// <param name="categoryID"></param>
        /// <returns></returns>
        public List<SavingsCategory> GetExpenseDataCategoryWise(string timePeriod, int userDataID, bool hasCategory, int categoryID, int year)
        {
            List<SavingsCategory> savingsCategories= new List<SavingsCategory>();            
            List<MonthlySaving> monthlySavingData = new List<MonthlySaving>();
            int month = timePeriod.Equals("CurrentMonth") ? DateTime.UtcNow.Month : timePeriod.Equals("PreviousMonth") ? DateTime.UtcNow.Month - 1 : 0;
            if (month != 0)
            {
                if (hasCategory && categoryID != 0)
                {
                    var expensedata = from m in db.MonthlySavings
                                      where m.UserDataID == userDataID &&
                                      m.CategoryID == categoryID &&
                                      m.CurrentMonth == month &&
                                      m.CurrentYear == year
                                      select m;
                    monthlySavingData = expensedata.ToList();
                }
                else
                {
                    var expensedata = from m in db.MonthlySavings
                                      where m.UserDataID == userDataID &&
                                       m.CurrentMonth == month &&
                                      m.CurrentYear == year
                                      select m;
                    monthlySavingData = expensedata.ToList();
                }
                if (monthlySavingData.Any())
                {
                    foreach(var item in monthlySavingData)
                    {
                        SavingsCategory saving = new SavingsCategory();
                        
                        saving.Category = db.Categories.Where(c=>c.CategoryID == item.CategoryID).FirstOrDefault().CategoryName;
                        saving.Amount = item.Amount;
                        savingsCategories.Add(saving);
                    }                    
                }
            }            
            return savingsCategories;
        }

        /// <summary>
        /// Gets month and year ranges based on current date
        /// </summary>
        /// <param name="timePeriod"></param>
        /// <returns></returns>
        public Tuple<List<int>, List<int>> GetMonthAndYearRange(string timePeriod)
        {
            List<int> months = new List<int>();
            List<int> years = new List<int>();
            DateTime today = DateTime.Now;
            int currentMonth = today.Month;
            int currentYear = today.Year;
            int startMonth = 1;
            int lastMonth = 12;
            if (timePeriod == GlobalConstants.Period_HalfYearly)
            {
                DateTime backPeriodStart = today.AddMonths(-6);
                int startSixMonths = backPeriodStart.Month;
                if (startSixMonths > currentMonth)
                {
                    int yearVal = today.AddYears(-1).Year;
                    years.Add(yearVal);
                    years.Add(currentYear);
                    for (int i = startSixMonths; i <= lastMonth; i++)
                    {
                        months.Add(i);
                    }
                    for (int j = startMonth; j <= currentMonth; j++)
                    {
                        months.Add(j);
                    }
                }
                else
                {
                    years.Add(currentYear);
                    for (int k = startSixMonths; k <= currentMonth; k++)
                    {
                        months.Add(k);
                    }
                }
            }
            else
            {
                years.Add(today.AddYears(-1).Year);
                years.Add(currentYear);

                for (int i = startMonth; i <= currentMonth; i++)
                {
                    months.Add(i);
                }
                for (int j = currentMonth; j <= lastMonth; j++)
                {
                    months.Add(j);
                }

            }
            return Tuple.Create(months, years);
        }

        /// <summary>
        /// Check if TotalSavings exists for a user and category
        /// </summary>
        /// <param name="userDataID"></param>
        /// <returns></returns>
        private bool TotalSavingsExistForUser(int userDataID)
        {
            return db.TotalSavings.Count(e => e.UserDataID == userDataID) > 0;
        }
    }
}
