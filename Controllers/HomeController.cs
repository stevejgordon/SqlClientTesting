using System.Data.SqlClient;
using System.Web.Mvc;

namespace SqlClientTesting.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            using (var connection = new SqlConnection("Server=localhost;Database=TestDb;User Id=SA;Password=Password1!;"))
            {
                connection.Open();
                using (System.Data.IDbCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT * FROM Inventory";
                    using (System.Data.IDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                Response.Write($"{reader.GetName(i)}: {reader.GetValue(i)}<br/>");
                            }
                            Response.Write("<hr/>");
                        }
                        Response.End();
                    }

                    //using (var command2 = new SqlCommand("ProcedureName", connection)
                    //{
                    //    CommandType = System.Data.CommandType.StoredProcedure
                    //})
                    //{
                    //    command2.ExecuteNonQuery();
                    //}
                    connection.Close();
                }

                return View();
            }
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}