using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace SqlClientTesting.Controllers
{
    public class HomeController : Controller
    {
        public async Task<ActionResult> Index()
        {
            using (var connection = new SqlConnection("Server=localhost;Database=TestDb;User Id=SA;Password=Password1!;"))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
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
                    }
                }

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT * FROM Inventory";
                    using (System.Data.IDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (reader.Read())
                        {
                        }
                    }
                }

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT * FROM Inventory";
                    using (System.Data.IDataReader reader = await command.ExecuteReaderAsync(CancellationToken.None))
                    {
                        while (reader.Read())
                        {
                        }
                    }
                }

                using (var command = new SqlCommand("ProcedureName", connection)
                {
                    CommandType = System.Data.CommandType.StoredProcedure
                })
                {
                    using (System.Data.IDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                        }
                    }
                }

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "INSERT INTO Inventory VALUES (10, 'apple', 200)";
                    command.ExecuteNonQuery();
                }

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "INSERT INTO Inventory VALUES (10, 'apple-async', 200)";
                    await command.ExecuteNonQueryAsync();
                }

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "INSERT INTO Inventory VALUES (10, 'apple-async', 200)";
                    await command.ExecuteNonQueryAsync(CancellationToken.None);
                }

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT Count(*) from Inventory";
                    command.ExecuteScalar();
                }

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT Count(*) from Inventory";
                    await command.ExecuteScalarAsync();
                }

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT Count(*) from Inventory";
                    await command.ExecuteScalarAsync(CancellationToken.None);
                }

                connection.Close();

                Response.End();
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