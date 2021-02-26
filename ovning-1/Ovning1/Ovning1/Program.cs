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

        /*
            - sqlParameter - för att infoga data till sql-kommandon på ett säkert sätt
            - egenskaper: ParameterName, Value, DataType
            - metoder: commandobject.Parameters.Add()
                       commandobject.Parameters.AddWithValue(...)
        */
        static void Main()
        {


            string myconnection = ConfigurationManager.AppSettings["personconnection"];

            //öppna förbindelse till databasen
            using (SqlConnection connection = new SqlConnection())
            {
                connection.ConnectionString = myconnection;
                connection.Open();
                Console.WriteLine("databasen öppnad...");

                using (SqlCommand command1 = new SqlCommand())
                {
                    command1.Connection = connection;
                    command1.CommandText = "select count(*) from Personer";
                    int numofpersons = Convert.ToInt32(command1.ExecuteScalar());
                    Console.WriteLine("Antalet personer: " + numofpersons);
                }
                connection.Close();

                //läs data om alla Personer
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
                            string person = reader["namn"].ToString();
                            int längd = Convert.ToInt32(reader[1]);

                            Console.WriteLine(person + ", " + längd);
                        }
                        reader.Close();
                    }
                    connection.Close();
                }


                ////////////////////////////////////////////////////////////////
                ///         BBBBBBBBBB
                ///         //////////////////////////////////////////////
                ///         alla mätningar, längd, senaste bmi
                                
                Console.WriteLine("Person? ");
                string name = Console.ReadLine();
                int length;

                using (SqlCommand command3L = new SqlCommand())
                {

                    //select * from Personer where namn is 'name 
                    command3L.Connection = connection;
                    command3L.CommandText = "select Längd from Personer where namn = @name";

                    command3L.Parameters.AddWithValue("@name", name);

                    connection.Open();

                    length = (int)command3L.ExecuteScalar();
                    if (length > 0)
                    {
                        Console.WriteLine("längden är " + length);
                        using (SqlCommand wCommand = new SqlCommand())
                        {
                            wCommand.Connection = connection;
                            wCommand.CommandText = "select top 1 vikt from Mätningar where namn = @name order by datum order by datum desc";
                            wCommand.Parameters.AddWithValue("@name", name);
                            int weight = (int)wCommand.ExecuteScalar();       
                            if (weight > 0)
                            {
                                double bmi = weight / ((length * 100) * (length * 100));
                                Console.WriteLine("Bmi: " + bmi);
                            }
                        }
                    }
                    connection.Close();
                }

                    // alla mätningsr
                using (SqlCommand command3 = new SqlCommand())
                {

                    //select * from Personer where namn is 'name 
                    command3.Connection = connection;
                    command3.CommandText = "select * from Mätningar where namn = @name";
                    /*SqlParameter namepar = new SqlParameter
                    {
                        ParameterName = "@name",
                        Value = name
                    };
                    command3.Parameters.Add(namepar);*/
                    command3.Parameters.AddWithValue("@name", name);

                    connection.Open();

                    SqlDataReader reader2 = command3.ExecuteReader();
                    if(reader2.HasRows)
                    {
                        while(reader2.Read())
                        {
                            DateTime mesdate = Convert.ToDateTime(reader2["Datum"]);
                            double weight = Convert.ToDouble(reader2["vikt"]);

                            Console.WriteLine(mesdate.ToShortDateString() + ", " + weight);
                        }
                    }
                    reader2.Close();
                    connection.Close();
                }

                //  Att skriva till databaser dvs insert, update, delete

                //ex - lagra ett nytt mätvärde - insert till Mätningar

                ////////////////////////////////////////
                ///     CCCCCCCCCC
                ///     ////////////////////////////////
                ///     

                string insertperson;
                int insertweight;
                Console.WriteLine("Person? ");
                insertperson = Console.ReadLine();
                Console.WriteLine("Vikt? ");
                insertweight = int.Parse(Console.ReadLine());


                using (SqlCommand iCommand = new SqlCommand())
                {
                    iCommand.Connection = connection;
                    iCommand.CommandText = "select * from Personer where namn = @name";
                    iCommand.Parameters.AddWithValue("@name", insertperson);
                    connection.Open();
                    string foundname = (string)iCommand.ExecuteScalar();
                    connection.Close();
                    if(foundname == "")
                    {
                        Console.WriteLine("Nya Personens längd? ");
                        int insertlength = int.Parse(Console.ReadLine());
                        iCommand.CommandText = "insert into Persons value (@name, @length)";
                        iCommand.Parameters.AddWithValue("@length", insertlength);
                        connection.Open();
                        iCommand.ExecuteNonQuery();     //borde ha try catch
                        connection.Close();
                    }
                }



                using (SqlCommand command4 = new SqlCommand())
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
                    catch (SqlException e)
                    {
                        Console.WriteLine("FEL --- kund int spara");
                        Console.WriteLine(e.Message);
                        connection.Close();
                        Console.ReadKey();
                        return;
                    }
                    catch (Exception e)
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


