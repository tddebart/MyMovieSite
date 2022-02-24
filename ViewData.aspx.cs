using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Web.UI;
using System.Web.UI.WebControls;
using MySql.Data.MySqlClient;

namespace MyMovieSite
{
    public partial class View_data : Page
    {
        public const string ConnectionString = @"server=localhost;uid=root;pwd=mysql;database=moviedatabase;";
        public MySqlConnection conn = new MySqlConnection(ConnectionString);

        public TextBox txtMovieName => FindControl("TextBox1") as TextBox;
        public TextBox txtMovieScore => FindControl("TextBox2") as TextBox;
        public TextBox txtMovieDescription => FindControl("TextBox3") as TextBox;
        
        protected void Page_Load()
        {
            var watch = new Stopwatch();
            watch.Start();
            var comm = conn.CreateCommand();
            comm.CommandText = "SELECT * FROM ratings";

            conn.Open();
            var reader = comm.ExecuteReader();
            var data = new string[10];
            var i = 0;

            while (reader.Read())
            {
                data[i] = reader.GetString(1);
                i++;
            }
            
            foreach (var item in data)
            {
                Response.Write(item+"<br>");
            }
            conn.Close();
            watch.Stop();
            Response.Write("Took " + watch.ElapsedMilliseconds + " milliseconds");

        }
        
        public void Button1_Click(object sender, EventArgs e)
        {
            // Get the information from the textboxes
            var movieName = txtMovieName.Text;
            var movieScore = txtMovieScore.Text;
            var movieDescription = txtMovieDescription.Text;
            
            // Create the command
            var comm = conn.CreateCommand();
            comm.CommandText = "INSERT INTO ratings (MovieName, Score, Description) VALUES (@movie_name, @movie_score, @movie_description)";
            comm.Parameters.AddWithValue("@movie_name", movieName);
            comm.Parameters.AddWithValue("@movie_score", int.Parse(movieScore));
            comm.Parameters.AddWithValue("@movie_description", movieDescription);

            // Execute the command
            conn.Open();
            comm.ExecuteNonQuery();
            conn.Close();
            
            // Clear the textboxes
            txtMovieName.Text = "";
            txtMovieScore.Text = "";
            txtMovieDescription.Text = "";
            
            // Refresh the page
            Response.Redirect(Request.RawUrl);
        }
    }
}