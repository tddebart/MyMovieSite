using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using MySql.Data.MySqlClient;

namespace MyMovieSite.Controllers
{
    public class DataBaseController : Controller
    {
        List<MovieReview> reviews = new List<MovieReview>();
        
        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult List()
        {
            GetDataBase();
            return View(reviews);
        }

        [HttpPost]
        public ActionResult ActionRemoveItem(int itemId)
        {
            if (itemId > 0)
            {
                RemoveItem(itemId);
            }
            return new RedirectResult("/DataBase/Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Submit(MovieReview model)
        {
            if (ModelState.IsValid)
            {
                Debug.Print("Model is valid");
            }
            // formCollection = collection;
            SubmitClick(model.Name,model.Rating, model.Description);
            return new RedirectResult("/DataBase/Index");
        }

        public FormCollection formCollection;
        
        public const string ConnectionString = @"server=localhost;uid=root;pwd=mysql;database=moviedatabase;";
        public MySqlConnection conn = new MySqlConnection(ConnectionString);

        public HtmlElement txtMovieName;
        public string txtMovieScore => formCollection["TextBox2"];
        public string txtMovieDescription => formCollection["TextBox3"];

        void GetDataBase()
        {
            reviews.Clear();
            var comm = conn.CreateCommand();
            comm.CommandText = "SELECT * FROM ratings";
            
            conn.Open();
            var reader = comm.ExecuteReader();

            while (reader.Read())
            {
                reviews.Add(new MovieReview()
                {
                    id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Rating = int.Parse(reader.GetString(2)),
                    Description = reader.GetString(3)
                });
            }


            conn.Close();
        }
        
        [System.Web.Services.WebMethod]
        public void RemoveItem(int id)
        {
            var comm = conn.CreateCommand();
            comm.CommandText = "DELETE FROM ratings WHERE Id = @id";
            comm.Parameters.AddWithValue("@id", id);
            conn.Open();
            comm.ExecuteNonQuery();
            conn.Close();
        }

        public void SubmitClick(string movieName,int rating, string movieDescription)
        {
            if(String.IsNullOrEmpty(movieName) || String.IsNullOrEmpty(movieDescription))
            {
                Debug.Print("Movie name or description is empty");
                return;
            }
            if(rating < 1 || rating > 5)
            {
                Debug.Print("Rating is not between 1 and 5");
                return;
            }
            
            
            // Create the command
            var comm = conn.CreateCommand();
            comm.CommandText = "INSERT INTO ratings (MovieName, Score, Description) VALUES (@movie_name, @movie_score, @movie_description)";
            comm.Parameters.AddWithValue("@movie_name", movieName);
            comm.Parameters.AddWithValue("@movie_score", rating);
            comm.Parameters.AddWithValue("@movie_description", movieDescription);
            
            // Execute the command
            conn.Open();
            comm.ExecuteNonQuery();
            conn.Close();
            
            // Redirect to the index page
            // Response.Redirect("/DataBase/Index");
        }
    }
}