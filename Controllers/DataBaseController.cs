using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.WebPages;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;

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
        [Route("DataBase/Reviews/")]
        public async Task<ActionResult> FullList()
        {
            IEnumerable<Movie> movies;
            if (!Request.QueryString["search"].IsEmpty())
            {
                movies = await GetMovies(Request.QueryString["search"]);
            }
            else
            {
                movies = await GetMovies();
            }
            return View(movies);
        }

        [HttpGet]
        [Route("DataBase/Reviews/{hashCode}")]
        public ActionResult MovieList(int hashCode)
        {
            GetDataBase();
            var myReviews = reviews.Where(x => x.Name.ToLower().GetHashCode() == hashCode)
                .ToList();
            if (myReviews.Count == 0)
            {
                return new RedirectResult("/DataBase/Reviews");
            }
            else
            {
                return View(myReviews);
            }
        }

        [HttpPost]
        public ActionResult ActionRemoveItem(int itemId)
        {
            if (itemId > 0)
            {
                RemoveItem(itemId);
                return Json(new { success = true });
            }
            else
            {
                return Json(new { success = false });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Submit(MovieReview model)
        {
            // Set the poster image url

            await SubmitClick(model);
            
            return new RedirectResult("/DataBase/Index");
        }

        public FormCollection formCollection;
        
        public const string ConnectionString = @"server=localhost;uid=root;pwd=mysql;database=moviedatabase;";
        public MySqlConnection conn = new MySqlConnection(ConnectionString);

        public HtmlElement txtMovieName;
        public string txtMovieScore => formCollection["TextBox2"];
        public string txtMovieDescription => formCollection["TextBox3"];

        private async Task<IEnumerable<Movie>> GetMovies(string searchString = "")
        {
            var movies = new List<Movie>();
            conn.Open();
            var cmd = conn.CreateCommand();
            if (searchString != "")
            {
                cmd.CommandText = "SELECT DISTINCT MovieName FROM ratings WHERE MovieName LIKE @searchString";
                cmd.Parameters.AddWithValue("@searchString", "%" + searchString + "%");
            }
            else
            {
                cmd.CommandText = "SELECT DISTINCT MovieName FROM ratings";
            }
            
            var reader = cmd.ExecuteReader();
            
            while (reader.Read())
            {
                movies.Add(new Movie { Name = reader.GetString(0) });
            }
            
            await conn.CloseAsync();
            
            // Get the poster image url from the database and the rest of the data from the API
            foreach (var movie in movies)
            {
                // Poster
                var poster = await GetPoster(movie.Name);
                movie.poster_path = poster;
            
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri("https://api.themoviedb.org/");
                    var response = await client.GetAsync($"3/search/movie?api_key=fae11d014542d194b478da188762d57f&include_adult=false&query={movie.Name}");
                    var json = await response.Content.ReadAsStringAsync();
                    var movieData = JsonConvert.DeserializeObject<ApiMovieResponse>(json);
                    if(movieData.results.Length > 0)
                    {
                        movie.overview = movieData.results[0].overview;
                    }
                }
            }
            
            
            // Count the number of ratings for each movie name
            
            foreach (var movie in movies)
            {
                conn.Open();
                cmd.CommandText = "SELECT COUNT(*) FROM ratings WHERE MovieName = @movieName";
                cmd.Parameters.AddWithValue("@movieName", movie.Name);
                movie.Reviews = Convert.ToInt32(cmd.ExecuteScalar());
                cmd.Parameters.Clear();
                await conn.CloseAsync();
            }

            await conn.CloseAsync();



            return movies;
        }

        private Task<string> GetPoster(string movieName)
        {
            // Get the poster image url from the database
            return Task.Run(() =>
            {
                string posterUrl = "";
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT poster_url FROM ratings WHERE MovieName = @movieName";
                cmd.Parameters.AddWithValue("@movieName", movieName);
                posterUrl = cmd.ExecuteScalar().ToString();
                cmd.Parameters.Clear();
                conn.Close();
                return posterUrl;
            });
        }

        private IEnumerable<MovieReview> GetDataBase()
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
            return reviews;
        }
        
        [HttpPost]
        public async Task<ActionResult> Search(string searchString)
        {
            if (searchString == null || searchString.Length < 3)
            {
                return Json(new { success = false });   
            }
            
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://api.themoviedb.org/");
                var response = await client.GetAsync($"3/search/movie?api_key=fae11d014542d194b478da188762d57f&include_adult=false&query={searchString}");
                response.EnsureSuccessStatusCode();
                var stringResult = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<ApiMovieResponse>(stringResult);
                return Json(new { success = true, result = result.results });
            }
            
            // var movies = GetMovies();
            // var searchResults = movies.Where(x => x.Name.ToLower().Contains(searchString.ToLower()));
            // return Json(new { success = true, result = searchResults },  JsonRequestBehavior.AllowGet);;
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

        public async Task SubmitClick(MovieReview model)
        {
            if(String.IsNullOrEmpty(model.Name) || String.IsNullOrEmpty(model.Description))
            {
                Debug.Print("Movie name or description is empty");
                return;
            }
            if(model.Rating < 1 || model.Rating > 5)
            {
                Debug.Print("Rating is not between 1 and 5");
                return;
            }
            
            // Get the poster path from the movie API
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://api.themoviedb.org/");
                var response = await client.GetAsync($"3/search/movie?api_key=fae11d014542d194b478da188762d57f&include_adult=false&query={model.Name}");
                response.EnsureSuccessStatusCode();
                var stringResult = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<ApiMovieResponse>(stringResult);
                // return Json(new { success = true, result = result.results });
                model.poster_path = result.results[0].poster_path;
            }
            
            
            
            // Create the command
            var comm = conn.CreateCommand();
            comm.CommandText = "INSERT INTO ratings (MovieName, Score, Description, poster_url) VALUES (@movie_name, @movie_score, @movie_description, @movie_poster_url)";
            comm.Parameters.AddWithValue("@movie_name", model.Name);
            comm.Parameters.AddWithValue("@movie_score", model.Rating);
            comm.Parameters.AddWithValue("@movie_description", model.Description);
            comm.Parameters.AddWithValue("@movie_poster_url", model.poster_path);
            
            // Execute the command
            conn.Open();
            comm.ExecuteNonQuery();
            conn.Close();
            
            // Redirect to the index page
            // Response.Redirect("/DataBase/Index");
        }
    }
}