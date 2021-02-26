using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;
using System.Configuration;
using System.Collections;

namespace Ovning1
{
    class Program
    {
        static void Main()
        {
            string myconnection = ConfigurationManager.AppSettings["personconnection"];

            using (SqlConnection connection = new SqlConnection())
            {
                connection.ConnectionString = myconnection;

                //a - lista alla Personer
                using (SqlCommand command2 = new SqlCommand())
                {
                    command2.Connection = connection;
                    command2.CommandText = "select * from Personer";
                    connection.Open();
                    SqlDataReader reader = command2.ExecuteReader();
                    if (reader.HasRows)
                    {
                        Console.WriteLine("Personerna är: ");
                        while (reader.Read())
                        {
                            string person = reader[0].ToString();
                            int längd = Convert.ToInt32(reader[1]);
                            Console.WriteLine(person + ", " + längd);
                        }
                        reader.Close();
                    }
                    connection.Close();
                }

                //b - en viss persons längd, senaste mätningens bmi oc alla mätningar
                Console.WriteLine("Person? ");
                string name = Console.ReadLine();

                //personens längd
                int length = 0;
                using (SqlCommand lcommand = new SqlCommand())
                {
                    lcommand.Connection = connection;
                    lcommand.CommandText = "select längd from Personer where namn = @name";
                    lcommand.Parameters.AddWithValue("@name", name);
                    connection.Open();
                    
                    try
                    {
                        length = (int)lcommand.ExecuteScalar();
                        Console.WriteLine("Längden är : " + length);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Ingen längd då personen saknas");
                    }
                    connection.Close();
                }

                //senaste bmi
                double weight = 0;
                double bmi = 0;
                using (SqlCommand wCommand = new SqlCommand())
                {
                     wCommand.Connection = connection;
                     wCommand.CommandText = "select top 1 vikt from Mätningar where namn = @name order by datum desc";
                     wCommand.Parameters.AddWithValue("@name", name);
                     
                     connection.Open();
                     try
                     {
                         weight = Convert.ToDouble(wCommand.ExecuteScalar());
                         bmi = weight / ((length / 100.0) * (length / 100.0));
                         Console.WriteLine("Bmi: " + bmi);
                     }
                     catch (Exception e)
                     {
                        Console.WriteLine("Kan ej beräkna bmi då viktmätningar saknas");
                     }
                     connection.Close();
                }
                        
                //alla mätningar för personen
                using (SqlCommand command3 = new SqlCommand())
                {

                     command3.Connection = connection;
                     command3.CommandText = "select * from Mätningar where namn = @name";
                     command3.Parameters.AddWithValue("@name", name);
                     connection.Open();
                     SqlDataReader reader2 = command3.ExecuteReader();
                     if (reader2.HasRows)
                     {
                           while (reader2.Read())
                           {
                                 DateTime mesdate = Convert.ToDateTime(reader2["Datum"]);
                                 weight = Convert.ToDouble(reader2["vikt"]);
                                 Console.WriteLine(mesdate.ToShortDateString() + ", " + weight);
                           }
                     }
                     reader2.Close();
                     connection.Close();
                }
                        
                //c - mata in ny mätning, kolla först om personen behöver sparas
                string insertperson;
                int insertweight;
                Console.WriteLine("Person? ");
                insertperson = Console.ReadLine();
                Console.WriteLine("Vikt? ");
                insertweight = int.Parse(Console.ReadLine());

                using (SqlCommand iCommand = new SqlCommand())
                {
                    iCommand.Connection=connection;
                    iCommand.CommandText = "select namn from Personer where namn = @name";
                    iCommand.Parameters.AddWithValue("@name", insertperson);
                    connection.Open();
                    string foundname = (string)iCommand.ExecuteScalar();
                    connection.Close();
                    if (foundname==null)
                    {
                        Console.Write("Nya personens längd i cm? ");
                        int insertlength = int.Parse(Console.ReadLine());
                        iCommand.CommandText = "insert into Personer(namn, längd) values (@name, @length)";
                        iCommand.Parameters.AddWithValue("@length", insertlength);
                        connection.Open();
                        iCommand.ExecuteNonQuery(); //borde ha try catch
                        connection.Close();
                    }
                }

                using(SqlCommand command4 = new SqlCommand())
                {
                    command4.Connection = connection;
                    command4.CommandText = "insert into Mätningar values (@person, @mesdate, @weight)";
                    command4.Parameters.AddWithValue("@person", insertperson);
                    command4.Parameters.AddWithValue("@mesdate", DateTime.Today);
                    command4.Parameters.AddWithValue("@weight", insertweight);
                    connection.Open();
                    try
                    {
                        command4.ExecuteNonQuery();
                    }
                    catch(SqlException e)
                    {
                        Console.WriteLine("FEL --- kund int spara");
                        Console.WriteLine(e.Message);
                        connection.Close();
                        Console.ReadKey();
                        return;
                    }
                    catch(Exception e)
                    {
                        Console.WriteLine("FEL --- någå fo fel");
                        Console.WriteLine(e.Message);
                        connection.Close();
                        Console.ReadKey();
                        return;
                    }
                    Console.WriteLine("allt gick bra");
                }
            }           
            Console.ReadKey();
        }
    }
}


