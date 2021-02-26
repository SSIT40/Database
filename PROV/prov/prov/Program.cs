using System;
using System.Data.SqlClient;
using System.Configuration;
using System.Data.Common;
using System.Data;

namespace prov
{
    class Program
    {
        static void Main(string[] args)
        {

            Console.WriteLine("reg nbr? ");
            string regnr = Console.ReadLine().ToString();

            string connstring = ConfigurationManager.AppSettings["carconnection"];
            string myprovider = ConfigurationManager.AppSettings["Provider"];
            DbProviderFactory factory = DbProviderFactories.GetFactory(myprovider);

            string service;
            DateTime sdate;
            string brand;
            string model;
            int yearmodel;
            int price = 0;

            using (SqlConnection connection = new SqlConnection())
            {
                connection.ConnectionString = connstring;
                string sql = "select * from Bilar where regnr = @regnr";

                using (DbCommand command1 = factory.CreateCommand())
                {
                    command1.Connection = connection;
                    command1.CommandText = sql;

                    DbParameter par = factory.CreateParameter();
                    par.ParameterName = "@regnr";
                    par.Value = regnr;
                    command1.Parameters.Add(par);

                    connection.Open();
                    DbDataReader reader = command1.ExecuteReader();
                    if (reader.HasRows)
                    {
                        reader.Read();
                        brand = reader["Märke"].ToString();
                        model = reader["Modell"].ToString();
                        yearmodel = Convert.ToInt32(reader["Årsmodell"]);

                        Console.WriteLine(regnr + " is a: ");
                        Console.WriteLine(brand + " - " + model + " - " + yearmodel);
                        Console.WriteLine();
                        reader.Close();

                        command1.CommandText = "select * from Service where regnr = @regnr";
                        reader = command1.ExecuteReader();
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                service = reader["Service"].ToString();
                                sdate = Convert.ToDateTime(reader["Datum"]);
                                sql = "select Pris from ServiceTyper Where service = @service";

                                DbCommand command2 = factory.CreateCommand();
                                command2.Connection = connection;
                                command2.CommandText = sql;

                                DbParameter pricepar = factory.CreateParameter();
                                pricepar.ParameterName = "@service";
                                pricepar.Value = service;
                                command2.Parameters.Add(pricepar);

                                DbDataReader reader2 = command2.ExecuteReader();                                
                                if (reader2.HasRows)
                                {
                                    reader2.Read();
                                    price = Convert.ToInt32(reader2["Pris"]);
                                }
                                Console.WriteLine(service + ": " + sdate + " Price: " + price);
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("Ingen sådan bil");
                    }
                    reader.Close();
                    connection.Close();

                    string newservice;
                    Console.WriteLine("Service? ");
                    newservice = Console.ReadLine();
                    DateTime indate = DateTime.Today;

                    string sqlin = "insert into Service(Regnr, Service, Datum) values(@regnr, @service, @indate)";
                    using (DbCommand insertcommand = factory.CreateCommand())
                    {
                        insertcommand.Connection = connection;
                        insertcommand.CommandText = sqlin;

                        DbTransaction transaction;
                        DbParameter regnrpar = factory.CreateParameter();
                        regnrpar.ParameterName = "@regnr";
                        regnrpar.Value = regnr;
                        insertcommand.Parameters.Add(regnrpar);

                        while (newservice != "")
                        {                           
                            DbParameter servicepar = factory.CreateParameter();
                            servicepar.ParameterName = "@service";
                            servicepar.Value = newservice;
                            insertcommand.Parameters.Add(servicepar);

                            DbParameter datepar = factory.CreateParameter();
                            datepar.ParameterName = "@indate";
                            datepar.Value = indate;
                            insertcommand.Parameters.Add(datepar);

                            newservice = "";
                            Console.WriteLine("Service? ");
                            newservice = Console.ReadLine();            

                            ////  Här vet ja inte vad som skall göras för att den skall godkänna flera inmatningar före det sparar
                            ///
                        }

                        connection.Open();
                        transaction = connection.BeginTransaction();
                        try
                        {
                            insertcommand.Transaction = transaction;
                            insertcommand.ExecuteNonQuery();
                            transaction.Commit();

                            Console.WriteLine("Serviceåtgärderna sparade!");
                        }
                        catch (Exception e)
                        {
                            transaction.Rollback();
                            Console.WriteLine("”FEL – inga serviceåtgärder sparade!");
                            Console.WriteLine(e.Message);
                        }
                        connection.Close();
                    }
                }
            }
            Console.ReadKey();
        }
    }
}
