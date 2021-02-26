using System;
using System.Data.SqlClient;
using System.Configuration;
using System.Collections;


namespace Ovning1
{

    /*
    
        --- ORM ---
        (av koden i uppg 1c)


         
    */



    class Program
    {

        /*
            Skriv ett program som ber användaren om ett namn och letar efter personen i tabellen Personer. 
            Om personen inte hittas ber programmet först om personens längd samt sparar personen som en ny rad i tabellen Personer.
            Programmet ber om personens vikt som sparas som en ny rad i tabellen Mätningar med dagens datum.
        */
        static void Main()
        {
            string insertname = Console.ReadLine();


            bool found; // = FindPerson(insertname);




            Console.ReadKey();
        }

        class Database
        {
            SqlConnection connection;

            public Database()
            {
                connection = new SqlConnection(ConfigurationManager.AppSettings["personconnection"]);
            }

            public string FindPerson(string name)
            {
                using(SqlCommand findcommand = new SqlCommand())
                {
                    findcommand.CommandText = "select namn from Personer where namn = @name";
                    findcommand.Parameters.AddWithValue("@name", name);
                    connection.Open();
                    string found = (string)findcommand.ExecuteScalar();
                    connection.Close();
                    if (found ==null)
                    {
                        /// görs ny person!!!!
                        ///  till Personer (namn + längd)
                        ///  
                    }
                    
                    //lägger till vikt mm. till mätningar (namn + datum + vikt)

                }
            }
        }

        class Measure
        {
            public Measure(string name, double weight)
            {
                Name = name;
                Weight = weight;
                Date = DateTime.Today;
            }

            public Measure(string name, DateTime mdate, double weight)
            {
                Name = name;
                Weight = weight;
                Date = mdate;
            }

            public string Name { get; set; }
            public DateTime Date { get; set; }
            public double Weight { get; set; }


        }

        class Person
        {
            public Person(string pname, int plength)
            {
                PersonName = pname;
                PersonLength = plength;
            }

            public string PersonName { get; set; }
            public int PersonLength { get; set; }
        }
    }
}


