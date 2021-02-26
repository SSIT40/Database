using System;
using System.Configuration;
using System.Data.Common;

//using System.Data.SqlClient;


namespace Ovning1
{
    class Program
    {

        static void Main()
        {

            string myconnection = ConfigurationManager.AppSettings["connection"];
            string myprovider = ConfigurationManager.AppSettings["Provider"];
            DbProviderFactory factory = DbProviderFactories.GetFactory(myprovider);


            Console.WriteLine("Number? ");
            int nr = Convert.ToInt32(Console.ReadLine());

            //öppna förbindelse till databasen
            using (DbConnection connection = factory.CreateConnection())
            {
                connection.ConnectionString = myconnection;

                Console.WriteLine("databasen öppnad...");

                string sql = "select namn, rekord from Deltagare where nr = @nr";

                using (DbCommand command1 = factory.CreateCommand())
                {
                    command1.Connection = connection;
                    command1.CommandText = sql;
                    //command1.Parameters.AddWithValue("@nr", nr);
                    DbParameter par = factory.CreateParameter();      // för att koda provider oberoende kod
                    par.ParameterName = "@nr";
                    par.Value = nr;
                    command1.Parameters.Add(par);

                    connection.Open();
                    DbDataReader reader = command1.ExecuteReader();
                    if (reader.HasRows)
                    {
                        reader.Read();
                        string name = reader["namn"].ToString();
                        double bestresult = Convert.ToDouble(reader["rekord"]);

                        Console.WriteLine("Person with nr " + nr + " is: ");
                        Console.WriteLine(name + " with a personal best of: " + bestresult);
                        reader.Close();

                        command1.CommandText = "select * from Resultat where deltagarnr = @nr";
                        reader = command1.ExecuteReader();
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                //int resultmbr = Convert.ToInt32(reader[""])
                                double result = Convert.ToDouble(reader["resultat"]);

                                Console.WriteLine(result);
                            }
                        }
                        else
                        {
                            Console.WriteLine("inga tidigare resultat");
                        }
                        reader.Close();
                        connection.Close();

                        //////////////////////////////////////////////////////////////////////////////////
                        ///// mata in resultat för personen
                        ///

                        Console.WriteLine("New result? ");

                        double newresult = Convert.ToDouble(Console.ReadLine());
                        //  Behöver också 'int nr'!!!

                        DbTransaction transaction;
                        command1.CommandText = "select max(resultatnr) from Resultat";
                        sql = "insert into Resultat (resultatnr, deltagarnr, resultat) values (@resnr, @deltnr, @result)";

                        //SqlCommand insertcommand = new SqlCommand(sql, connection);
                        DbCommand insertcommand = factory.CreateCommand();              // för att koda provider oberoende kod
                        insertcommand.Connection = connection;
                        insertcommand.CommandText = sql;

                        // @resnr e tom variabel
                                //insertcommand.Parameters.Add(new SqlParameter("@resnr", System.Data.SqlDbType.Int));
                        DbParameter resnrpar = factory.CreateParameter();
                        resnrpar.ParameterName = "@resnr";
                        insertcommand.Parameters.Add(resnrpar);

                                //insertcommand.Parameters.AddWithValue("@deltnr", nr);
                        DbParameter deltnrpar = factory.CreateParameter();
                        deltnrpar.ParameterName = "@deltnr";
                        deltnrpar.Value = nr;
                        insertcommand.Parameters.Add(deltnrpar);

                                //insertcommand.Parameters.AddWithValue("@result", newresult);
                        DbParameter resultpar = factory.CreateParameter();
                        resultpar.ParameterName = "@result";
                        resultpar.Value = newresult;
                        insertcommand.Parameters.Add(resultpar);


                        sql = "update Deltagare set rekord = @best where nr = @nr";
                        DbCommand updatecommand = factory.CreateCommand();
                        updatecommand.Connection = connection;
                        updatecommand.CommandText = sql;

                        DbParameter bestpar = factory.CreateParameter();
                        bestpar.ParameterName = "@best";
                        bestpar.Value = newresult;
                        updatecommand.Parameters.Add(bestpar);

                        DbParameter nrpar = factory.CreateParameter();
                        nrpar.ParameterName = "@nr";
                        nrpar.Value = nr;
                        updatecommand.Parameters.Add(nrpar);


                        connection.Open();
                        transaction = connection.BeginTransaction();
                        try
                        {
                            command1.Transaction = transaction;
                            insertcommand.Transaction = transaction;
                            updatecommand.Transaction = transaction;

                            //nästa lediga resultat nummer
                            int resnr = (int)command1.ExecuteScalar();
                            resnr++;

                            command1.CommandText = "select rekord from Deltagare where nr = @nr";
                            double best = Convert.ToDouble(command1.ExecuteScalar());

                            insertcommand.Parameters[0].Value = resnr;
                            insertcommand.ExecuteNonQuery();

                            //uppdatera rekord
                            if (newresult > best)
                            {
                                updatecommand.ExecuteNonQuery();
                            }
                            transaction.Commit();   //      COMMIT
                            Console.WriteLine("Resultatet uppdaterat!");

                            if (newresult > best)
                            {
                                Console.WriteLine("Nytt rekord!");
                            }
                        }
                        catch (Exception e)
                        {
                            transaction.Rollback();
                            Console.WriteLine("FEL -- kunde inte spara");
                            Console.WriteLine(e.Message);
                        }
                        connection.Close();

                    }
                    else
                    {
                        Console.WriteLine("ingen sådan deltagare");
                        reader.Close();
                        connection.Close();

                    }

                }              
            }


            Console.ReadKey();
        }
    }
}


